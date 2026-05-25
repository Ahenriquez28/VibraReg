using Microsoft.EntityFrameworkCore;
using RegistrationService.Data;
using RegistrationService.Data.Entities;
using RegistrationService.DTOs;
using Amazon.S3;
using Amazon.S3.Transfer;
using Amazon.S3.Model;

namespace RegistrationService.Services
{
    public class RegistrationService : IRegistrationService
    {
        private readonly RegistrationDbContext _dbContext;
        private readonly IConfiguration _configuration;
        private readonly IAmazonS3 _s3Client;
        private readonly string _bucketName;
        private readonly string _publicUrl;
        private readonly IEmailService _emailService;

        public RegistrationService(
            RegistrationDbContext dbContext,
            IConfiguration configuration,
            IAmazonS3 s3Client,
            IEmailService emailService)
        {
            _dbContext = dbContext;
            _configuration = configuration;
            _s3Client = s3Client;
            _bucketName = configuration["R2:BucketName"] ?? throw new InvalidOperationException("R2 bucket name not configured");
            _publicUrl = configuration["R2:PublicUrl"] ?? throw new InvalidOperationException("R2 public URL not configured");
            _emailService = emailService;
        }

        // Helper method to ensure TeamId 404 exists
        private async Task<RegisteredTeams> EnsureUnassignedTeamExists()
        {
            var unassignedTeam = await _dbContext.RegisteredTeams
                .FirstOrDefaultAsync(t => t.TeamId == 404);

            if (unassignedTeam == null)
            {
                unassignedTeam = new RegisteredTeams
                {
                    TeamId = 404,
                    GroupName = "Unassigned",
                    TeamFull = false,
                    Id = "",
                    CreatedAt = DateTime.UtcNow
                };

                _dbContext.RegisteredTeams.Add(unassignedTeam);
                await _dbContext.SaveChangesAsync();
                
                Console.WriteLine("Created default Unassigned team with TeamId 404");
            }

            return unassignedTeam;
        }

        public async Task<RegisteredUser> RegisterAsync(RegisterDTO dto)
    {
        using var transaction = await _dbContext.Database.BeginTransactionAsync();
        
        try
        {
            // Check for duplicate email
            var existingUser = await _dbContext.RegisteredUsers
                .FirstOrDefaultAsync(u => u.Email == dto.Email);

            if (existingUser != null)
                throw new InvalidOperationException("This email is already registered.");

            // Check team logic before doing anything else
            if (dto.HasGroup)
            {
                var team = await _dbContext.RegisteredTeams
                    .FirstOrDefaultAsync(t => t.GroupName == dto.GroupName);

                if (team != null)
                {
                    if (team.TeamFull)
                        throw new InvalidOperationException("This team is already full.");

                    var currentIds = string.IsNullOrEmpty(team.Id)
                        ? Array.Empty<string>()
                        : team.Id.Split(',');

                    if (currentIds.Length >= 4)
                    {
                        team.TeamFull = true;
                        _dbContext.RegisteredTeams.Update(team);
                        await _dbContext.SaveChangesAsync();
                        throw new InvalidOperationException("This team is already full (max 4 members).");
                    }
                }
            }

            // Save resume to Cloudflare R2 bucket
            string? resumePath = null;
            if (dto.Resume != null)
            {
                resumePath = await SaveResumeAsync(dto.Resume);
            }

            // Create the user
            var registration = new RegisteredUser
            {
                FullName = dto.FullName,
                Email = dto.Email,
                School = dto.School,
                Gpa = string.IsNullOrWhiteSpace(dto.Gpa) ? "Not Provided" : dto.Gpa,
                HasGroup = dto.HasGroup,
                GroupName = dto.GroupName,
                ResumePath = resumePath,
                Status = "registered",
                CreatedAt = DateTime.UtcNow,
                ConfirmationToken = Guid.NewGuid().ToString(),
                ConfirmationDeadline = new DateTime(2026, 4, 1, 17, 0, 0, DateTimeKind.Local)
            };

            _dbContext.RegisteredUsers.Add(registration);
            await _dbContext.SaveChangesAsync();

            // Handle team assignment
            if (dto.HasGroup)
            {
                var team = await _dbContext.RegisteredTeams
                    .FirstOrDefaultAsync(t => t.GroupName == dto.GroupName);

                if (team == null)
                {
                    // Team doesnt exist yet — create it
                    team = new RegisteredTeams
                    {
                        GroupName = dto.GroupName!,
                        TeamFull = false,
                        Id = "",
                        CreatedAt = DateTime.UtcNow
                    };
                    _dbContext.RegisteredTeams.Add(team);
                    await _dbContext.SaveChangesAsync();
                }

                var studentId = registration.Id.ToString();

                if (string.IsNullOrEmpty(team.Id))
                    team.Id = studentId;
                else
                    team.Id += "," + studentId;

                team.TeamFull = team.Id.Split(',').Length >= 4;

                _dbContext.RegisteredTeams.Update(team);
                await _dbContext.SaveChangesAsync();

                registration.TeamId = team.TeamId;
                _dbContext.RegisteredUsers.Update(registration);
                await _dbContext.SaveChangesAsync();

                Console.WriteLine($"Student {registration.Id} assigned to team {team.TeamId} ({team.GroupName})");
            }
            else
            {
                // No team — assign to the unassigned pool
                var noTeamOption = await EnsureUnassignedTeamExists();

                var studentId = registration.Id.ToString();

                if (string.IsNullOrEmpty(noTeamOption.Id))
                    noTeamOption.Id = studentId;
                else
                    noTeamOption.Id += "," + studentId;

                _dbContext.RegisteredTeams.Update(noTeamOption);

                registration.TeamId = noTeamOption.TeamId;
                _dbContext.RegisteredUsers.Update(registration);

                await _dbContext.SaveChangesAsync();
            }

            // Commit everything — only hits this line if nothing above threw
            await transaction.CommitAsync();

            Console.WriteLine("Registration complete");

            // Send confirmation email
            if (dto.HasGroup && !string.IsNullOrEmpty(dto.GroupName))
            {
                await _emailService.SendRegistrationEmailWithTeam(
                    registration.Email,
                    registration.FullName,
                    dto.GroupName
                );
            }
            else
            {
                await _emailService.SendRegistrationEmailNoTeam(
                    registration.Email,
                    registration.FullName
                );
            }

            return registration;
        }
        catch (InvalidOperationException)
        {
            // User error — roll back any partial saves and re-throw
            // The controller will catch this and return 400
            await transaction.RollbackAsync();
            throw;
        }
        catch (Exception ex)
        {
            // Unexpected server error — roll back and re-throw
            await transaction.RollbackAsync();
            Console.WriteLine($"Registration failed unexpectedly: {ex.Message}");
            throw;
        }
    }

        public async Task<string> SaveResumeAsync(IFormFile file)
        {
            var allowedExtensions = new[] { ".pdf", ".doc", ".docx", ".png", ".jpg", ".jpeg" };
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

            if (!allowedExtensions.Contains(extension))
                throw new InvalidOperationException("Invalid file type");

            // Generate unique filename
            var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
            var sanitizedFileName = Path.GetFileNameWithoutExtension(file.FileName)
                .Replace(" ", "_")
                .Replace(",", "");
            var uniqueFileName = $"resumes/{timestamp}_{sanitizedFileName}{extension}";

            try
            {
                using var memoryStream = new MemoryStream();
                await file.CopyToAsync(memoryStream);
                memoryStream.Position = 0;

                var putRequest = new PutObjectRequest
                {
                    BucketName = _bucketName,
                    Key = uniqueFileName,
                    InputStream = memoryStream,
                    ContentType = file.ContentType,
                    CannedACL = S3CannedACL.PublicRead,
                    DisablePayloadSigning = false,
                    UseChunkEncoding = false  // Disable chunked encoding for R2
                };

                await _s3Client.PutObjectAsync(putRequest);

                // Return public URL
                var publicUrl = $"{_publicUrl}/{uniqueFileName}";
                Console.WriteLine($"Resume uploaded successfully: {publicUrl}");
                
                return publicUrl;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"R2 upload failed: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                throw new InvalidOperationException("Failed to upload resume to R2", ex);
            }
        }

        public async Task<List<AdminTeamDto>> GetTeams()
        {
            await EnsureUnassignedTeamExists();

            var teams = await _dbContext.RegisteredTeams
                .Include(t => t.Students)
                .ToListAsync();

            var teamDtos = teams.Select(team => new AdminTeamDto
            {
                TeamId = team.TeamId,
                GroupName = team.GroupName,
                TeamFull = team.TeamFull,
                Students = team.Students.Select(s => new AdminStudentDto
                {
                    Id = s.Id,
                    FullName = s.FullName,
                    Email = s.Email,
                    School = s.School,
                    Gpa = s.Gpa,
                    HasGroup = s.TeamId != null && s.TeamId != 404,
                    ResumePath = s.ResumePath,
                    IsPresent = s.IsPresent,
                    Status = s.Status
                }).ToList()
            }).ToList();

            var unassignedStudents = await _dbContext.RegisteredUsers
                .Where(u => u.TeamId == null)
                .ToListAsync();

            if (unassignedStudents.Any())
            {
                var existingUnassigned = teamDtos.FirstOrDefault(t => t.TeamId == 404);
                if (existingUnassigned != null)
                {
                    var legacyStudents = unassignedStudents.Select(s => new AdminStudentDto
                    {
                        Id = s.Id,
                        FullName = s.FullName,
                        Email = s.Email,
                        School = s.School,
                        Gpa = s.Gpa,
                        HasGroup = false,
                        ResumePath = s.ResumePath,
                        IsPresent = s.IsPresent

                    }).ToList();

                    existingUnassigned.Students.AddRange(legacyStudents);
                }
            }

            return teamDtos;
        }

        public async Task UpdateTeamAssignments(UpdateTeamsDTO dto)
        {
            using var transaction = await _dbContext.Database.BeginTransactionAsync();

            try
            {
                foreach (var assignment in dto.Assignments)
                {
                    var student = await _dbContext.RegisteredUsers
                        .FirstOrDefaultAsync(u => u.Id == assignment.StudentId);

                    if (student == null) continue;

                    if (student.TeamId.HasValue)
                    {
                        var oldTeam = await _dbContext.RegisteredTeams
                            .FirstOrDefaultAsync(t => t.TeamId == student.TeamId);

                        if (oldTeam != null)
                        {
                            var ids = oldTeam.Id?.Split(',').ToList() ?? new();
                            ids.Remove(student.Id.ToString());
                            oldTeam.Id = string.Join(",", ids);
                            oldTeam.TeamFull = ids.Count >= 4;
                            _dbContext.RegisteredTeams.Update(oldTeam);
                        }
                    }

                    if (assignment.TeamId.HasValue && assignment.TeamId > 0)
                    {
                        var newTeam = await _dbContext.RegisteredTeams
                            .FirstOrDefaultAsync(t => t.TeamId == assignment.TeamId);

                        if (newTeam != null)
                        {
                            var ids = newTeam.Id?.Split(',').ToList() ?? new();
                            if (!ids.Contains(student.Id.ToString()))
                                ids.Add(student.Id.ToString());

                            newTeam.Id = string.Join(",", ids);
                            newTeam.TeamFull = ids.Count >= 4;
                            student.TeamId = assignment.TeamId;

                            _dbContext.RegisteredTeams.Update(newTeam);
                        }
                    }
                    else
                    {
                        var unassignedTeam = await EnsureUnassignedTeamExists();
                        student.TeamId = 404;
                        
                        var ids = unassignedTeam.Id?.Split(',').ToList() ?? new();
                        if (!ids.Contains(student.Id.ToString()))
                            ids.Add(student.Id.ToString());
                        
                        unassignedTeam.Id = string.Join(",", ids);
                        _dbContext.RegisteredTeams.Update(unassignedTeam);
                    }

                    _dbContext.RegisteredUsers.Update(student);
                }

                await _dbContext.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task RemoveStudents(UpdateTeamsDTO dto)
        {
            using var transaction = await _dbContext.Database.BeginTransactionAsync();

            try
            {
                foreach (var assignment in dto.Assignments)
                {
                    var student = await _dbContext.RegisteredUsers
                        .FirstOrDefaultAsync(u => u.Id == assignment.StudentId);

                    if (student == null) continue;

                    if (student.TeamId.HasValue)
                    {
                        var oldTeam = await _dbContext.RegisteredTeams
                            .FirstOrDefaultAsync(t => t.TeamId == student.TeamId);
                        
                        if (oldTeam != null)
                        {
                            var ids = oldTeam.Id?.Split(',').ToList() ?? new();
                            ids.Remove(student.Id.ToString());
                            oldTeam.Id = string.Join(",", ids);
                            oldTeam.TeamFull = ids.Count >= 4;
                            _dbContext.RegisteredTeams.Update(oldTeam);
                        }
                    }

                    _dbContext.RegisteredUsers.Remove(student);
                }

                await _dbContext.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task RemoveTeam(RegisteredTeams badTeam)
        {
            using var transaction = await _dbContext.Database.BeginTransactionAsync();

            try
            {
                var unassignedTeam = await EnsureUnassignedTeamExists();

                if (!string.IsNullOrWhiteSpace(badTeam.Id))
                {
                    var existingIds = unassignedTeam.Id?.Split(',', StringSplitOptions.RemoveEmptyEntries) ?? Array.Empty<string>();
                    var incomingIds = badTeam.Id.Split(',', StringSplitOptions.RemoveEmptyEntries);

                    unassignedTeam.Id = string.Join(",", existingIds.Concat(incomingIds).Distinct());

                    var studentIds = incomingIds.Select(int.Parse).ToList();
                    var students = await _dbContext.RegisteredUsers
                        .Where(s => studentIds.Contains(s.Id))
                        .ToListAsync();

                    foreach (var student in students)
                    {
                        student.TeamId = 404;
                        _dbContext.RegisteredUsers.Update(student);
                    }

                    _dbContext.RegisteredTeams.Update(unassignedTeam);
                }

                _dbContext.RegisteredTeams.Remove(badTeam);

                await _dbContext.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task CreateTeam (RegisteredTeams newTeam)
        {

            var team = new RegisteredTeams
            {
                GroupName = newTeam.GroupName,
                TeamFull = false,
                CreatedAt = DateTime.UtcNow
            };
            _dbContext.RegisteredTeams.Add(team);
            await _dbContext.SaveChangesAsync();

            return;
        }

        public async Task<List<string>> GetTeamNamesAsync()
        {
            return await _dbContext.RegisteredTeams
                .Where(t => t.TeamId != 404)        // exclude unassigned
                .Where(t => t.TeamFull == false)    // only show teams with space
                .Select(t => t.GroupName)           
                .ToListAsync();
        }
    }
}
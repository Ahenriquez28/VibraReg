using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using RegistrationService.DTOs;
using RegistrationService.Services;
using RegistrationService.Data.Entities;
using RegistrationService.Data;
using Microsoft.EntityFrameworkCore;
using System.Net;



namespace RegistrationService.Controllers
{
    [ApiController]
    [Route("api")]
    public class RegistrationController : ControllerBase
    {
        private readonly IRegistrationService _registrationService;
        private readonly RegistrationDbContext _dbContext;
        private readonly IEmailService _emailService;


        public RegistrationController(
            IRegistrationService registrationService,
            RegistrationDbContext dbContext,
            IEmailService emailService)
        {
            _registrationService = registrationService;
            _dbContext = dbContext;
            _emailService = emailService;
        }

        // POST: api/register
        [HttpPost("register")]
        public async Task<IActionResult> RegisterAsync([FromForm] RegisterDTO dto)
        {
            // Get authenticated user info from JWT token
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var username = User.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value;
            var totalStudents = await _dbContext.RegisteredUsers.CountAsync();
            if (totalStudents >= 150)
            {
                throw new InvalidOperationException("Registration is full. Maximum 150 students allowed.");
            }
            
            try
            {
                //We are sending the resume to CloudFlare 
                // if (dto.Resume != null)
                // {
                //     var relativePath = await _registrationService.SaveResumeAsync(dto.Resume);
                // }

                var result = await _registrationService.RegisterAsync(dto);
                return Ok(new
                {
                    success = true,
                    message = "Registration successful",
                    registrationId = result.Id,
                    registeredBy = username
                });
            }

            catch (Exception ex)
            {
                //Status 500 means error in our loginc
                return StatusCode(500, new
                {
                    success = false,
                    message = "Registration failed",
                    exceptionMessage = ex.Message,
                    stackTrace = ex.StackTrace,
                    innerException = ex.InnerException != null ? new {
                        message = ex.InnerException.Message,
                        stackTrace = ex.InnerException.StackTrace
                    } : null
                });
            }
        }

        [HttpGet("getTeams")]
        [AllowAnonymous]  // ← Public - no auth needed
        public async Task<IActionResult> GetTeams()
        {
            try
            {
                var teamData = await _registrationService.GetTeams();
                
                return Ok(new
                {
                    success = true,
                    teams = teamData
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Failed to fetch teams",
                    error = ex.Message
                });
            }
        }

        [HttpPost("updateTeams")]
        [Authorize]  // ← Requires JWT token
        public async Task<IActionResult> UpdateTeams([FromBody] UpdateTeamsDTO dto) 
        {
            try
            {
                await _registrationService.UpdateTeamAssignments(dto);
                
                return Ok(new
                {
                    success = true,
                    message = "Teams updated successfully"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Failed to update teams",
                    error = ex.Message
                });
            }
        }

        [HttpPost("removeStudents")]
        [Authorize]  // ← Requires JWT token
        public async Task<IActionResult> RemoveStudents([FromBody] UpdateTeamsDTO dto)
        {
            try
            {
                await _registrationService.RemoveStudents(dto);
                
                return Ok(new
                {
                    success = true,
                    message = "Student removed successfully"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Failed to remove student",
                    error = ex.Message
                });
            }
        }

        [HttpPost("removeTeam")]
        [Authorize]  // ← Requires JWT token
        public async Task<IActionResult> RemoveTeam([FromBody] RegisteredTeams badTeam)
        {
            try
            {
                await _registrationService.RemoveTeam(badTeam);
                
                return Ok(new
                {
                    success = true,
                    message = "Team removed successfully"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Failed to remove team",
                    error = ex.Message
                });
            }
        }

        [HttpPost("createTeam")]
        [Authorize]  // ← Requires JWT token
        public async Task<IActionResult> CreateTeam([FromBody] RegisteredTeams newTeam)
        {
            try
            {
                await _registrationService.CreateTeam(newTeam);
                
                return Ok(new
                {
                    success = true,
                    message = "Team created successfully"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Failed to create team",
                    error = ex.Message
                });
            }
        }

        [HttpPost("togglePresent")]
        [Authorize]
        public async Task<IActionResult> TogglePresent([FromBody] TogglePresentDTO dto)
        {
            try
            {
                var student = await _dbContext.RegisteredUsers
                    .FirstOrDefaultAsync(s => s.Id == dto.StudentId);

                if (student == null)
                {
                    return NotFound(new { success = false, message = "Student not found" });
                }

                student.IsPresent = dto.IsPresent;
                await _dbContext.SaveChangesAsync();

                return Ok(new
                {
                    success = true,
                    message = "Attendance updated",
                    isPresent = student.IsPresent
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Failed to update attendance",
                    error = ex.Message
                });
            }
        }

        //Email Section from here and bottom

        [HttpGet("confirm/{token}")]
        public async Task<IActionResult> ConfirmationAttendance(string token)
        {
            var student = await _dbContext.RegisteredUsers
                .FirstOrDefaultAsync(s => s.ConfirmationToken == token); // Are we saying they have a token already and were watingin for them to give it back?

            if (student == null)  // ← Add null check!
            {
                return NotFound(new
                {
                    success = false,
                    message = "Invalid confirmation token"
                });
            }

            if (student.Status == "confirmed")
            {
                return Ok(new
                {
                    success = true,
                    message = "You're already confirmed"
                });

            }

            if (student.ConfirmationDeadline < DateTime.UtcNow)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Sorry deadline has passed, please reach out to SHPE.gastate@gmail.com to see if there is more space!"
                });
            }

            //Confirmation of the student
            student.Status = "confirmed";
            student.ConfirmedAt = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync();

            return Ok(new
            {
                success= true,
                message="You are confirmed! See yo at Vibra ATL Hackathon!",
                confirmedAt = student.ConfirmedAt
            });

        }

        [HttpPost("send-confirmation-emails")]
        public async Task<IActionResult> SendConfirmationEmails()
        {
            var students = await _dbContext.RegisteredUsers
                .Where(s => s.Status == "registered"
                    && s.ConfirmationSentAt ==null)
                .ToListAsync();
        
            var emailsSent = 0;
            var errors = new List<string>();

            foreach (var student in students)
            {
                try
                {
                    await _emailService.SendConfirmationEmailAsync(
                        student.Email,
                        student.FullName,
                        student.ConfirmationToken ?? ""
                    );

                    student.ConfirmationSentAt = DateTime.UtcNow;
                    emailsSent++;
                }
                catch (Exception ex)
                {
                    errors.Add($"Failed to send to {student.Email}: {ex.Message}");
                }
            }
            await _dbContext.SaveChangesAsync();

            return Ok(new
            {
                success = true,
                emailsSent,
                totalStudents = students.Count,
                errors
            });
        }

        [HttpPost("send-deadline-reminders")]
        public async Task<IActionResult> SendDeadlineReminders()
        {
            var students = await _dbContext.RegisteredUsers
                .Where(s => s.Status == "registered")  // Still not confirmed
                .ToListAsync();
            
            var emailsSent = 0;
            var errors = new List<string>();

            foreach (var student in students)
            {
                try
                {
                    await _emailService.SendDeadlineReminderAsync(
                        student.Email,
                        student.FullName,
                        student.ConfirmationToken ?? ""
                    );
                    
                    emailsSent++;
                }
                catch (Exception ex)
                {
                    errors.Add($"Failed to send to {student.Email}: {ex.Message}");
                }
            }
            
            return Ok(new 
            { 
                success = true,
                emailsSent,
                totalStudents = students.Count,
                errors
            });
        }

        // Mark unconfirmed students as "removed" after deadline
        [HttpPost("cleanup-unconfirmed")]
        public async Task<IActionResult> CleanupUnconfirmed()
        {
            var now = DateTime.UtcNow;
            
            var unconfirmed = await _dbContext.RegisteredUsers
                .Where(s => s.Status == "registered" 
                        && s.ConfirmationDeadline < now)
                .ToListAsync();
            
            foreach (var student in unconfirmed)
            {
                student.Status = "removed";  // Changed from "registered" to "removed"
            }
            
            await _dbContext.SaveChangesAsync();
            
            return Ok(new 
            { 
                success = true,
                removedCount = unconfirmed.Count,
                message = $"Removed {unconfirmed.Count} unconfirmed students"
            });
        }
    }


}
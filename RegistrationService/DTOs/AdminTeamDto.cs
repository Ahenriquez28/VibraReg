// DTOs/AdminTeamDto.cs
namespace RegistrationService.DTOs
{
    public class AdminTeamDto
    {
        public int TeamId { get; set; }
        public string GroupName { get; set; } = string.Empty;
        public bool TeamFull { get; set; }
        public List<AdminStudentDto> Students { get; set; } = new();
    }

    public class AdminStudentDto
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string School { get; set; } = string.Empty;
        public string Gpa { get; set; } = string.Empty;
        public bool HasGroup { get; set; }
        public string? ResumePath { get; set; }

        public bool IsPresent { get; set; } = false;
        public string Status { get; set; }

    }
}
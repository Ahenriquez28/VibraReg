using System.Dynamic;

namespace RegistrationService.Data.Entities
{
    public class RegisteredUser
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string School { get; set; } = string.Empty;
        public string? Gpa { get; set; } = string.Empty;
        public bool HasGroup { get; set; }
        public string? GroupName { get; set; }
        public string? ResumePath { get; set; }
        public int? TeamId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsPresent { get; set; } = false;

        public string Status {get; set; } = "registered";
        public DateTime? ConfirmedAt {get; set;}
        public string? ConfirmationToken {get; set;}
        public DateTime? ConfirmationSentAt {get; set;}
        public DateTime? ConfirmationDeadline{ get; set;}
    }

    public class TogglePresentDTO
    {
        public int StudentId { get; set;}
        public bool IsPresent {get; set;}
    }
}
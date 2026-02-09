using Microsoft.AspNetCore.Http;

namespace VibraApiGateway.DTOs
{
    public class RegisterDTO
    {
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string School { get; set; } = string.Empty;
        public string? Gpa { get; set; } = string.Empty;
        public bool HasGroup { get; set; }
        public string? GroupName { get; set; }  // NEW
        public List<string>? GroupMembers { get; set; }
        public IFormFile? Resume { get; set; }
    }
}
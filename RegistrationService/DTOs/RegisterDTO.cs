// DTOs/RegisterDTO.cs
using Microsoft.AspNetCore.Http;

namespace RegistrationService.DTOs
{
    public class RegisterDTO
    {
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string School { get; set; } = string.Empty;
        public string? Gpa { get; set; } = string.Empty;
        public bool HasGroup { get; set; }
        public string? GroupName { get; set; }
        public IFormFile? Resume { get; set; }
        public bool IsPresent { get; set; } = false;

    }
}
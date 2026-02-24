// DTOs/UpdateTeamsDTO.cs
namespace RegistrationService.DTOs
{
    public class UpdateTeamsDTO
    {
        public List<TeamAssignment> Assignments { get; set; } = new();
    }

    public class TeamAssignment
    {
        public int StudentId { get; set; }
        public int? TeamId { get; set; }  // Null means unassigned
    }
}
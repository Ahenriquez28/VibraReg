using RegistrationService.Data.Entities;
using RegistrationService.DTOs;  // ← Make sure this is here

namespace RegistrationService.Services
{
    public interface IRegistrationService
    {
        Task<RegisteredUser> RegisterAsync(RegisterDTO dto);
        Task<List<AdminTeamDto>> GetTeams();
        Task UpdateTeamAssignments(UpdateTeamsDTO dto);
        Task<string> SaveResumeAsync(IFormFile file);
        Task RemoveStudents(UpdateTeamsDTO dto);
        Task RemoveTeam(RegisteredTeams badTeam);
        Task CreateTeam(RegisteredTeams newTeam);

    }
}
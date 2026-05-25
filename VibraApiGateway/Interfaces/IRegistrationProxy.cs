using VibraApiGateway.DTOs;

namespace VibraApiGateway.Interfaces
{
    public interface IRegistrationProxy
    {
        Task<object> RegisterAsync(RegisterDTO dto);
        Task<object> GetTeamsAsync(string? authToken);  
        Task<object> UpdateTeamsAsync(object dto, string authToken);  
        Task<object> RemoveStudentsAsync(object dto, string authToken);  
        Task<object> RemoveTeamAsync(object dto, string authToken);  
        Task<object> CreateTeamAsync(object dto, string authToken);
        Task<object> TogglePresentAsync(object dto, string authToken);  // ✅ Add this
        Task<object> ConfirmAttendanceAsync(string token);
        Task<object> GetTeamNamesAsync(); 
        Task<object> TriggerConfirmationEmailsAsync();
  
    }
}
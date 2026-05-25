using VibraApiGateway.DTOs;
using VibraApiGateway.Interfaces;
using System.Net.Http.Headers;
using System.Text.Json;

namespace VibraApiGateway.Proxies
{
    public class RegistrationProxy : IRegistrationProxy
    {
        private readonly HttpClient _httpClient;
        private readonly string _registrationServiceUrl;
        
        public RegistrationProxy(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _registrationServiceUrl = configuration["Services:RegistrationService"] 
                ?? "http://registration-service:5001";
        }
        
        public async Task<object> RegisterAsync(RegisterDTO dto)
        {
            var formData = new MultipartFormDataContent();
            
            formData.Add(new StringContent(dto.FullName), "FullName");
            formData.Add(new StringContent(dto.Email), "Email");
            formData.Add(new StringContent(dto.School), "School");
            if (!string.IsNullOrWhiteSpace(dto.Gpa))
            {
                formData.Add(new StringContent(dto.Gpa), "Gpa");
            }
            formData.Add(new StringContent(dto.HasGroup.ToString()), "HasGroup");

            if (!string.IsNullOrEmpty(dto.GroupName))
            {
                formData.Add(new StringContent(dto.GroupName), "GroupName"); 
            }

            if (dto.Resume != null)
            {
                var fileContent = new StreamContent(dto.Resume.OpenReadStream());
                fileContent.Headers.ContentType =
                    new MediaTypeHeaderValue(
                        string.IsNullOrEmpty(dto.Resume.ContentType) ? "application/octet-stream" : dto.Resume.ContentType
                    );

                var safeFileName = Path.GetFileName(dto.Resume.FileName).Replace("\"", "");
                var contentDisposition = new ContentDispositionHeaderValue("form-data")
                {
                    Name = "\"Resume\"",
                    FileName = $"\"{safeFileName}\""
                };

                fileContent.Headers.ContentDisposition = contentDisposition;
                formData.Add(fileContent, "Resume", safeFileName);
            }

            var response = await _httpClient.PostAsync("/api/register", formData);
            
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                return new 
                { 
                    success = false, 
                    message = $"Registration failed with status {response.StatusCode}: {errorContent}" 
                };
            }
            
            return await response.Content.ReadFromJsonAsync<object>() ?? new { success = false };
        }

        public async Task<object> GetTeamsAsync(string? authToken)
        {
            if (!string.IsNullOrEmpty(authToken))
            {
                _httpClient.DefaultRequestHeaders.Authorization = 
                    new AuthenticationHeaderValue("Bearer", authToken);
            }
            
            var response = await _httpClient.GetAsync("/api/getTeams");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<object>();
        }

        public async Task<object> UpdateTeamsAsync(object dto, string authToken)
        {
            _httpClient.DefaultRequestHeaders.Authorization = 
                new AuthenticationHeaderValue("Bearer", authToken);
            
            var response = await _httpClient.PostAsJsonAsync("/api/updateTeams", dto);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<object>();
        }

        public async Task<object> RemoveStudentsAsync(object dto, string authToken)
        {
            _httpClient.DefaultRequestHeaders.Authorization = 
                new AuthenticationHeaderValue("Bearer", authToken);
            
            var response = await _httpClient.PostAsJsonAsync("/api/removeStudents", dto);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<object>();
        }

        public async Task<object> RemoveTeamAsync(object dto, string authToken)
        {
            _httpClient.DefaultRequestHeaders.Authorization = 
                new AuthenticationHeaderValue("Bearer", authToken);
            
            var response = await _httpClient.PostAsJsonAsync("/api/removeTeam", dto);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<object>();
        }

        public async Task<object> CreateTeamAsync(object dto, string authToken)
        {
            _httpClient.DefaultRequestHeaders.Authorization = 
                new AuthenticationHeaderValue("Bearer", authToken);
            
            var response = await _httpClient.PostAsJsonAsync("/api/createTeam", dto);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<object>();
        }

        public async Task<object> TogglePresentAsync(object dto, string authToken)
        {
            _httpClient.DefaultRequestHeaders.Authorization = 
                new AuthenticationHeaderValue("Bearer", authToken);
            
            var response = await _httpClient.PostAsJsonAsync("/api/togglePresent", dto);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<object>();
        }

        public async Task<object> ConfirmAttendanceAsync(string token)
        {
            var response = await _httpClient.GetAsync($"/api/confirm/{token}");
            
            if (!response.IsSuccessStatusCode)
            {
                return new { success = false, message = "Failed to confirm attendance" };
            }
            
            return await response.Content.ReadFromJsonAsync<object>() ?? new { success = false };
        }

        public async Task<object> GetTeamNamesAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_registrationServiceUrl}/api/team-names");
                var content = await response.Content.ReadAsStringAsync();
                
                if (!response.IsSuccessStatusCode)
                {
                    return new { success = false, message = "Failed to fetch team names" };
                }
                
                return JsonSerializer.Deserialize<object>(content) ?? new { success = false };
            }
            catch (Exception ex)
            {
                return new { success = false, message = ex.Message };
            }
        }

        public async Task<object> TriggerConfirmationEmailsAsync()
        {
            
            var response = await _httpClient.PostAsync("/api/trigger-confirmation-emails", null);
            
            if (!response.IsSuccessStatusCode)
            {
                return new { success = false, message = "Failed to trigger confirmation emails" };
            }
            
            return await response.Content.ReadFromJsonAsync<object>() ?? new { success = false };
        }
    }
}
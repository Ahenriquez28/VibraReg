using VibraApiGateway.Interfaces;

namespace VibraApiGateway.Proxies;

public class AuthProxy : IAuthProxy
{
    private readonly HttpClient _httpClient;

    public AuthProxy(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<HttpResponseMessage> LoginAsync(LoginRequest request)
    {
        return await _httpClient.PostAsJsonAsync("/api/auth/login", request);
    }

    public async Task<HttpResponseMessage> RegisterAsync(RegisterRequest request)
    {
        return await _httpClient.PostAsJsonAsync("/api/auth/register", request);
    }

    public async Task<HttpResponseMessage> ValidateTokenAsync(string token)
    {
        return await _httpClient.PostAsJsonAsync("/api/auth/validate", token);
    }
}
using Microsoft.AspNetCore.Mvc;
using VibraApiGateway.Interfaces;

namespace VibraApiGateway.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthProxy _authProxy;

    public AuthController(IAuthProxy authProxy)
    {
        _authProxy = authProxy;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var response = await _authProxy.LoginAsync(request);
        var content = await response.Content.ReadAsStringAsync();
        
        if (response.IsSuccessStatusCode)
        {
            return Ok(content);
        }
        
        return StatusCode((int)response.StatusCode, content);
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var response = await _authProxy.RegisterAsync(request);
        var content = await response.Content.ReadAsStringAsync();
        
        if (response.IsSuccessStatusCode)
        {
            return Ok(content);
        }
        
        return StatusCode((int)response.StatusCode, content);
    }

    [HttpPost("validate")]
    public async Task<IActionResult> ValidateToken([FromBody] string token)
    {
        var response = await _authProxy.ValidateTokenAsync(token);
        var content = await response.Content.ReadAsStringAsync();
        
        if (response.IsSuccessStatusCode)
        {
            return Ok(content);
        }
        
        return Unauthorized(content);
    }
}
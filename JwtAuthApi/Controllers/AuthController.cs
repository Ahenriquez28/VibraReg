using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using JwtAuthApi.Models;
using JwtAuthApi.Services;
using JwtAuthApi.Data;

namespace JwtAuthApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly JwtTokenService _jwtTokenService;
    private readonly ApplicationDbContext _context;
    private readonly ILogger<AuthController> _logger;

    //Dependency Injections
    public AuthController(
        JwtTokenService jwtTokenService, 
        ApplicationDbContext context,
        ILogger<AuthController> logger)
    {
        _jwtTokenService = jwtTokenService;
        _context = context;
        _logger = logger;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        if (string.IsNullOrEmpty(request.Username) || string.IsNullOrEmpty(request.Password))
        {
            return BadRequest(new LoginResponse
            {
                Success = false,
                Message = "Username and password are required"
            });
        }

        // Find all authenticated Usernames
        var user = await _context.AuthUsers
            .FirstOrDefaultAsync(u => u.Username == request.Username && u.IsActive);
        
        if (user == null)
        {
            _logger.LogWarning("Login attempt failed: User {Username} not found in authUsers", request.Username);
            return Unauthorized(new LoginResponse
            {
                Success = false,
                Message = "Invalid username or password"
            });
        }

        // Hash the incoming password with BCrypt, then checked it with the existing hashed password in the table
        bool isValidPassword = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);
        
        if (!isValidPassword)
        {
            _logger.LogWarning("Login attempt failed: Invalid password for user {Username}", request.Username);
            return Unauthorized(new LoginResponse
            {
                Success = false,
                Message = "Invalid username or password"
            });
        }

        // Updating our user's last login time
        user.LastLoginAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        // Generate JWT token
            //Checking for multiple rows in var roles
        var roles = user.Roles.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList();
        var token = _jwtTokenService.GenerateToken(
            user.Username, 
            user.Id.ToString(), 
            roles
        );
        
        var expiresAt = _jwtTokenService.GetTokenExpiration();

        _logger.LogInformation("User {Username} logged in successfully from authUsers", user.Username);

        return Ok(new LoginResponse
        {
            Success = true,
            Token = token,
            Message = "Login successful",
            ExpiresAt = expiresAt
        });
    }

    /// <summary>
    /// Register a new user in authUsers table
    /// </summary>
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        if (string.IsNullOrEmpty(request.Username) || string.IsNullOrEmpty(request.Password))
        {
            return BadRequest(new { Message = "Username and password are required" });
        }

        if (request.Password.Length < 6)
        {
            return BadRequest(new { Message = "Password must be at least 6 characters" });
        }

        // Check if username already exists in authUsers
        var existingUser = await _context.AuthUsers
            .FirstOrDefaultAsync(u => u.Username == request.Username || u.Email == request.Email);
        
        if (existingUser != null)
        {
            return BadRequest(new { Message = "Username or email already exists" });
        }

        // Create new user with hashed password
        var newUser = new User
        {
            Username = request.Username,
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            FirstName = request.FirstName,
            LastName = request.LastName,
            Roles = "User",
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        _context.AuthUsers.Add(newUser);
        await _context.SaveChangesAsync();

        _logger.LogInformation("New user registered in authUsers: {Username}", newUser.Username);

        return Ok(new { Message = "User registered successfully", UserId = newUser.Id });
    }

    /// <summary>
    /// Validates a token - can be called by external APIs
    /// </summary>
    [HttpPost("validate")]
    public IActionResult ValidateToken([FromBody] string token)
    {
        var principal = _jwtTokenService.ValidateToken(token);
        
        if (principal == null)
        {
            return Unauthorized(new { Message = "Invalid or expired token" });
        }

        var username = principal.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value;
        var userId = principal.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

        return Ok(new
        {
            Valid = true,
            Username = username,
            UserId = userId,
            Message = "Token is valid"
        });
    }
}

public class RegisterRequest
{
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
}
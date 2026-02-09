using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using JwtAuthApi.Models;

namespace JwtAuthApi.Services;

public class JwtTokenService
{
    private readonly JwtSettings _jwtSettings;

    public JwtTokenService(JwtSettings jwtSettings)
    {
        _jwtSettings = jwtSettings;
    }

    //
    public string GenerateToken(string username, string userId, List<string>? roles = null)
    {
        //JWT Tokens are broken down into 3 sections.
        //1. Header
        //2.Payload
        //3. Signature

    //1 & 3. Header and Singature: This part is for creating the signature and header
        //Encode the secret key into bytes, then wrap the byte into a key object in the JWT libary that is regonized
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));

        //We are starting to configure the token here. We are saying: "I my token's alogrithm will be Hmac
        // and my token type is JWT. Lasly, secruity key will be used for my signature later 
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

    //2. Payload: What is this token saying 
        //Our token's payload will show, username, userId, and a unique tokenID
        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, username),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.NameIdentifier, userId),
            new Claim(ClaimTypes.Name, username)
        };

        // Add roles if provided
        if (roles != null)
        {
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }
        }

        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer, //webapp issuing this
            audience: _jwtSettings.Audience, //who is the token for 
            claims: claims, //payload
            expires: DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationMinutes),
            signingCredentials: credentials //instructions for signature
        );

        //Create the token
        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    /// <summary>
    /// Validates a JWT token and returns the claims principal
    /// This can be called from external APIs to verify the token
    /// </summary>
    public ClaimsPrincipal? ValidateToken(string token)
    {
        //JwtSecuirtyTokenHandler is a built in .NET class to validate JWTs
        var tokenHandler = new JwtSecurityTokenHandler();
        //Key prepping by creating it as an object
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));

        try
        {
            var principal = tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = _jwtSettings.Issuer,
                ValidAudience = _jwtSettings.Audience,
                IssuerSigningKey = securityKey, //did we issue the singing key 
                ClockSkew = TimeSpan.Zero // Remove default 5 minute tolerance
            }, out SecurityToken validatedToken);

            return principal;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Gets the expiration time for tokens
    /// </summary>
    public DateTime GetTokenExpiration()
    {
        return DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationMinutes);
    }
}
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Options;
using src.API.Configuration;
using src.Core.Services;
using src.Core.Entities;

namespace src.IntegrationTests.Helpers;

/// <summary>
/// Helper for authentication-related functionality in tests
/// </summary>
public class AuthenticationHelper
{
    private readonly AuthService _authService;
    private readonly string _secretKey = "this-is-a-very-long-secret-key-for-testing-purposes-minimum-32-chars";
    private readonly string _issuer = "TestIssuer";
    private readonly string _audience = "TestAudience";

    public AuthenticationHelper()
    {
        var jwtConfig = new JwtConfig
        {
            Key = "ThisIsAVeryLongSecretKeyForTestingPurposes12345678901234567890",
            Issuer = "TestIssuer",
            Audience = "TestAudience",
            ExpiryInMinutes = 60
        };

        var jwtOptions = Options.Create(jwtConfig);
        _authService = new AuthService(null!, jwtOptions);
    }

    /// <summary>
    /// Generates a JWT token for testing purposes
    /// </summary>
    /// <param name="adminId">Admin ID</param>
    /// <param name="username">Username</param>
    /// <param name="expirationMinutes">Token expiration in minutes (default: 60)</param>
    /// <returns>JWT token string</returns>
    public string GenerateJwtToken(int adminId, string username, int expirationMinutes = 60)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_secretKey);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, adminId.ToString()),
                new Claim(ClaimTypes.Name, username),
                new Claim("AdminId", adminId.ToString()),
                new Claim("Username", username)
            }),
            Expires = DateTime.UtcNow.AddMinutes(expirationMinutes),
            Issuer = _issuer,
            Audience = _audience,
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    /// <summary>
    /// Validates a JWT token structure (for testing token format)
    /// </summary>
    /// <param name="token">JWT token to validate</param>
    /// <returns>True if token has valid structure</returns>
    public bool ValidateTokenStructure(string token)
    {
        if (string.IsNullOrEmpty(token))
            return false;

        var parts = token.Split('.');
        return parts.Length == 3; // Header.Payload.Signature
    }

    /// <summary>
    /// Extracts claims from a JWT token without validation (for testing)
    /// </summary>
    /// <param name="token">JWT token</param>
    /// <returns>Dictionary of claims</returns>
    public Dictionary<string, string> ExtractClaims(string token)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var jsonToken = tokenHandler.ReadJwtToken(token);

        return jsonToken.Claims.ToDictionary(
            claim => claim.Type,
            claim => claim.Value);
    }

    /// <summary>
    /// Creates an invalid JWT token for negative testing
    /// </summary>
    /// <returns>Invalid JWT token</returns>
    public string GenerateInvalidToken()
    {
        return "invalid.jwt.token";
    }

    /// <summary>
    /// Creates an expired JWT token for testing
    /// </summary>
    /// <param name="adminId">Admin ID</param>
    /// <param name="username">Username</param>
    /// <returns>Expired JWT token</returns>
    public string GenerateExpiredToken(int adminId, string username)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_secretKey);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, adminId.ToString()),
                new Claim(ClaimTypes.Name, username)
            }),
            Expires = DateTime.UtcNow.AddMinutes(-1), // Expired 1 minute ago
            Issuer = _issuer,
            Audience = _audience,
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    public string GenerateJwtToken(int adminId = 1, string username = "testadmin")
    {
        return _authService.GenerateJwtToken(adminId, username);
    }

    public Dictionary<string, string> GetAuthHeaders(int adminId = 1, string username = "testadmin")
    {
        var token = GenerateJwtToken(adminId, username);
        return new Dictionary<string, string>
        {
            ["Authorization"] = $"Bearer {token}"
        };
    }
}
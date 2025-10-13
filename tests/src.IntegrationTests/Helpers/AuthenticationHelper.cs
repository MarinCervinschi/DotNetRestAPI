using Microsoft.Extensions.Options;
using src.API.Configuration;
using src.Core.Services;
using src.Core.Entities;

namespace src.IntegrationTests.Helpers;

public class AuthenticationHelper
{
    private readonly AuthService _authService;

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

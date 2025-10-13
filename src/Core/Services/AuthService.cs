using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using src.API.Configuration;
using src.API.DTOs;
using src.Core.Interfaces.Repositories;
using src.Core.Interfaces.Services;

namespace src.Core.Services;

public class AuthService(IAdminRepository adminRepository, IOptions<JwtConfig> jwtConfig)
    : IAuthService
{
    private readonly JwtConfig _jwtConfig = jwtConfig.Value;

    public async Task<LoginResponseDto?> LoginAsync(AdminLoginDto loginDto)
    {
        var admin = await adminRepository.GetByUsernameAsync(loginDto.Username);
        if (admin == null || !VerifyPassword(loginDto.Password, admin.PasswordHash))
            return null;

        var token = GenerateJwtToken(admin.Id, admin.Username);

        return new LoginResponseDto
        {
            Token = token,
            Admin = new AdminDto
            {
                Id = admin.Id,
                Username = admin.Username,
                Email = admin.Email
            }
        };
    }

    public string GenerateJwtToken(int adminId, string username)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtConfig.Key));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, adminId.ToString()),
            new Claim(ClaimTypes.Name, username)
        };

        var token = new JwtSecurityToken(
            issuer: _jwtConfig.Issuer,
            audience: _jwtConfig.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_jwtConfig.ExpiryInMinutes),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public bool VerifyPassword(string password, string passwordHash)
    {
        if (string.IsNullOrEmpty(passwordHash))
            return false;
            
        return BCrypt.Net.BCrypt.Verify(password, passwordHash);
    }
}
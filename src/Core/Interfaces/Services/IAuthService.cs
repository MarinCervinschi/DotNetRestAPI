using src.API.DTOs;

namespace src.Core.Interfaces.Services;

public interface IAuthService
{
    Task<LoginResponseDto?> LoginAsync(AdminLoginDto loginDto);
    string GenerateJwtToken(int adminId, string username);
    bool VerifyPassword(string password, string passwordHash);
}
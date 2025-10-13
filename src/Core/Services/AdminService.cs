using src.API.DTOs;
using src.Core.Entities;
using src.Core.Interfaces.Repositories;
using src.Core.Interfaces.Services;

namespace src.Core.Services;

public class AdminService(IAdminRepository adminRepository) : IAdminService
{
    public async Task<AdminDto?> GetByUsernameAsync(string username)
    {
        var admin = await adminRepository.GetByUsernameAsync(username);
        if (admin == null) return null;

        return new AdminDto
        {
            Id = admin.Id,
            Username = admin.Username,
            Email = admin.Email
        };
    }

    public async Task<AdminDto> CreateAdminAsync(string username, string email, string password)
    {
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(password);

        var admin = new Admin
        {
            Username = username,
            Email = email,
            PasswordHash = passwordHash
        };

        await adminRepository.CreateAsync(admin);

        return new AdminDto
        {
            Id = admin.Id,
            Username = admin.Username,
            Email = admin.Email
        };
    }
}

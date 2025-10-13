using src.API.DTOs;
using src.Core.Entities;

namespace src.Core.Interfaces.Services;

public interface IAdminService
{
    Task<AdminDto?> GetByUsernameAsync(string username);
    Task<AdminDto> CreateAdminAsync(string username, string email, string password);
}

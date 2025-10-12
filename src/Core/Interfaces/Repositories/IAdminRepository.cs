using src.Core.Entities;

namespace src.Core.Interfaces.Repositories;

public interface IAdminRepository : IRepository<Admin>
{
    Task<Admin?> GetByUsernameAsync(string username);
}
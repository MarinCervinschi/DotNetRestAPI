using Microsoft.EntityFrameworkCore;
using src.Core.Entities;
using src.Core.Interfaces.Repositories;
using src.Infrastructure.Data;

namespace src.Infrastructure.Repositories;

public class AdminRepository(ApplicationDbContext context) : Repository<Admin>(context), IAdminRepository
{
    public async Task<Admin?> GetByUsernameAsync(string username)
    {
        return await Context.Set<Admin>()
            .FirstOrDefaultAsync(a => a.Username == username);
    }
}

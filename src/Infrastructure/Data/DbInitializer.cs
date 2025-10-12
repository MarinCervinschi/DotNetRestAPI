using Microsoft.EntityFrameworkCore;
using src.Core.Entities;

namespace src.Infrastructure.Data;

public static class DbInitializer
{
    public static async Task SeedAsync(ApplicationDbContext context)
    {
        if (await context.Admins.AnyAsync())
            return;

        var defaultAdmin = new Admin
        {
            Username = "admin",
            Email = "admin@dotnetrestapi.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123")
        };

        context.Admins.Add(defaultAdmin);
        await context.SaveChangesAsync();
    }
}

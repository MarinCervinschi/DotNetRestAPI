using Microsoft.EntityFrameworkCore;
using src.Core.Entities;

namespace src.Infrastructure.Data.Seeding;

public class DataSeeder(ILogger<DataSeeder> logger) : IDataSeeder
{
    public async Task SeedAsync(ApplicationDbContext context, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Starting data seeding...");

        await SeedAdminsAsync(context, cancellationToken);
        await SeedBooksAsync(context, cancellationToken);
        await SeedCustomersAsync(context, cancellationToken);

        logger.LogInformation("Data seeding completed successfully");
    }

    private async Task SeedAdminsAsync(ApplicationDbContext context, CancellationToken cancellationToken)
    {
        if (await context.Admins.AnyAsync(cancellationToken))
        {
            logger.LogInformation("Admins already exist, skipping admin seeding");
            return;
        }

        var admins = new[]
        {
            new Admin
            {
                Username = "admin",
                Email = "admin@dotnetrestapi.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123")
            },
            new Admin
            {
                Username = "superadmin",
                Email = "superadmin@dotnetrestapi.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("super123")
            }
        };

        context.Admins.AddRange(admins);
        await context.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Seeded {Count} admins", admins.Length);
    }

    private async Task SeedBooksAsync(ApplicationDbContext context, CancellationToken cancellationToken)
    {
        if (await context.Books.AnyAsync(cancellationToken))
        {
            logger.LogInformation("Books already exist, skipping book seeding");
            return;
        }

        var books = new[]
        {
            new Book { Title = "Clean Code", Author = "Robert C. Martin", ISBN = "9780132350884" },
            new Book { Title = "Design Patterns", Author = "Gang of Four", ISBN = "9780201633612" },
            new Book { Title = "Refactoring", Author = "Martin Fowler", ISBN = "9780201485677" },
            new Book { Title = "The Pragmatic Programmer", Author = "Andrew Hunt", ISBN = "9780201616224" },
            new Book { Title = "Code Complete", Author = "Steve McConnell", ISBN = "9780735619678" }
        };

        context.Books.AddRange(books);
        await context.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Seeded {Count} books", books.Length);
    }

    private async Task SeedCustomersAsync(ApplicationDbContext context, CancellationToken cancellationToken)
    {
        if (await context.Customers.AnyAsync(cancellationToken))
        {
            logger.LogInformation("Customers already exist, skipping customer seeding");
            return;
        }

        var customers = new[]
        {
            new Customer { FirstName = "Mario", LastName = "Rossi", Email = "mario.rossi@email.com" },
            new Customer { FirstName = "Luigi", LastName = "Bianchi", Email = "luigi.bianchi@email.com" },
            new Customer { FirstName = "Anna", LastName = "Verdi", Email = "anna.verdi@email.com" }
        };

        context.Customers.AddRange(customers);
        await context.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Seeded {Count} customers", customers.Length);
    }
}
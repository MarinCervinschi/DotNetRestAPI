namespace src.Infrastructure.Data.Seeding;

public interface IDataSeeder
{
    Task SeedAsync(ApplicationDbContext context, CancellationToken cancellationToken = default);
}
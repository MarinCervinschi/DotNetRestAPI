using Microsoft.EntityFrameworkCore;

namespace src.Infrastructure.Data.Seeding;

public class SeedCommand(IServiceProvider serviceProvider, ILogger<SeedCommand> logger)
    : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var args = Environment.GetCommandLineArgs();

        if (args.Contains("--seed"))
        {
            logger.LogInformation("Explicit seeding command detected");

            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var seeder = scope.ServiceProvider.GetRequiredService<IDataSeeder>();

            await seeder.SeedAsync(context, cancellationToken);

            logger.LogInformation("Seeding completed. Shutting down...");
            Environment.Exit(0);
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
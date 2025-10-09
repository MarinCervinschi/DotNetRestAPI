using Microsoft.EntityFrameworkCore;
using DotNetEnv;
using src.Infrastructure.Data;

namespace src.API.Configuration;

public static class DatabaseConfig
{
    public static string GetConnectionString()
    {
        Env.Load("../.env");

        var dbHost = Environment.GetEnvironmentVariable("DB_HOST") ?? "localhost";
        var dbPort = Environment.GetEnvironmentVariable("DB_PORT") ?? "5432";
        var dbName = Environment.GetEnvironmentVariable("DB_NAME") ?? "dotnetrestapi";
        var dbUser = Environment.GetEnvironmentVariable("DB_USER") ?? "postgres";
        var dbPassword = Environment.GetEnvironmentVariable("DB_PASSWORD") ?? "password";

        return $"Host={dbHost};Port={dbPort};Database={dbName};Username={dbUser};Password={dbPassword}";
    }

    public static IServiceCollection AddDatabaseConfiguration(this IServiceCollection services)
    {
        var connectionString = GetConnectionString();
        Console.WriteLine(connectionString);
        services.AddDbContext<ApplicationDbContext>(options =>
        {
            options.UseNpgsql(connectionString);

            // Development-specific configurations
            if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development")
            {
                options.EnableSensitiveDataLogging();
                options.EnableDetailedErrors();
            }
        });

        return services;
    }

    public static async Task<IServiceProvider> EnsureDatabaseCreated(this IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        try
        {
            await context.Database.MigrateAsync();
            Console.WriteLine("✅ Database migrations applied successfully");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error applying database migrations: {ex.Message}");
            throw;
        }

        return serviceProvider;
    }
}
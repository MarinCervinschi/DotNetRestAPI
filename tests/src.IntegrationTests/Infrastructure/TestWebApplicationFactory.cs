using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using src.Infrastructure.Data;

namespace src.IntegrationTests.Infrastructure;

public class TestWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _databaseName = Guid.NewGuid().ToString();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // Set environment to Testing FIRST
        builder.UseEnvironment("Testing");

        // Set environment variable explicitly to ensure DatabaseConfig sees it
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Testing");

        builder.ConfigureServices(services =>
        {
            // Remove ALL existing database registrations
            var dbContextDescriptor =
                services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));
            if (dbContextDescriptor != null)
                services.Remove(dbContextDescriptor);

            var dbContextServiceDescriptor =
                services.SingleOrDefault(d => d.ServiceType == typeof(ApplicationDbContext));
            if (dbContextServiceDescriptor != null)
                services.Remove(dbContextServiceDescriptor);

            // Add in-memory database for testing with unique name
            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseInMemoryDatabase(_databaseName);
                options.ConfigureWarnings(warnings =>
                {
                    // Ignore all InMemory warnings, including transaction warnings
                    warnings.Ignore(InMemoryEventId.TransactionIgnoredWarning);
                });
            });

            // Ensure the database is created
            var serviceProvider = services.BuildServiceProvider();
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            context.Database.EnsureCreated();
        });
    }

    public ApplicationDbContext GetDbContext()
    {
        var scope = Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        return context;
    }

    public async Task<ApplicationDbContext> GetDbContextAsync()
    {
        var context = GetDbContext();
        await context.Database.EnsureCreatedAsync();
        return context;
    }

    public async Task CleanupDatabaseAsync()
    {
        using var scope = Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        // Clear all entities from the database
        if (context.Admins.Any())
        {
            context.Admins.RemoveRange(context.Admins);
        }

        if (context.Customers.Any())
        {
            context.Customers.RemoveRange(context.Customers);
        }

        if (context.Books.Any())
        {
            context.Books.RemoveRange(context.Books);
        }

        if (context.Reservations.Any())
        {
            context.Reservations.RemoveRange(context.Reservations);
        }

        await context.SaveChangesAsync();

        // Reset identity counters for InMemory database
        context.ChangeTracker.Clear();
    }
}
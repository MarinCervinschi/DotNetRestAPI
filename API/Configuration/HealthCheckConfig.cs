using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace DotNetRestAPI.API.Configuration;

public static class HealthCheckConfig
{
    public static IServiceCollection AddHealthChecksConfiguration(this IServiceCollection services)
    {
        var connectionString = DatabaseConfig.GetConnectionString();

        services.AddHealthChecks()
            .AddNpgSql(connectionString, name: "postgresql")
            .AddCheck("api", () =>
                HealthCheckResult.Healthy("API is running"));

        return services;
    }

    public static void MapHealthCheckEndpoints(this WebApplication app)
    {
        app.MapHealthChecks("/health");

        app.MapHealthChecks("/health/db", new HealthCheckOptions
        {
            Predicate = check => check.Name == "postgresql"
        });

        app.MapHealthChecks("/health/api", new HealthCheckOptions
        {
            Predicate = check => check.Name == "api"
        });
    }
}
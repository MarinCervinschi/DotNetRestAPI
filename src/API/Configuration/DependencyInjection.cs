using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using src.Core.Interfaces;
using src.Core.Interfaces.Repositories;
using src.Core.Interfaces.Services;
using src.Core.Services;
using src.Infrastructure.Data.Seeding;
using src.Infrastructure.Repositories;

namespace src.API.Configuration;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

        services.AddScoped<IReservationRepository, ReservationRepository>();
        services.AddScoped<IAdminRepository, AdminRepository>();

        services.AddScoped<ICustomerService, CustomerService>();
        services.AddScoped<IBookService, BookService>();
        services.AddScoped<IReservationService, ReservationService>();
        services.AddScoped<IAdminService, AdminService>();
        services.AddScoped<IAuthService, AuthService>();

        services.AddHostedService<ReservationExpirationService>();

        return services;
    }

    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services)
    {
        services.AddScoped<IDataSeeder, DataSeeder>();
        services.AddHostedService<SeedCommand>();

        return services;
    }

    public static IServiceCollection AddApiServices(this IServiceCollection services)
    {
        services.AddControllers();
        services.AddOpenApi();
        services.ConfigureSwagger();
        services.AddLogging();

        return services;
    }

    public static IServiceCollection AddJwtAuthentication(this IServiceCollection services,
        IConfiguration configuration)
    {
        var jwtConfig = new JwtConfig();
        configuration.GetSection("Jwt").Bind(jwtConfig);
        services.Configure<JwtConfig>(configuration.GetSection("Jwt"));

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtConfig.Issuer,
                    ValidAudience = jwtConfig.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtConfig.Key))
                };
            });

        return services;
    }
}

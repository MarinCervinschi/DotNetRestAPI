using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
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
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo { Title = "DotNet REST API", Version = "v1" });

            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.ApiKey,
                Scheme = "Bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "Insert JWT with Bearer into field (Example: 'Bearer {token}')"
            });

            c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
                    },
                    []
                }
            });
        });
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
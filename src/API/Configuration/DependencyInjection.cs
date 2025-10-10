using src.Core.Interfaces;
using src.Core.Interfaces.Repositories;
using src.Core.Interfaces.Services;
using src.Core.Services;
using src.Infrastructure.Repositories;

namespace src.API.Configuration;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        
        services.AddScoped<IReservationRepository, ReservationRepository>();

        services.AddScoped<ICustomerService, CustomerService>();
        services.AddScoped<IBookService, BookService>();
        services.AddScoped<IReservationService, ReservationService>();

        services.AddHostedService<ReservationExpirationService>();

        return services;
    }

    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services)
    {
        // Here you can add infrastructure related services, like DbContext, etc.
        return services;
    }

    public static IServiceCollection AddApiServices(this IServiceCollection services)
    {
        services.AddControllers();
        services.AddOpenApi();
        services.AddSwaggerGen();
        services.AddLogging();


        return services;
    }
}
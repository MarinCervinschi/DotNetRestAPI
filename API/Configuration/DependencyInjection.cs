using DotNetRestAPI.Core.Interfaces;
using DotNetRestAPI.Core.Interfaces.Services;
using DotNetRestAPI.Core.Services;
using DotNetRestAPI.Infrastructure.Repositories;

namespace DotNetRestAPI.API.Configuration;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

        services.AddScoped<ICustomerService, CustomerService>();
        // services.AddScoped<IBookService, BookService>();
        // services.AddScoped<IReservationService, ReservationService>();

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
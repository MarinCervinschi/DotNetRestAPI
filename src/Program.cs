using src.API.Configuration;
using src.API.Middleware;
using src.Infrastructure.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDatabaseConfiguration();
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices();
builder.Services.AddApiServices();
builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddHealthChecksConfiguration();

var app = builder.Build();

if (!app.Environment.IsEnvironment("Testing"))
{
    await app.Services.EnsureDatabaseCreated();

    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await DbInitializer.SeedAsync(context);
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Middleware - ORDER IS IMPORTANT
app.UseGlobalExceptionHandling();
app.UseRequestLogging();
//app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHealthCheckEndpoints();

app.Run();

public abstract partial class Program
{
}
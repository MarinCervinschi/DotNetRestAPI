using src.API.Configuration;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDatabaseConfiguration();
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices();
builder.Services.AddApiServices();
builder.Services.AddHealthChecksConfiguration();

var app = builder.Build();

if (!app.Environment.IsEnvironment("Testing"))
{
    await app.Services.EnsureDatabaseCreated();
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.MapHealthCheckEndpoints();

app.Run();

public abstract partial class Program
{
}
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using src.API.DTOs;
using src.Core.Entities;
using src.Infrastructure.Data;

namespace src.IntegrationTests.API.Controllers;

public class CustomersControllerIntegrationTests : IClassFixture<WebApplicationFactory<Program>>, IAsyncLifetime
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public CustomersControllerIntegrationTests(WebApplicationFactory<Program> factory)
    {
        var databaseName = $"TestDb_{Guid.NewGuid()}";

        _factory = factory.WithWebHostBuilder(builder =>
        {
            // Configure test environment FIRST
            builder.ConfigureAppConfiguration((context, _) =>
            {
                context.HostingEnvironment.EnvironmentName = "Testing";
            });

            builder.ConfigureServices(services =>
            {
                // Remove all Entity Framework related services
                var descriptorsToRemove = services.Where(d =>
                        d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>) ||
                        d.ServiceType == typeof(ApplicationDbContext) ||
                        d.ServiceType.IsGenericType &&
                        d.ServiceType.GetGenericTypeDefinition() == typeof(DbContextOptions<>) ||
                        d.ServiceType.IsGenericType &&
                        d.ServiceType.GetGenericTypeDefinition() == typeof(IDbContextFactory<>) ||
                        d.ImplementationType?.FullName?.Contains("EntityFramework") == true ||
                        d.ImplementationType?.FullName?.Contains("Npgsql") == true ||
                        d.ServiceType.FullName?.Contains("EntityFramework") == true ||
                        d.ServiceType.FullName?.Contains("Npgsql") == true)
                    .ToList();

                foreach (var descriptor in descriptorsToRemove)
                {
                    services.Remove(descriptor);
                }

                // Add in-memory database for testing with consistent name
                services.AddDbContext<ApplicationDbContext>(options => { options.UseInMemoryDatabase(databaseName); });
            });
        });

        _client = _factory.CreateClient();
    }

    public async Task InitializeAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await context.Database.EnsureCreatedAsync();
    }

    public async Task DisposeAsync()
    {
        try
        {
            using var scope = _factory.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            await context.Database.EnsureDeletedAsync();
        }
        catch (ObjectDisposedException)
        {
            // Context already disposed, ignore
        }
        finally
        {
            _client.Dispose();
        }
    }

    private async Task ClearDatabaseAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        // Clear all data from tables
        context.Customers.RemoveRange(context.Customers);
        context.Books.RemoveRange(context.Books);
        context.Reservations.RemoveRange(context.Reservations);

        await context.SaveChangesAsync();
    }

    [Fact]
    public async Task GetCustomers_ShouldReturnOkWithCustomerList()
    {
        // Arrange - Clear database and add test data
        await ClearDatabaseAsync();

        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var customers = new List<Customer>
        {
            new() { FirstName = "John", LastName = "Doe", Email = "john@example.com" },
            new() { FirstName = "Jane", LastName = "Smith", Email = "jane@example.com" }
        };

        context.Customers.AddRange(customers);
        await context.SaveChangesAsync();

        // Act
        var response = await _client.GetAsync("/api/customers");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/json");

        var responseContent = await response.Content.ReadAsStringAsync();
        var customerDtos = JsonSerializer.Deserialize<List<CustomerDto>>(responseContent, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        customerDtos.Should().NotBeNull();
        customerDtos.Should().HaveCount(2);
        customerDtos.Should().Contain(c => c.FirstName == "John" && c.LastName == "Doe");
        customerDtos.Should().Contain(c => c.FirstName == "Jane" && c.LastName == "Smith");
    }

    [Fact]
    public async Task GetCustomers_WhenNoCustomers_ShouldReturnEmptyList()
    {
        // Arrange - Clear database to ensure no customers
        await ClearDatabaseAsync();

        // Act
        var response = await _client.GetAsync("/api/customers");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var responseContent = await response.Content.ReadAsStringAsync();
        var customerDtos = JsonSerializer.Deserialize<List<CustomerDto>>(responseContent, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        customerDtos.Should().NotBeNull();
        customerDtos.Should().BeEmpty();
    }

    [Fact]
    public async Task PostCustomer_WithValidData_ShouldCreateCustomerAndReturnCreated()
    {
        // Arrange - Clear database
        await ClearDatabaseAsync();

        var createDto = new CustomerCreateDto
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@example.com"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/customers", createDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        // Verify Location header
        response.Headers.Location.Should().NotBeNull();
        var locationHeader = response.Headers.Location!.ToString();
        (locationHeader.Contains("/api/customers/", StringComparison.OrdinalIgnoreCase) ||
         locationHeader.Contains("/api/Customers/")).Should().BeTrue(
            "Location header should contain the customers endpoint");

        // Verify response body
        var responseContent = await response.Content.ReadAsStringAsync();
        var customerDto = JsonSerializer.Deserialize<CustomerDto>(responseContent, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        customerDto.Should().NotBeNull();
        customerDto.Id.Should().BeGreaterThan(0);
        customerDto.FirstName.Should().Be("John");
        customerDto.LastName.Should().Be("Doe");
        customerDto.Email.Should().Be("john.doe@example.com");

        // Verify it's actually in the database
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var customerInDb = await context.Customers.FindAsync(customerDto.Id);
        customerInDb.Should().NotBeNull();
        customerInDb.FirstName.Should().Be("John");
    }

    [Fact]
    public async Task PostCustomer_WithInvalidData_ShouldReturnBadRequest()
    {
        // Arrange - Clear database and create invalid customer (empty required fields)
        await ClearDatabaseAsync();

        var createDto = new CustomerCreateDto
        {
            FirstName = "",
            LastName = "",
            Email = "invalid-email"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/customers", createDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var responseContent = await response.Content.ReadAsStringAsync();
        responseContent.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetCustomer_WithValidId_ShouldReturnOkWithCustomer()
    {
        // Arrange - Clear database and add customer to database
        await ClearDatabaseAsync();

        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var customer = new Customer
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john@example.com"
        };

        context.Customers.Add(customer);
        await context.SaveChangesAsync();

        // Act
        var response = await _client.GetAsync($"/api/customers/{customer.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var responseContent = await response.Content.ReadAsStringAsync();
        var customerDto = JsonSerializer.Deserialize<CustomerDto>(responseContent, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        customerDto.Should().NotBeNull();
        customerDto.Id.Should().Be(customer.Id);
        customerDto.FirstName.Should().Be("John");
        customerDto.LastName.Should().Be("Doe");
        customerDto.Email.Should().Be("john@example.com");
    }

    [Fact]
    public async Task GetCustomer_WithInvalidId_ShouldReturnNotFound()
    {
        // Arrange - Clear database
        await ClearDatabaseAsync();

        // Act
        var response = await _client.GetAsync("/api/customers/999");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task PutCustomer_WithValidData_ShouldUpdateCustomerAndReturnOk()
    {
        // Arrange - Clear database and add customer to database
        await ClearDatabaseAsync();

        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var customer = new Customer
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john@example.com"
        };

        context.Customers.Add(customer);
        await context.SaveChangesAsync();

        var updateDto = new CustomerUpdateDto
        {
            FirstName = "UpdatedJohn",
            LastName = "UpdatedDoe",
            Email = "updated.john@example.com"
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/customers/{customer.Id}", updateDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var responseContent = await response.Content.ReadAsStringAsync();
        var customerDto = JsonSerializer.Deserialize<CustomerDto>(responseContent, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        customerDto.Should().NotBeNull();
        customerDto.Id.Should().Be(customer.Id);
        customerDto.FirstName.Should().Be("UpdatedJohn");
        customerDto.LastName.Should().Be("UpdatedDoe");
        customerDto.Email.Should().Be("updated.john@example.com");

        // Verify it's actually updated in the database
        using var verifyScope = _factory.Services.CreateScope();
        var verifyContext = verifyScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var updatedCustomer = await verifyContext.Customers.FindAsync(customer.Id);
        updatedCustomer.Should().NotBeNull();
        updatedCustomer.FirstName.Should().Be("UpdatedJohn");
        updatedCustomer.LastName.Should().Be("UpdatedDoe");
        updatedCustomer.Email.Should().Be("updated.john@example.com");
    }

    [Fact]
    public async Task PutCustomer_WithInvalidId_ShouldReturnNotFound()
    {
        // Arrange - Clear database
        await ClearDatabaseAsync();

        var updateDto = new CustomerUpdateDto
        {
            FirstName = "UpdatedJohn",
            LastName = "UpdatedDoe",
            Email = "updated.john@example.com"
        };

        // Act
        var response = await _client.PutAsJsonAsync("/api/customers/999", updateDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task PutCustomer_WithInvalidData_ShouldReturnBadRequest()
    {
        // Arrange - Clear database and add customer to database
        await ClearDatabaseAsync();

        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var customer = new Customer
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john@example.com"
        };

        context.Customers.Add(customer);
        await context.SaveChangesAsync();

        var updateDto = new CustomerUpdateDto
        {
            FirstName = "",
            LastName = "",
            Email = "invalid-email"
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/customers/{customer.Id}", updateDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task DeleteCustomer_WhenCustomerExists_ShouldReturnNoContent()
    {
        // Arrange - Clear database and add customer to database
        await ClearDatabaseAsync();

        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var customer = new Customer
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john@example.com"
        };

        context.Customers.Add(customer);
        await context.SaveChangesAsync();

        // Act
        var response = await _client.DeleteAsync($"/api/customers/{customer.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify it's actually deleted from the database
        using var verifyScope = _factory.Services.CreateScope();
        var verifyContext = verifyScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var deletedCustomer = await verifyContext.Customers.FindAsync(customer.Id);
        deletedCustomer.Should().BeNull();
    }

    [Fact]
    public async Task DeleteCustomer_WhenCustomerNotExists_ShouldReturnNotFound()
    {
        // Arrange - Clear database
        await ClearDatabaseAsync();

        // Act
        var response = await _client.DeleteAsync("/api/customers/999");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ApiEndpoints_ShouldHaveCorrectContentType()
    {
        // Arrange - Clear database and add customer to database for testing
        await ClearDatabaseAsync();

        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var customer = new Customer
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john@example.com"
        };

        context.Customers.Add(customer);
        await context.SaveChangesAsync();

        // Act & Assert - Test all GET endpoints return JSON
        var getAllResponse = await _client.GetAsync("/api/customers");
        getAllResponse.Content.Headers.ContentType?.MediaType.Should().Be("application/json");

        var getByIdResponse = await _client.GetAsync($"/api/customers/{customer.Id}");
        getByIdResponse.Content.Headers.ContentType?.MediaType.Should().Be("application/json");
    }

    [Fact]
    public async Task CompleteWorkflow_ShouldWorkEndToEnd()
    {
        // Arrange - Clear database
        await ClearDatabaseAsync();

        // 1. Create a customer
        var createDto = new CustomerCreateDto
        {
            FirstName = "WorkflowTest",
            LastName = "Customer",
            Email = "workflow@example.com"
        };

        var createResponse = await _client.PostAsJsonAsync("/api/customers", createDto);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var createdCustomerJson = await createResponse.Content.ReadAsStringAsync();
        var createdCustomer = JsonSerializer.Deserialize<CustomerDto>(createdCustomerJson, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        // 2. Retrieve the customer
        var getResponse = await _client.GetAsync($"/api/customers/{createdCustomer?.Id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // 3. Update the customer
        var updateDto = new CustomerUpdateDto
        {
            FirstName = "UpdatedWorkflow",
            LastName = "UpdatedCustomer",
            Email = "updated.workflow@example.com"
        };

        var updateResponse = await _client.PutAsJsonAsync($"/api/customers/{createdCustomer?.Id}", updateDto);
        updateResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // 4. Verify the update
        var verifyResponse = await _client.GetAsync($"/api/customers/{createdCustomer?.Id}");
        var verifyJson = await verifyResponse.Content.ReadAsStringAsync();
        var verifiedCustomer = JsonSerializer.Deserialize<CustomerDto>(verifyJson, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        verifiedCustomer?.FirstName.Should().Be("UpdatedWorkflow");
        verifiedCustomer?.LastName.Should().Be("UpdatedCustomer");
        verifiedCustomer?.Email.Should().Be("updated.workflow@example.com");

        // 5. Delete the customer
        var deleteResponse = await _client.DeleteAsync($"/api/customers/{createdCustomer?.Id}");
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // 6. Verify deletion
        var deletedResponse = await _client.GetAsync($"/api/customers/{createdCustomer?.Id}");
        deletedResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
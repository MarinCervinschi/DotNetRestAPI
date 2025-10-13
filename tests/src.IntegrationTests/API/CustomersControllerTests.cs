using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using src.API.DTOs;
using src.Core.Entities;
using src.Core.Interfaces;
using src.Core.Interfaces.Repositories;
using src.IntegrationTests.Base;
using src.UnitTests.Core.Builders;
using System.Net;
using Xunit;

namespace src.IntegrationTests.API;

public class CustomersControllerTests : IntegrationTestBase
{
    private IRepository<Customer> _customerRepository = null!;
    private IAdminRepository _adminRepository = null!;

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        _customerRepository = Factory.Services.GetRequiredService<IRepository<Customer>>();
        _adminRepository = Factory.Services.GetRequiredService<IAdminRepository>();
    }

    private async Task SetupAuthenticationAsync()
    {
        var passwordHash = BCrypt.Net.BCrypt.HashPassword("password123");
        var admin = AdminBuilder.New()
            .WithUsername("testadmin")
            .WithPasswordHash(passwordHash)
            .Build();

        await _adminRepository.CreateAsync(admin);
        
        var loginRequest = new AdminLoginDto
        {
            Username = "testadmin",
            Password = "password123"
        };

        var loginResponse = await HttpClient.PostAsync("/Auth/login", CreateJsonContent(loginRequest));
        var loginResult = await DeserializeResponseAsync<LoginResponseDto>(loginResponse);
        
        HttpClient.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", loginResult!.Token);
    }

    [Fact]
    public async Task GetAllCustomers_WithAuthentication_ShouldReturnAllCustomers()
    {
        // Arrange
        await SetupAuthenticationAsync();
        
        var customer1 = CustomerBuilder.New().WithFirstName("John").WithLastName("Doe").WithEmail("john@test.com").Build();
        var customer2 = CustomerBuilder.New().WithFirstName("Jane").WithLastName("Smith").WithEmail("jane@test.com").Build();
        
        await _customerRepository.CreateAsync(customer1);
        await _customerRepository.CreateAsync(customer2);

        // Act
        var response = await HttpClient.GetAsync("/Customers");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var customers = await DeserializeResponseAsync<IEnumerable<CustomerDto>>(response);
        customers.Should().NotBeNull();
        var customersList = customers!.ToList();
        customersList.Should().HaveCountGreaterThanOrEqualTo(2);
    }

    [Fact]
    public async Task GetAllCustomers_WithoutAuthentication_ShouldReturnUnauthorized()
    {
        // Act
        var response = await HttpClient.GetAsync("/Customers");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetCustomer_WithValidIdAndAuthentication_ShouldReturnCustomer()
    {
        // Arrange
        await SetupAuthenticationAsync();
        
        var customer = CustomerBuilder.New()
            .WithFirstName("John")
            .WithLastName("Doe")
            .WithEmail("john@test.com")
            .Build();
        
        var createdCustomer = await _customerRepository.CreateAsync(customer);

        // Act
        var response = await HttpClient.GetAsync($"/Customers/{createdCustomer.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var returnedCustomer = await DeserializeResponseAsync<CustomerDto>(response);
        returnedCustomer.Should().NotBeNull();
        returnedCustomer.Id.Should().Be(createdCustomer.Id);
        returnedCustomer.FirstName.Should().Be("John");
        returnedCustomer.LastName.Should().Be("Doe");
        returnedCustomer.Email.Should().Be("john@test.com");
    }

    [Fact]
    public async Task GetCustomer_WithValidIdButNoAuthentication_ShouldReturnUnauthorized()
    {
        // Arrange
        var customer = CustomerBuilder.New().Build();
        var createdCustomer = await _customerRepository.CreateAsync(customer);

        // Act
        var response = await HttpClient.GetAsync($"/Customers/{createdCustomer.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetCustomer_WithInvalidId_ShouldReturnNotFound()
    {
        // Arrange
        await SetupAuthenticationAsync();

        // Act
        var response = await HttpClient.GetAsync("/Customers/99999");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CreateCustomer_WithValidDataAndAuthentication_ShouldReturnCreated()
    {
        // Arrange
        await SetupAuthenticationAsync();
        
        var customerCreateDto = new CustomerCreateDto
        {
            FirstName = "New",
            LastName = "Customer",
            Email = "new.customer@test.com"
        };

        // Act
        var response = await HttpClient.PostAsync("/Customers", CreateJsonContent(customerCreateDto));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        
        var createdCustomer = await DeserializeResponseAsync<CustomerDto>(response);
        createdCustomer.Should().NotBeNull();
        createdCustomer.FirstName.Should().Be("New");
        createdCustomer.LastName.Should().Be("Customer");
        createdCustomer.Email.Should().Be("new.customer@test.com");

        // Verify location header
        response.Headers.Location.Should().NotBeNull();
        response.Headers.Location!.ToString().Should().Contain($"/Customers/{createdCustomer.Id}");
    }

    [Fact]
    public async Task CreateCustomer_WithoutAuthentication_ShouldReturnUnauthorized()
    {
        // Arrange
        var customerCreateDto = new CustomerCreateDto
        {
            FirstName = "New",
            LastName = "Customer",
            Email = "new.customer@test.com"
        };

        // Act
        var response = await HttpClient.PostAsync("/Customers", CreateJsonContent(customerCreateDto));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreateCustomer_WithInvalidData_ShouldReturnBadRequest()
    {
        // Arrange
        await SetupAuthenticationAsync();

        var customerCreateDto = new CustomerCreateDto
        {
            FirstName = "", // Invalid - empty
            LastName = "Customer",
            Email = "invalid-email" // Invalid email format
        };

        // Act
        var response = await HttpClient.PostAsync("/Customers", CreateJsonContent(customerCreateDto));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateCustomer_WithMissingRequiredFields_ShouldReturnBadRequest()
    {
        // Arrange
        await SetupAuthenticationAsync();

        var customerCreateDto = new CustomerCreateDto
        {
            // Missing FirstName, LastName, and Email
        };

        // Act
        var response = await HttpClient.PostAsync("/Customers", CreateJsonContent(customerCreateDto));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UpdateCustomer_WithValidDataAndAuthentication_ShouldReturnUpdatedCustomer()
    {
        // Arrange
        await SetupAuthenticationAsync();
        
        var customer = CustomerBuilder.New()
            .WithFirstName("Original")
            .WithLastName("Name")
            .WithEmail("original@test.com")
            .Build();
        
        var createdCustomer = await _customerRepository.CreateAsync(customer);

        var customerUpdateDto = new CustomerUpdateDto
        {
            FirstName = "Updated",
            LastName = "Name",
            Email = "updated@test.com"
        };

        // Act
        var response = await HttpClient.PutAsync($"/Customers/{createdCustomer.Id}", CreateJsonContent(customerUpdateDto));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var updatedCustomer = await DeserializeResponseAsync<CustomerDto>(response);
        updatedCustomer.Should().NotBeNull();
        updatedCustomer.Id.Should().Be(createdCustomer.Id);
        updatedCustomer.FirstName.Should().Be("Updated");
        updatedCustomer.LastName.Should().Be("Name");
        updatedCustomer.Email.Should().Be("updated@test.com");
    }

    [Fact]
    public async Task UpdateCustomer_WithoutAuthentication_ShouldReturnUnauthorized()
    {
        // Arrange
        var customer = CustomerBuilder.New().Build();
        var createdCustomer = await _customerRepository.CreateAsync(customer);

        var customerUpdateDto = new CustomerUpdateDto
        {
            FirstName = "Updated",
            LastName = "Name",
            Email = "updated@test.com"
        };

        // Act
        var response = await HttpClient.PutAsync($"/Customers/{createdCustomer.Id}", CreateJsonContent(customerUpdateDto));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task UpdateCustomer_WithInvalidId_ShouldReturnNotFound()
    {
        // Arrange
        await SetupAuthenticationAsync();

        var customerUpdateDto = new CustomerUpdateDto
        {
            FirstName = "Updated",
            LastName = "Name",
            Email = "updated@test.com"
        };

        // Act
        var response = await HttpClient.PutAsync("/Customers/99999", CreateJsonContent(customerUpdateDto));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateCustomer_WithInvalidData_ShouldReturnBadRequest()
    {
        // Arrange
        await SetupAuthenticationAsync();

        var customer = CustomerBuilder.New().Build();
        var createdCustomer = await _customerRepository.CreateAsync(customer);

        var customerUpdateDto = new CustomerUpdateDto
        {
            FirstName = "", // Invalid - empty
            LastName = "Name",
            Email = "invalid-email" // Invalid email format
        };

        // Act
        var response = await HttpClient.PutAsync($"/Customers/{createdCustomer.Id}", CreateJsonContent(customerUpdateDto));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task DeleteCustomer_WithValidIdAndAuthentication_ShouldReturnNoContent()
    {
        // Arrange
        await SetupAuthenticationAsync();

        var customer = CustomerBuilder.New().Build();
        var createdCustomer = await _customerRepository.CreateAsync(customer);

        // Act
        var response = await HttpClient.DeleteAsync($"/Customers/{createdCustomer.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify customer is actually deleted
        var getResponse = await HttpClient.GetAsync($"/Customers/{createdCustomer.Id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteCustomer_WithoutAuthentication_ShouldReturnUnauthorized()
    {
        // Arrange
        var customer = CustomerBuilder.New().Build();
        var createdCustomer = await _customerRepository.CreateAsync(customer);

        // Act
        var response = await HttpClient.DeleteAsync($"/Customers/{createdCustomer.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task DeleteCustomer_WithInvalidId_ShouldReturnNotFound()
    {
        // Arrange
        await SetupAuthenticationAsync();

        // Act
        var response = await HttpClient.DeleteAsync("/Customers/99999");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task CreateCustomer_WithInvalidFirstName_ShouldReturnBadRequest(string firstName)
    {
        // Arrange
        await SetupAuthenticationAsync();

        var customerCreateDto = new CustomerCreateDto
        {
            FirstName = firstName,
            LastName = "ValidLastName",
            Email = "valid@test.com"
        };

        // Act
        var response = await HttpClient.PostAsync("/Customers", CreateJsonContent(customerCreateDto));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Theory]
    [InlineData("invalid")]
    [InlineData("@test.com")]
    [InlineData("user@")]
    [InlineData("")]
    public async Task CreateCustomer_WithInvalidEmail_ShouldReturnBadRequest(string email)
    {
        // Arrange
        await SetupAuthenticationAsync();

        var customerCreateDto = new CustomerCreateDto
        {
            FirstName = "ValidFirstName",
            LastName = "ValidLastName",
            Email = email
        };

        // Act
        var response = await HttpClient.PostAsync("/Customers", CreateJsonContent(customerCreateDto));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
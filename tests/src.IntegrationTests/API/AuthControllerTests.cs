using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using src.API.DTOs;
using src.Core.Interfaces.Repositories;
using src.IntegrationTests.Base;
using src.IntegrationTests.Helpers;
using src.UnitTests.Core.Builders;
using System.Net;
using System.Net.Http.Headers;
using Xunit;

namespace src.IntegrationTests.API;

public class AuthControllerTests : IntegrationTestBase
{
    private IAdminRepository _adminRepository = null!;
    private ApiTestHelper _apiHelper = null!;

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        _adminRepository = Factory.Services.GetRequiredService<IAdminRepository>();
        _apiHelper = new ApiTestHelper(this);
    }

    [Fact]
    public async Task Login_WithValidCredentials_ShouldReturnSuccessAndJwtToken()
    {
        // Arrange
        var passwordHash = BCrypt.Net.BCrypt.HashPassword("password123");
        var admin = AdminBuilder.New()
            .WithUsername("testadmin")
            .WithEmail("admin@test.com")
            .WithPasswordHash(passwordHash)
            .Build();

        await _adminRepository.CreateAsync(admin);

        var loginRequest = new AdminLoginDto
        {
            Username = "testadmin",
            Password = "password123"
        };

        // Act
        var response = await HttpClient.PostAsync("/Auth/login", CreateJsonContent(loginRequest));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var loginResponse = await DeserializeResponseAsync<LoginResponseDto>(response);
        loginResponse.Should().NotBeNull();
        loginResponse!.Token.Should().NotBeNullOrEmpty();
        loginResponse.Admin.Should().NotBeNull();
        loginResponse.Admin.Id.Should().Be(admin.Id);
        loginResponse.Admin.Username.Should().Be("testadmin");
        loginResponse.Admin.Email.Should().Be("admin@test.com");

        // Verify JWT token structure
        var tokenParts = loginResponse.Token.Split('.');
        tokenParts.Should().HaveCount(3);
    }

    [Fact]
    public async Task Login_WithInvalidUsername_ShouldReturnUnauthorized()
    {
        // Arrange
        var loginRequest = new AdminLoginDto
        {
            Username = "nonexistent",
            Password = "password123"
        };

        // Act
        var response = await HttpClient.PostAsync("/Auth/login", CreateJsonContent(loginRequest));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Login_WithInvalidPassword_ShouldReturnUnauthorized()
    {
        // Arrange
        var passwordHash = BCrypt.Net.BCrypt.HashPassword("correctpassword");
        var admin = AdminBuilder.New()
            .WithUsername("testadmin")
            .WithPasswordHash(passwordHash)
            .Build();

        await _adminRepository.CreateAsync(admin);

        var loginRequest = new AdminLoginDto
        {
            Username = "testadmin",
            Password = "wrongpassword"
        };

        // Act
        var response = await HttpClient.PostAsync("/Auth/login", CreateJsonContent(loginRequest));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Theory]
    [InlineData("", "password123")]
    [InlineData("testadmin", "")]
    [InlineData("testadmin", "123")] // Short password
    [InlineData("   ", "password123")]
    [InlineData("testadmin", "   ")]
    public async Task Login_WithInvalidData_ShouldReturnBadRequest(string username, string password)
    {
        // Arrange
        var loginRequest = new AdminLoginDto
        {
            Username = username,
            Password = password
        };

        // Act
        var response = await HttpClient.PostAsync("/Auth/login", CreateJsonContent(loginRequest));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Login_ThenUseTokenForProtectedEndpoint_ShouldWork()
    {
        // Arrange - Create admin and get token via API helper
        var (admin, loginResponse) = await _apiHelper.SetupAuthenticationAsync("integrationtest", "password123");

        // Create test customer to verify authenticated endpoint works
        var customer = await _apiHelper.CreateTestCustomerAsync();

        // Act - Access protected endpoint with authenticated client
        var response = await HttpClient.GetAsync($"/Customers/{customer.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var returnedCustomer = await DeserializeResponseAsync<CustomerDto>(response);
        returnedCustomer.Should().NotBeNull();
        returnedCustomer.Id.Should().Be(customer.Id);
    }

    [Fact]
    public async Task ProtectedEndpoint_WithoutToken_ShouldReturnUnauthorized()
    {
        // Arrange
        _apiHelper.ClearAuthentication();
        var customer = await _apiHelper.CreateTestCustomerAsync();

        // Act - Try to access protected endpoint without authentication
        var response = await HttpClient.GetAsync($"/Customers/{customer.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ProtectedEndpoint_WithInvalidToken_ShouldReturnUnauthorized()
    {
        // Arrange
        HttpClient.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "invalid.jwt.token");

        // Act
        var response = await HttpClient.GetAsync("/Customers");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}

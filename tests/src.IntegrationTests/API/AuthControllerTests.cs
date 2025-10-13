using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using src.API.DTOs;
using src.Core.Interfaces.Repositories;
using src.IntegrationTests.Base;
using src.UnitTests.Core.Builders;
using System.Net;
using System.Net.Http.Headers;

namespace src.IntegrationTests.API;

public class AuthControllerTests : IntegrationTestBase
{
    private IAdminRepository _adminRepository = null!;

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        _adminRepository = Factory.Services.GetRequiredService<IAdminRepository>();
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

    [Fact]
    public async Task Login_WithEmptyUsername_ShouldReturnBadRequest()
    {
        // Arrange
        var loginRequest = new AdminLoginDto
        {
            Username = "",
            Password = "password123"
        };

        // Act
        var response = await HttpClient.PostAsync("/Auth/login", CreateJsonContent(loginRequest));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Login_WithEmptyPassword_ShouldReturnBadRequest()
    {
        // Arrange
        var loginRequest = new AdminLoginDto
        {
            Username = "testadmin",
            Password = ""
        };

        // Act
        var response = await HttpClient.PostAsync("/Auth/login", CreateJsonContent(loginRequest));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Login_WithShortPassword_ShouldReturnBadRequest()
    {
        // Arrange
        var loginRequest = new AdminLoginDto
        {
            Username = "testadmin",
            Password = "123" // Less than 6 characters
        };

        // Act
        var response = await HttpClient.PostAsync("/Auth/login", CreateJsonContent(loginRequest));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Login_WithNullRequestBody_ShouldReturnBadRequest()
    {
        // Act
        var response = await HttpClient.PostAsync("/Auth/login", CreateJsonContent((object?)null));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task JwtToken_WhenUsedInAuthenticatedEndpoint_ShouldWork()
    {
        // Arrange
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

        // Act
        HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResult!.Token);
        var authenticatedResponse = await HttpClient.GetAsync("/Books");

        // Assert
        authenticatedResponse.StatusCode.Should().NotBe(HttpStatusCode.Unauthorized);
    }
}

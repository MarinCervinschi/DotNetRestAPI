using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using src.API.Controllers;
using src.API.DTOs;
using src.Core.Interfaces.Services;
using src.UnitTests.Core.Builders;

namespace src.UnitTests.API.Controllers;

public class AuthControllerTests
{
    private readonly Mock<IAuthService> _mockAuthService;
    private readonly AuthController _controller;

    public AuthControllerTests()
    {
        _mockAuthService = new Mock<IAuthService>();
        var mockLogger = new Mock<ILogger<AuthController>>();
        _controller = new AuthController(_mockAuthService.Object, mockLogger.Object);
    }

    [Fact]
    public async Task Login_WithValidCredentials_ReturnsOkWithToken()
    {
        // Arrange
        var loginDto = AdminBuilder.New()
            .WithUsername("testadmin")
            .BuildLoginDto();

        var expectedResponse = new LoginResponseDto
        {
            Token = "valid.jwt.token",
            Admin = AdminBuilder.New()
                .WithId(1)
                .WithUsername("testadmin")
                .WithEmail("test@example.com")
                .BuildDto()
        };

        _mockAuthService.Setup(s => s.LoginAsync(loginDto))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _controller.Login(loginDto);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(200);

        var returnedResponse = okResult.Value.Should().BeOfType<LoginResponseDto>().Subject;
        returnedResponse.Token.Should().Be("valid.jwt.token");
        returnedResponse.Admin.Should().NotBeNull();
        returnedResponse.Admin.Id.Should().Be(1);
        returnedResponse.Admin.Username.Should().Be("testadmin");
        returnedResponse.Admin.Email.Should().Be("test@example.com");

        _mockAuthService.Verify(s => s.LoginAsync(loginDto), Times.Once);
    }

    [Fact]
    public async Task Login_WithInvalidCredentials_ReturnsUnauthorized()
    {
        // Arrange
        var loginDto = AdminBuilder.New()
            .WithUsername("invaliduser")
            .BuildLoginDto();

        _mockAuthService.Setup(s => s.LoginAsync(loginDto))
            .ReturnsAsync((LoginResponseDto?)null);

        // Act
        var result = await _controller.Login(loginDto);

        // Assert
        var unauthorizedResult = result.Result.Should().BeOfType<UnauthorizedObjectResult>().Subject;
        unauthorizedResult.StatusCode.Should().Be(401);
        unauthorizedResult.Value.Should().Be("Invalid username or password");

        _mockAuthService.Verify(s => s.LoginAsync(loginDto), Times.Once);
    }

    [Fact]
    public async Task Login_WithInvalidModelState_ReturnsBadRequest()
    {
        // Arrange
        var loginDto = AdminBuilder.New()
            .WithUsername("")
            .BuildLoginDto();
        loginDto.Password = "123"; // Too short

        _controller.ModelState.AddModelError("Username", "Username is required");
        _controller.ModelState.AddModelError("Password", "Password must be at least 6 characters long");

        // Act
        var result = await _controller.Login(loginDto);

        // Assert
        var badRequestResult = result.Result.Should().BeOfType<BadRequestObjectResult>().Subject;
        badRequestResult.StatusCode.Should().Be(400);
        badRequestResult.Value.Should().BeOfType<SerializableError>();

        _mockAuthService.Verify(s => s.LoginAsync(It.IsAny<AdminLoginDto>()), Times.Never);
    }

    [Theory]
    [InlineData("", "password123")]
    [InlineData("   ", "password123")]
    [InlineData("validuser", "")]
    [InlineData("validuser", "   ")]
    public async Task Login_WithEmptyCredentials_ReturnsUnauthorized(string username, string password)
    {
        // Arrange
        var loginDto = new AdminLoginDto
        {
            Username = username,
            Password = password
        };

        _mockAuthService.Setup(s => s.LoginAsync(It.IsAny<AdminLoginDto>()))
            .ReturnsAsync((LoginResponseDto?)null);

        // Act
        var result = await _controller.Login(loginDto);

        // Assert
        var unauthorizedResult = result.Result.Should().BeOfType<UnauthorizedObjectResult>().Subject;
        unauthorizedResult.StatusCode.Should().Be(401);
        unauthorizedResult.Value.Should().Be("Invalid username or password");

        _mockAuthService.Verify(s => s.LoginAsync(It.IsAny<AdminLoginDto>()), Times.Once);
    }

    [Fact]
    public async Task Login_WithNullLoginDto_ReturnsBadRequest()
    {
        // Act
        var result = await _controller.Login(null!);

        // Assert
        var badRequestResult = result.Result.Should().BeOfType<BadRequestObjectResult>().Subject;
        badRequestResult.StatusCode.Should().Be(400);
        badRequestResult.Value.Should().Be("Login data is required");

        _mockAuthService.Verify(s => s.LoginAsync(It.IsAny<AdminLoginDto>()), Times.Never);
    }

    [Fact]
    public async Task Login_ServiceThrowsException_PropagatesException()
    {
        // Arrange
        var loginDto = AdminBuilder.New()
            .WithUsername("testuser")
            .BuildLoginDto();

        _mockAuthService.Setup(s => s.LoginAsync(loginDto))
            .ThrowsAsync(new InvalidOperationException("Database connection failed"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _controller.Login(loginDto));

        exception.Message.Should().Be("Database connection failed");
        _mockAuthService.Verify(s => s.LoginAsync(loginDto), Times.Once);
    }
}

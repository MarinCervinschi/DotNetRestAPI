using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;
using src.API.Configuration;
using src.Core.Entities;
using src.Core.Interfaces.Repositories;
using src.Core.Services;
using src.UnitTests.Core.Builders;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace src.UnitTests.Core.Services;

public class AuthServiceTests
{
    private readonly Mock<IAdminRepository> _mockAdminRepository;
    private readonly JwtConfig _jwtConfig;
    private readonly AuthService _authService;

    public AuthServiceTests()
    {
        _mockAdminRepository = new Mock<IAdminRepository>();
        var mockJwtOptions = new Mock<IOptions<JwtConfig>>();

        _jwtConfig = new JwtConfig
        {
            Key = "ThisIsAVeryLongSecretKeyForTestingPurposes12345678901234567890",
            Issuer = "TestIssuer",
            Audience = "TestAudience",
            ExpiryInMinutes = 60
        };

        mockJwtOptions.Setup(x => x.Value).Returns(_jwtConfig);
        _authService = new AuthService(_mockAdminRepository.Object, mockJwtOptions.Object);
    }

    [Fact]
    public async Task LoginAsync_WithValidCredentials_ReturnsLoginResponse()
    {
        // Arrange
        var username = "testadmin";
        var password = "testpassword123";
        var hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);

        var admin = AdminBuilder.New()
            .WithId(1)
            .WithUsername(username)
            .WithEmail("test@example.com")
            .WithPasswordHash(hashedPassword)
            .Build();

        var loginDto = AdminBuilder.New()
            .WithUsername(username)
            .BuildLoginDto();
        loginDto.Password = password;

        _mockAdminRepository.Setup(r => r.GetByUsernameAsync(username))
            .ReturnsAsync(admin);

        // Act
        var result = await _authService.LoginAsync(loginDto);

        // Assert
        result.Should().NotBeNull();
        result.Token.Should().NotBeNullOrEmpty();
        result.Admin.Should().NotBeNull();
        result.Admin.Id.Should().Be(1);
        result.Admin.Username.Should().Be(username);
        result.Admin.Email.Should().Be("test@example.com");

        _mockAdminRepository.Verify(r => r.GetByUsernameAsync(username), Times.Once);
    }

    [Fact]
    public async Task LoginAsync_WithInvalidUsername_ReturnsNull()
    {
        // Arrange
        var loginDto = AdminBuilder.New()
            .WithUsername("nonexistent")
            .BuildLoginDto();

        _mockAdminRepository.Setup(r => r.GetByUsernameAsync(loginDto.Username))
            .ReturnsAsync((Admin?)null);

        // Act
        var result = await _authService.LoginAsync(loginDto);

        // Assert
        result.Should().BeNull();
        _mockAdminRepository.Verify(r => r.GetByUsernameAsync(loginDto.Username), Times.Once);
    }

    [Fact]
    public async Task LoginAsync_WithInvalidPassword_ReturnsNull()
    {
        // Arrange
        var username = "testadmin";
        var correctPassword = "correctpassword123";
        var wrongPassword = "wrongpassword";
        var hashedPassword = BCrypt.Net.BCrypt.HashPassword(correctPassword);

        var admin = AdminBuilder.New()
            .WithId(1)
            .WithUsername(username)
            .WithPasswordHash(hashedPassword)
            .Build();

        var loginDto = AdminBuilder.New()
            .WithUsername(username)
            .BuildLoginDto();
        loginDto.Password = wrongPassword;

        _mockAdminRepository.Setup(r => r.GetByUsernameAsync(username))
            .ReturnsAsync(admin);

        // Act
        var result = await _authService.LoginAsync(loginDto);

        // Assert
        result.Should().BeNull();
        _mockAdminRepository.Verify(r => r.GetByUsernameAsync(username), Times.Once);
    }

    [Fact]
    public void GenerateJwtToken_WithValidData_ReturnsValidToken()
    {
        // Arrange
        var adminId = 1;
        var username = "testadmin";

        // Act
        var token = _authService.GenerateJwtToken(adminId, username);

        // Assert
        token.Should().NotBeNullOrEmpty();

        // Verify token structure
        var tokenHandler = new JwtSecurityTokenHandler();
        var jsonToken = tokenHandler.ReadJwtToken(token);

        jsonToken.Issuer.Should().Be(_jwtConfig.Issuer);
        jsonToken.Audiences.Should().Contain(_jwtConfig.Audience);
        jsonToken.Claims.Should().Contain(c => c.Type == ClaimTypes.NameIdentifier && c.Value == adminId.ToString());
        jsonToken.Claims.Should().Contain(c => c.Type == ClaimTypes.Name && c.Value == username);
        jsonToken.ValidTo.Should()
            .BeCloseTo(DateTime.UtcNow.AddMinutes(_jwtConfig.ExpiryInMinutes), TimeSpan.FromSeconds(5));
    }

    [Theory]
    [InlineData("password123", true)]
    [InlineData("wrongpassword", false)]
    [InlineData("", false)]
    [InlineData("PASSWORD123", false)] // Case sensitive
    public void VerifyPassword_WithVariousPasswords_ReturnsExpectedResult(string passwordToVerify, bool expectedResult)
    {
        // Arrange
        var originalPassword = "password123";
        var hashedPassword = BCrypt.Net.BCrypt.HashPassword(originalPassword);

        // Act
        var result = _authService.VerifyPassword(passwordToVerify, hashedPassword);

        // Assert
        result.Should().Be(expectedResult);
    }

    [Fact]
    public void VerifyPassword_WithNullOrEmptyHash_ReturnsFalse()
    {
        // Arrange
        var password = "testpassword";

        // Act & Assert
        _authService.VerifyPassword(password, "").Should().BeFalse();
        _authService.VerifyPassword(password, null!).Should().BeFalse();
    }

    [Fact]
    public void GenerateJwtToken_CreatesTokenWithCorrectExpiry()
    {
        // Arrange
        var adminId = 1;
        var username = "testadmin";
        var expectedExpiry = DateTime.UtcNow.AddMinutes(_jwtConfig.ExpiryInMinutes);

        // Act
        var token = _authService.GenerateJwtToken(adminId, username);

        // Assert
        var tokenHandler = new JwtSecurityTokenHandler();
        var jsonToken = tokenHandler.ReadJwtToken(token);

        jsonToken.ValidTo.Should().BeCloseTo(expectedExpiry, TimeSpan.FromSeconds(5));
    }
}
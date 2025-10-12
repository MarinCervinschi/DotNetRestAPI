using FluentAssertions;
using Moq;
using src.Core.Entities;
using src.Core.Interfaces.Repositories;
using src.Core.Services;
using src.UnitTests.Core.Builders;

namespace src.UnitTests.Core.Services;

public class AdminServiceTests
{
    private readonly Mock<IAdminRepository> _mockAdminRepository;
    private readonly AdminService _adminService;

    public AdminServiceTests()
    {
        _mockAdminRepository = new Mock<IAdminRepository>();
        _adminService = new AdminService(_mockAdminRepository.Object);
    }

    [Fact]
    public async Task GetByUsernameAsync_WithValidUsername_ReturnsAdminDto()
    {
        // Arrange
        var username = "testadmin";
        var admin = AdminBuilder.New()
            .WithId(1)
            .WithUsername(username)
            .WithEmail("testadmin@example.com")
            .Build();

        _mockAdminRepository.Setup(r => r.GetByUsernameAsync(username))
            .ReturnsAsync(admin);

        // Act
        var result = await _adminService.GetByUsernameAsync(username);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(1);
        result.Username.Should().Be(username);
        result.Email.Should().Be("testadmin@example.com");

        _mockAdminRepository.Verify(r => r.GetByUsernameAsync(username), Times.Once);
    }

    [Fact]
    public async Task GetByUsernameAsync_WithInvalidUsername_ReturnsNull()
    {
        // Arrange
        var username = "nonexistent";
        _mockAdminRepository.Setup(r => r.GetByUsernameAsync(username))
            .ReturnsAsync((Admin?)null);

        // Act
        var result = await _adminService.GetByUsernameAsync(username);

        // Assert
        result.Should().BeNull();
        _mockAdminRepository.Verify(r => r.GetByUsernameAsync(username), Times.Once);
    }

    [Fact]
    public async Task CreateAdminAsync_WithValidData_ReturnsAdminDto()
    {
        // Arrange
        var username = "newadmin";
        var email = "newadmin@example.com";
        var password = "password123";

        _mockAdminRepository.Setup(r => r.CreateAsync(It.IsAny<Admin>()))
            .Callback<Admin>(admin => admin.Id = 1);

        // Act
        var result = await _adminService.CreateAdminAsync(username, email, password);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(1);
        result.Username.Should().Be(username);
        result.Email.Should().Be(email);

        _mockAdminRepository.Verify(r => r.CreateAsync(It.Is<Admin>(a =>
            a.Username == username &&
            a.Email == email &&
            !string.IsNullOrEmpty(a.PasswordHash))), Times.Once);
    }

    [Fact]
    public async Task CreateAdminAsync_HashesPasswordCorrectly()
    {
        // Arrange
        var username = "testadmin";
        var email = "test@example.com";
        var password = "testpassword123";
        Admin capturedAdmin = null!;

        _mockAdminRepository.Setup(r => r.CreateAsync(It.IsAny<Admin>()))
            .Callback<Admin>(admin =>
            {
                admin.Id = 1;
                capturedAdmin = admin;
            });

        // Act
        await _adminService.CreateAdminAsync(username, email, password);

        // Assert
        capturedAdmin.Should().NotBeNull();
        capturedAdmin.PasswordHash.Should().NotBeNullOrEmpty();
        capturedAdmin.PasswordHash.Should().NotBe(password); // Should be hashed, not plain text
        BCrypt.Net.BCrypt.Verify(password, capturedAdmin.PasswordHash).Should().BeTrue();

        _mockAdminRepository.Verify(r => r.CreateAsync(It.IsAny<Admin>()), Times.Once);
    }
}
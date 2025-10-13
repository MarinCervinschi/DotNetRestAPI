using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using src.API.Controllers;
using src.API.DTOs;
using src.Core.Interfaces.Services;
using src.UnitTests.Core.Builders;

namespace src.UnitTests.API.Controllers;

public class AdminControllerTests
{
    private readonly Mock<IAdminService> _mockAdminService;
    private readonly AdminController _controller;

    public AdminControllerTests()
    {
        _mockAdminService = new Mock<IAdminService>();
        var mockLogger = new Mock<ILogger<AdminController>>();
        _controller = new AdminController(_mockAdminService.Object, mockLogger.Object);
    }

    [Fact]
    public async Task GetByUsername_WithValidUsername_ReturnsOkWithAdmin()
    {
        // Arrange
        var username = "testadmin";
        var adminDto = AdminBuilder.New()
            .WithId(1)
            .WithUsername(username)
            .WithEmail("testadmin@example.com")
            .BuildDto();

        _mockAdminService.Setup(s => s.GetByUsernameAsync(username))
            .ReturnsAsync(adminDto);

        // Act
        var result = await _controller.GetByUsername(username);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(200);

        var returnedAdmin = okResult.Value.Should().BeOfType<AdminDto>().Subject;
        returnedAdmin.Id.Should().Be(1);
        returnedAdmin.Username.Should().Be(username);
        returnedAdmin.Email.Should().Be("testadmin@example.com");

        _mockAdminService.Verify(s => s.GetByUsernameAsync(username), Times.Once);
    }

    [Fact]
    public async Task GetByUsername_WithInvalidUsername_ReturnsNotFound()
    {
        // Arrange
        var username = "nonexistent";
        _mockAdminService.Setup(s => s.GetByUsernameAsync(username))
            .ReturnsAsync((AdminDto?)null);

        // Act
        var result = await _controller.GetByUsername(username);

        // Assert
        result.Result.Should().BeOfType<NotFoundResult>();
        var notFoundResult = result.Result as NotFoundResult;
        notFoundResult!.StatusCode.Should().Be(404);

        _mockAdminService.Verify(s => s.GetByUsernameAsync(username), Times.Once);
    }

    [Fact]
    public async Task CreateAdmin_WithValidData_ReturnsCreated()
    {
        // Arrange
        var createDto = AdminBuilder.New()
            .WithUsername("newadmin")
            .WithEmail("newadmin@example.com")
            .BuildCreateDto();

        var createdAdmin = AdminBuilder.New()
            .WithId(1)
            .WithUsername("newadmin")
            .WithEmail("newadmin@example.com")
            .BuildDto();

        _mockAdminService.Setup(s => s.CreateAdminAsync(createDto.Username, createDto.Email, createDto.Password))
            .ReturnsAsync(createdAdmin);

        // Act
        var result = await _controller.CreateAdmin(createDto);

        // Assert
        var createdResult = result.Result.Should().BeOfType<CreatedAtActionResult>().Subject;
        createdResult.StatusCode.Should().Be(201);
        createdResult.ActionName.Should().Be(nameof(AdminController.GetByUsername));
        createdResult.RouteValues!["username"].Should().Be("newadmin");

        var returnedAdmin = createdResult.Value.Should().BeOfType<AdminDto>().Subject;
        returnedAdmin.Id.Should().Be(1);
        returnedAdmin.Username.Should().Be("newadmin");
        returnedAdmin.Email.Should().Be("newadmin@example.com");

        _mockAdminService.Verify(s => s.CreateAdminAsync(createDto.Username, createDto.Email, createDto.Password),
            Times.Once);
    }

    [Fact]
    public async Task CreateAdmin_WithInvalidModelState_ReturnsBadRequest()
    {
        // Arrange
        var createDto = AdminBuilder.New()
            .WithUsername("")
            .WithEmail("invalid-email")
            .BuildCreateDto();
        createDto.Password = "123"; // Too short

        _controller.ModelState.AddModelError("Username", "Username is required");
        _controller.ModelState.AddModelError("Email", "Please enter a valid email address");
        _controller.ModelState.AddModelError("Password", "Password must be at least 6 characters long");

        // Act
        var result = await _controller.CreateAdmin(createDto);

        // Assert
        var badRequestResult = result.Result.Should().BeOfType<BadRequestObjectResult>().Subject;
        badRequestResult.StatusCode.Should().Be(400);
        badRequestResult.Value.Should().BeOfType<SerializableError>();

        _mockAdminService.Verify(s => s.CreateAdminAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()),
            Times.Never);
    }

    [Fact]
    public async Task CreateAdmin_WithDuplicateUsername_ThrowsException()
    {
        // Arrange
        var createDto = AdminBuilder.New()
            .WithUsername("existingadmin")
            .WithEmail("test@example.com")
            .BuildCreateDto();

        _mockAdminService.Setup(s => s.CreateAdminAsync(createDto.Username, createDto.Email, createDto.Password))
            .ThrowsAsync(new InvalidOperationException("An admin with username 'existingadmin' already exists."));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _controller.CreateAdmin(createDto));

        exception.Message.Should().Contain("existingadmin");
        _mockAdminService.Verify(s => s.CreateAdminAsync(createDto.Username, createDto.Email, createDto.Password),
            Times.Once);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public async Task GetByUsername_WithEmptyOrNullUsername_ReturnsNotFound(string? username)
    {
        // Arrange
        _mockAdminService.Setup(s => s.GetByUsernameAsync(It.IsAny<string>()))
            .ReturnsAsync((AdminDto?)null);

        // Act
        var result = await _controller.GetByUsername(username!);

        // Assert
        result.Result.Should().BeOfType<NotFoundResult>();
        _mockAdminService.Verify(s => s.GetByUsernameAsync(It.IsAny<string>()), Times.Once);
    }
}

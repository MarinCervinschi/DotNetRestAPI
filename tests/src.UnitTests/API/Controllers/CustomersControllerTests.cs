using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using src.API.Controllers;
using src.API.DTOs;
using src.Core.Interfaces.Services;

namespace src.UnitTests.API.Controllers;

public class CustomersControllerTests
{
    private readonly Mock<ICustomerService> _mockCustomerService;
    private readonly CustomersController _controller;

    public CustomersControllerTests()
    {
        _mockCustomerService = new Mock<ICustomerService>();
        var mockLogger = new Mock<ILogger<CustomersController>>();
        _controller = new CustomersController(_mockCustomerService.Object, mockLogger.Object);
    }

    [Fact]
    public async Task GetAllCustomers_ReturnsOkWithCustomerList()
    {
        // Arrange
        var customerDtos = new List<CustomerDto>
        {
            new() { Id = 1, FirstName = "John", LastName = "Doe", Email = "john@example.com" },
            new() { Id = 2, FirstName = "Jane", LastName = "Smith", Email = "jane@example.com" }
        };

        _mockCustomerService.Setup(s => s.GetAllCustomersAsync())
            .ReturnsAsync(customerDtos);

        // Act
        var result = await _controller.GetAllCustomers();

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(200);

        var returnedCustomers = okResult.Value.Should().BeOfType<List<CustomerDto>>().Subject;
        returnedCustomers.Should().HaveCount(2);
        returnedCustomers[0].Id.Should().Be(1);
        returnedCustomers[0].FirstName.Should().Be("John");

        _mockCustomerService.Verify(s => s.GetAllCustomersAsync(), Times.Once);
    }

    [Fact]
    public async Task GetAllCustomers_WhenNoCustomers_ReturnsOkWithEmptyList()
    {
        // Arrange
        _mockCustomerService.Setup(s => s.GetAllCustomersAsync())
            .ReturnsAsync(new List<CustomerDto>());

        // Act
        var result = await _controller.GetAllCustomers();

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(200);

        var returnedCustomers = okResult.Value.Should().BeOfType<List<CustomerDto>>().Subject;
        returnedCustomers.Should().BeEmpty();

        _mockCustomerService.Verify(s => s.GetAllCustomersAsync(), Times.Once);
    }

    [Fact]
    public async Task GetCustomer_WithValidId_ReturnsOkWithCustomer()
    {
        // Arrange
        var customerId = 1;
        var customerDto = new CustomerDto
        {
            Id = customerId,
            FirstName = "John",
            LastName = "Doe",
            Email = "john@example.com"
        };

        _mockCustomerService.Setup(s => s.GetCustomerByIdAsync(customerId))
            .ReturnsAsync(customerDto);

        // Act
        var result = await _controller.GetCustomer(customerId);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(200);

        var returnedCustomer = okResult.Value.Should().BeOfType<CustomerDto>().Subject;
        returnedCustomer.Id.Should().Be(customerId);
        returnedCustomer.FirstName.Should().Be("John");
        returnedCustomer.LastName.Should().Be("Doe");
        returnedCustomer.Email.Should().Be("john@example.com");

        _mockCustomerService.Verify(s => s.GetCustomerByIdAsync(customerId), Times.Once);
    }

    [Fact]
    public async Task GetCustomer_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        var customerId = 999;
        _mockCustomerService.Setup(s => s.GetCustomerByIdAsync(customerId))
            .ReturnsAsync((CustomerDto?)null);

        // Act
        var result = await _controller.GetCustomer(customerId);

        // Assert
        result.Result.Should().BeOfType<NotFoundResult>();
        var notFoundResult = result.Result as NotFoundResult;
        notFoundResult!.StatusCode.Should().Be(404);

        _mockCustomerService.Verify(s => s.GetCustomerByIdAsync(customerId), Times.Once);
    }

    [Fact]
    public async Task CreateCustomer_WithValidData_ReturnsCreated()
    {
        // Arrange
        var createDto = new CustomerCreateDto
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john@example.com"
        };

        var createdCustomer = new CustomerDto
        {
            Id = 1,
            FirstName = "John",
            LastName = "Doe",
            Email = "john@example.com"
        };

        _mockCustomerService.Setup(s => s.CreateCustomerAsync(createDto))
            .ReturnsAsync(createdCustomer);

        // Act
        var result = await _controller.CreateCustomer(createDto);

        // Assert
        var createdResult = result.Result.Should().BeOfType<CreatedAtActionResult>().Subject;
        createdResult.StatusCode.Should().Be(201);
        createdResult.ActionName.Should().Be(nameof(CustomersController.GetCustomer));
        createdResult.RouteValues!["id"].Should().Be(1);

        var returnedCustomer = createdResult.Value.Should().BeOfType<CustomerDto>().Subject;
        returnedCustomer.Id.Should().Be(1);
        returnedCustomer.FirstName.Should().Be("John");

        _mockCustomerService.Verify(s => s.CreateCustomerAsync(createDto), Times.Once);
    }

    [Fact]
    public async Task CreateCustomer_WithInvalidModelState_ReturnsBadRequest()
    {
        // Arrange
        var createDto = new CustomerCreateDto
        {
            FirstName = "",
            LastName = "",
            Email = "invalid-email"
        };

        _controller.ModelState.AddModelError("FirstName", "First name is required");
        _controller.ModelState.AddModelError("Email", "Invalid email format");

        // Act
        var result = await _controller.CreateCustomer(createDto);

        // Assert
        var badRequestResult = result.Result.Should().BeOfType<BadRequestObjectResult>().Subject;
        badRequestResult.StatusCode.Should().Be(400);
        badRequestResult.Value.Should().BeOfType<SerializableError>();

        _mockCustomerService.Verify(s => s.CreateCustomerAsync(It.IsAny<CustomerCreateDto>()), Times.Never);
    }

    [Fact]
    public async Task CreateCustomer_WhenServiceThrowsException_ReturnsInternalServerError()
    {
        // Arrange
        var createDto = new CustomerCreateDto
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john@example.com"
        };

        _mockCustomerService.Setup(s => s.CreateCustomerAsync(createDto))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _controller.CreateCustomer(createDto);

        // Assert
        var statusCodeResult = result.Result.Should().BeOfType<ObjectResult>().Subject;
        statusCodeResult.StatusCode.Should().Be(500);
        statusCodeResult.Value.Should().Be("Internal server error");

        _mockCustomerService.Verify(s => s.CreateCustomerAsync(createDto), Times.Once);
    }

    [Fact]
    public async Task UpdateCustomer_WithValidData_ReturnsOkWithUpdatedCustomer()
    {
        // Arrange
        var customerId = 1;
        var updateDto = new CustomerUpdateDto
        {
            FirstName = "UpdatedJohn",
            LastName = "UpdatedDoe",
            Email = "updated@example.com"
        };

        var updatedCustomer = new CustomerDto
        {
            Id = customerId,
            FirstName = "UpdatedJohn",
            LastName = "UpdatedDoe",
            Email = "updated@example.com"
        };

        _mockCustomerService.Setup(s => s.UpdateCustomerAsync(customerId, updateDto))
            .ReturnsAsync(updatedCustomer);

        // Act
        var result = await _controller.UpdateCustomer(customerId, updateDto);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(200);

        var returnedCustomer = okResult.Value.Should().BeOfType<CustomerDto>().Subject;
        returnedCustomer.Id.Should().Be(customerId);
        returnedCustomer.FirstName.Should().Be("UpdatedJohn");
        returnedCustomer.LastName.Should().Be("UpdatedDoe");
        returnedCustomer.Email.Should().Be("updated@example.com");

        _mockCustomerService.Verify(s => s.UpdateCustomerAsync(customerId, updateDto), Times.Once);
    }

    [Fact]
    public async Task UpdateCustomer_WithInvalidModelState_ReturnsBadRequest()
    {
        // Arrange
        var customerId = 1;
        var updateDto = new CustomerUpdateDto
        {
            FirstName = "",
            LastName = "",
            Email = "invalid-email"
        };

        _controller.ModelState.AddModelError("FirstName", "First name is required");
        _controller.ModelState.AddModelError("Email", "Invalid email format");

        // Act
        var result = await _controller.UpdateCustomer(customerId, updateDto);

        // Assert
        var badRequestResult = result.Result.Should().BeOfType<BadRequestObjectResult>().Subject;
        badRequestResult.StatusCode.Should().Be(400);
        badRequestResult.Value.Should().BeOfType<SerializableError>();

        _mockCustomerService.Verify(s => s.UpdateCustomerAsync(It.IsAny<int>(), It.IsAny<CustomerUpdateDto>()),
            Times.Never);
    }

    [Fact]
    public async Task UpdateCustomer_WhenCustomerNotExists_ReturnsNotFound()
    {
        // Arrange
        var customerId = 999;
        var updateDto = new CustomerUpdateDto
        {
            FirstName = "UpdatedJohn",
            LastName = "UpdatedDoe",
            Email = "updated@example.com"
        };

        _mockCustomerService.Setup(s => s.UpdateCustomerAsync(customerId, updateDto))
            .ThrowsAsync(new KeyNotFoundException($"Customer with ID {customerId} not found."));

        // Act
        var result = await _controller.UpdateCustomer(customerId, updateDto);

        // Assert
        result.Result.Should().BeOfType<NotFoundResult>();
        var notFoundResult = result.Result as NotFoundResult;
        notFoundResult!.StatusCode.Should().Be(404);

        _mockCustomerService.Verify(s => s.UpdateCustomerAsync(customerId, updateDto), Times.Once);
    }

    [Fact]
    public async Task DeleteCustomer_WhenCustomerExists_ReturnsNoContent()
    {
        // Arrange
        var customerId = 1;
        _mockCustomerService.Setup(s => s.DeleteCustomerAsync(customerId))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.DeleteCustomer(customerId);

        // Assert
        result.Should().BeOfType<NoContentResult>();
        var noContentResult = result as NoContentResult;
        noContentResult!.StatusCode.Should().Be(204);

        _mockCustomerService.Verify(s => s.DeleteCustomerAsync(customerId), Times.Once);
    }

    [Fact]
    public async Task DeleteCustomer_WhenCustomerNotExists_ReturnsNotFound()
    {
        // Arrange
        var customerId = 999;
        _mockCustomerService.Setup(s => s.DeleteCustomerAsync(customerId))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.DeleteCustomer(customerId);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
        var notFoundResult = result as NotFoundResult;
        notFoundResult!.StatusCode.Should().Be(404);

        _mockCustomerService.Verify(s => s.DeleteCustomerAsync(customerId), Times.Once);
    }
}

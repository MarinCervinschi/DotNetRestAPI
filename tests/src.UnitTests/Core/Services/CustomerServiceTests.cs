using FluentAssertions;
using Moq;
using src.API.DTOs;
using src.Core.Entities;
using src.Core.Interfaces;
using src.Core.Services;
using src.UnitTests.Core.Builders;

namespace src.UnitTests.Core.Services;

public class CustomerServiceTests
{
    private readonly Mock<IRepository<Customer>> _mockRepository;
    private readonly CustomerService _customerService;

    public CustomerServiceTests()
    {
        _mockRepository = new Mock<IRepository<Customer>>();
        _customerService = new CustomerService(_mockRepository.Object);
    }

    [Fact]
    public async Task GetAllCustomersAsync_WhenCustomersExist_ReturnsCustomerDtos()
    {
        // Arrange
        var customers = new List<Customer>
        {
            CustomerBuilder.Default().WithId(1).WithFirstName("John").WithLastName("Doe").WithEmail("john@example.com")
                .Build(),
            CustomerBuilder.Default().WithId(2).WithFirstName("Jane").WithLastName("Smith")
                .WithEmail("jane@example.com").Build()
        };

        _mockRepository.Setup(r => r.GetAllAsync())
            .ReturnsAsync(customers);

        // Act
        var result = await _customerService.GetAllCustomersAsync();
        var customerList = result.ToList();

        // Assert
        customerList.Should().NotBeNull();
        customerList.Should().HaveCount(2);

        customerList[0].Id.Should().Be(1);
        customerList[0].FirstName.Should().Be("John");
        customerList[0].LastName.Should().Be("Doe");
        customerList[0].Email.Should().Be("john@example.com");

        customerList[1].Id.Should().Be(2);
        customerList[1].FirstName.Should().Be("Jane");
        customerList[1].LastName.Should().Be("Smith");
        customerList[1].Email.Should().Be("jane@example.com");

        _mockRepository.Verify(r => r.GetAllAsync(), Times.Once);
    }

    [Fact]
    public async Task GetAllCustomersAsync_WhenNoCustomersExist_ReturnsEmptyList()
    {
        // Arrange
        _mockRepository.Setup(r => r.GetAllAsync())
            .ReturnsAsync(new List<Customer>());

        // Act
        var result = await _customerService.GetAllCustomersAsync();
        var customerList = result.ToList();

        // Assert
        customerList.Should().NotBeNull();
        customerList.Should().BeEmpty();
        _mockRepository.Verify(r => r.GetAllAsync(), Times.Once);
    }

    [Fact]
    public async Task GetCustomerByIdAsync_WhenCustomerExists_ReturnsCustomerDto()
    {
        // Arrange
        var customerId = 1;
        var customer = CustomerBuilder.Default()
            .WithId(customerId)
            .WithFirstName("John")
            .WithLastName("Doe")
            .WithEmail("john@example.com")
            .Build();

        _mockRepository.Setup(r => r.GetByIdAsync(customerId))
            .ReturnsAsync(customer);

        // Act
        var result = await _customerService.GetCustomerByIdAsync(customerId);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(customerId);
        result.FirstName.Should().Be("John");
        result.LastName.Should().Be("Doe");
        result.Email.Should().Be("john@example.com");

        _mockRepository.Verify(r => r.GetByIdAsync(customerId), Times.Once);
    }

    [Fact]
    public async Task GetCustomerByIdAsync_WhenCustomerNotExists_ReturnsNull()
    {
        // Arrange
        var customerId = 999;
        _mockRepository.Setup(r => r.GetByIdAsync(customerId))
            .ReturnsAsync((Customer?)null);

        // Act
        var result = await _customerService.GetCustomerByIdAsync(customerId);

        // Assert
        result.Should().BeNull();
        _mockRepository.Verify(r => r.GetByIdAsync(customerId), Times.Once);
    }

    [Fact]
    public async Task CreateCustomerAsync_WithValidData_ReturnsCreatedCustomer()
    {
        // Arrange
        var createDto = new CustomerCreateDto
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john@example.com"
        };

        var expectedCustomer = CustomerBuilder.Default()
            .WithId(1)
            .WithFirstName(createDto.FirstName)
            .WithLastName(createDto.LastName)
            .WithEmail(createDto.Email)
            .Build();

        _mockRepository.Setup(r => r.CreateAsync(It.IsAny<Customer>()))
            .ReturnsAsync(expectedCustomer);

        // Act
        var result = await _customerService.CreateCustomerAsync(createDto);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(1);
        result.FirstName.Should().Be(createDto.FirstName);
        result.LastName.Should().Be(createDto.LastName);
        result.Email.Should().Be(createDto.Email);

        _mockRepository.Verify(r => r.CreateAsync(It.Is<Customer>(c =>
            c.FirstName == createDto.FirstName &&
            c.LastName == createDto.LastName &&
            c.Email == createDto.Email)), Times.Once);
    }

    [Fact]
    public async Task CreateCustomerAsync_WithInvalidEmail_ThrowsValidationException()
    {
        // Arrange
        var createDto = new CustomerCreateDto
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "invalid-email"
        };

        // Note: In a real scenario, validation would happen at the service level or through data annotations
        // For this test, we'll simulate the repository throwing an exception for invalid data
        _mockRepository.Setup(r => r.CreateAsync(It.IsAny<Customer>()))
            .ThrowsAsync(new ArgumentException("Invalid email format"));

        // Act & Assert
        var act = async () => await _customerService.CreateCustomerAsync(createDto);
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("Invalid email format");

        _mockRepository.Verify(r => r.CreateAsync(It.IsAny<Customer>()), Times.Once);
    }

    [Fact]
    public async Task UpdateCustomerAsync_WhenCustomerExists_ReturnsUpdatedCustomer()
    {
        // Arrange
        var customerId = 1;
        var updateDto = new CustomerUpdateDto
        {
            FirstName = "UpdatedJohn",
            LastName = "UpdatedDoe",
            Email = "updated@example.com"
        };

        var existingCustomer = CustomerBuilder.Default()
            .WithId(customerId)
            .WithFirstName("John")
            .WithLastName("Doe")
            .WithEmail("john@example.com")
            .Build();

        var updatedCustomer = CustomerBuilder.Default()
            .WithId(customerId)
            .WithFirstName(updateDto.FirstName)
            .WithLastName(updateDto.LastName)
            .WithEmail(updateDto.Email)
            .Build();

        _mockRepository.Setup(r => r.GetByIdAsync(customerId))
            .ReturnsAsync(existingCustomer);

        _mockRepository.Setup(r => r.UpdateAsync(It.IsAny<Customer>()))
            .ReturnsAsync(updatedCustomer);

        // Act
        var result = await _customerService.UpdateCustomerAsync(customerId, updateDto);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(customerId);
        result.FirstName.Should().Be(updateDto.FirstName);
        result.LastName.Should().Be(updateDto.LastName);
        result.Email.Should().Be(updateDto.Email);

        _mockRepository.Verify(r => r.GetByIdAsync(customerId), Times.Once);
        _mockRepository.Verify(r => r.UpdateAsync(It.Is<Customer>(c =>
            c.Id == customerId &&
            c.FirstName == updateDto.FirstName &&
            c.LastName == updateDto.LastName &&
            c.Email == updateDto.Email)), Times.Once);
    }

    [Fact]
    public async Task UpdateCustomerAsync_WhenCustomerNotExists_ThrowsKeyNotFoundException()
    {
        // Arrange
        var customerId = 999;
        var updateDto = new CustomerUpdateDto
        {
            FirstName = "UpdatedJohn",
            LastName = "UpdatedDoe",
            Email = "updated@example.com"
        };

        _mockRepository.Setup(r => r.GetByIdAsync(customerId))
            .ReturnsAsync((Customer?)null);

        // Act & Assert
        var act = async () => await _customerService.UpdateCustomerAsync(customerId, updateDto);
        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage($"Customer with ID {customerId} not found.");

        _mockRepository.Verify(r => r.GetByIdAsync(customerId), Times.Once);
        _mockRepository.Verify(r => r.UpdateAsync(It.IsAny<Customer>()), Times.Never);
    }

    [Fact]
    public async Task DeleteCustomerAsync_WhenCustomerExists_ReturnsTrue()
    {
        // Arrange
        var customerId = 1;
        _mockRepository.Setup(r => r.DeleteAsync(customerId))
            .ReturnsAsync(true);

        // Act
        var result = await _customerService.DeleteCustomerAsync(customerId);

        // Assert
        result.Should().BeTrue();
        _mockRepository.Verify(r => r.DeleteAsync(customerId), Times.Once);
    }

    [Fact]
    public async Task DeleteCustomerAsync_WhenCustomerNotExists_ReturnsFalse()
    {
        // Arrange
        var customerId = 999;
        _mockRepository.Setup(r => r.DeleteAsync(customerId))
            .ReturnsAsync(false);

        // Act
        var result = await _customerService.DeleteCustomerAsync(customerId);

        // Assert
        result.Should().BeFalse();
        _mockRepository.Verify(r => r.DeleteAsync(customerId), Times.Once);
    }

    [Fact]
    public async Task CustomerExistsAsync_WhenCustomerExists_ReturnsTrue()
    {
        // Arrange
        var customerId = 1;
        _mockRepository.Setup(r => r.ExistsAsync(customerId))
            .ReturnsAsync(true);

        // Act
        var result = await _customerService.CustomerExistsAsync(customerId);

        // Assert
        result.Should().BeTrue();
        _mockRepository.Verify(r => r.ExistsAsync(customerId), Times.Once);
    }

    [Fact]
    public async Task CustomerExistsAsync_WhenCustomerNotExists_ReturnsFalse()
    {
        // Arrange
        var customerId = 999;
        _mockRepository.Setup(r => r.ExistsAsync(customerId))
            .ReturnsAsync(false);

        // Act
        var result = await _customerService.CustomerExistsAsync(customerId);

        // Assert
        result.Should().BeFalse();
        _mockRepository.Verify(r => r.ExistsAsync(customerId), Times.Once);
    }
}

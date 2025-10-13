using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using src.Core.Entities;
using src.Core.Interfaces;
using src.IntegrationTests.Base;
using src.UnitTests.Core.Builders;

namespace src.IntegrationTests.Repositories;

public class CustomerRepositoryTests : IntegrationTestBase
{
    private IRepository<Customer> _customerRepository = null!;

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        _customerRepository = Factory.Services.GetRequiredService<IRepository<Customer>>();
    }

    [Fact]
    public async Task CreateAsync_WithValidCustomer_ShouldPersistToDatabase()
    {
        // Arrange
        var customer = CustomerBuilder.New()
            .WithFirstName("John")
            .WithLastName("Doe")
            .WithEmail("john.doe@test.com")
            .Build();

        // Act
        var createdCustomer = await _customerRepository.CreateAsync(customer);

        // Assert
        createdCustomer.Should().NotBeNull();
        createdCustomer.Id.Should().BeGreaterThan(0);
        createdCustomer.FirstName.Should().Be("John");
        createdCustomer.LastName.Should().Be("Doe");
        createdCustomer.Email.Should().Be("john.doe@test.com");

        // Verify persistence
        var retrievedCustomer = await _customerRepository.GetByIdAsync(createdCustomer.Id);
        retrievedCustomer.Should().NotBeNull();
        retrievedCustomer!.FirstName.Should().Be("John");
    }

    [Fact]
    public async Task GetAllAsync_WithMultipleCustomers_ShouldReturnAllCustomers()
    {
        // Arrange
        var customer1 = CustomerBuilder.New().WithFirstName("Customer").WithLastName("One").WithEmail("customer1@test.com").Build();
        var customer2 = CustomerBuilder.New().WithFirstName("Customer").WithLastName("Two").WithEmail("customer2@test.com").Build();
        var customer3 = CustomerBuilder.New().WithFirstName("Customer").WithLastName("Three").WithEmail("customer3@test.com").Build();

        await _customerRepository.CreateAsync(customer1);
        await _customerRepository.CreateAsync(customer2);
        await _customerRepository.CreateAsync(customer3);

        // Act
        var result = await _customerRepository.GetAllAsync();

        // Assert
        var customerList = result.ToList();
        customerList.Should().HaveCount(c => c >= 3);
        customerList.Should().Contain(c => c.FirstName == "Customer" && c.LastName == "One");
        customerList.Should().Contain(c => c.FirstName == "Customer" && c.LastName == "Two");
        customerList.Should().Contain(c => c.FirstName == "Customer" && c.LastName == "Three");
    }

    [Fact]
    public async Task UpdateAsync_WithValidChanges_ShouldPersistChanges()
    {
        // Arrange
        var customer = CustomerBuilder.New()
            .WithFirstName("Original")
            .WithLastName("Name")
            .WithEmail("original@test.com")
            .Build();

        var createdCustomer = await _customerRepository.CreateAsync(customer);
        createdCustomer.FirstName = "Updated";
        createdCustomer.LastName = "Name";

        // Act
        var updatedCustomer = await _customerRepository.UpdateAsync(createdCustomer);

        // Assert
        updatedCustomer.FirstName.Should().Be("Updated");
        updatedCustomer.LastName.Should().Be("Name");

        // Verify persistence
        var retrievedCustomer = await _customerRepository.GetByIdAsync(createdCustomer.Id);
        retrievedCustomer!.FirstName.Should().Be("Updated");
        retrievedCustomer.LastName.Should().Be("Name");
    }

    [Fact]
    public async Task DeleteAsync_WithExistingId_ShouldRemoveFromDatabase()
    {
        // Arrange
        var customer = CustomerBuilder.New()
            .WithFirstName("Customer")
            .WithLastName("ToDelete")
            .WithEmail("todelete@test.com")
            .Build();

        var createdCustomer = await _customerRepository.CreateAsync(customer);

        // Act
        var deleteResult = await _customerRepository.DeleteAsync(createdCustomer.Id);

        // Assert
        deleteResult.Should().BeTrue();

        // Verify removal
        var retrievedCustomer = await _customerRepository.GetByIdAsync(createdCustomer.Id);
        retrievedCustomer.Should().BeNull();
    }

    [Fact]
    public async Task DeleteAsync_WithNonExistentId_ShouldReturnFalse()
    {
        // Act
        var result = await _customerRepository.DeleteAsync(99999);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task ExistsAsync_WithExistingId_ShouldReturnTrue()
    {
        // Arrange
        var customer = CustomerBuilder.New().WithFirstName("Exists").WithLastName("Test").WithEmail("exists@test.com").Build();
        var createdCustomer = await _customerRepository.CreateAsync(customer);

        // Act
        var exists = await _customerRepository.ExistsAsync(createdCustomer.Id);

        // Assert
        exists.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsAsync_WithNonExistentId_ShouldReturnFalse()
    {
        // Act
        var exists = await _customerRepository.ExistsAsync(99999);

        // Assert
        exists.Should().BeFalse();
    }

    [Fact]
    public async Task GetByIdAsync_WithExistingId_ShouldReturnCorrectCustomer()
    {
        // Arrange
        var customer = CustomerBuilder.New()
            .WithFirstName("Specific")
            .WithLastName("Customer")
            .WithEmail("specific@test.com")
            .Build();

        var createdCustomer = await _customerRepository.CreateAsync(customer);

        // Act
        var result = await _customerRepository.GetByIdAsync(createdCustomer.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(createdCustomer.Id);
        result.FirstName.Should().Be("Specific");
        result.LastName.Should().Be("Customer");
        result.Email.Should().Be("specific@test.com");
    }

    [Fact]
    public async Task GetByIdAsync_WithNonExistentId_ShouldReturnNull()
    {
        // Act
        var result = await _customerRepository.GetByIdAsync(99999);

        // Assert
        result.Should().BeNull();
    }
}

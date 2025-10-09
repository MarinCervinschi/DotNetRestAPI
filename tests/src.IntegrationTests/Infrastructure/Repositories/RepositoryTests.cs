using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using src.Core.Entities;
using src.Infrastructure.Data;
using src.Infrastructure.Repositories;

namespace src.IntegrationTests.Infrastructure.Repositories;

public class RepositoryTests : IAsyncLifetime
{
    private ApplicationDbContext _context = null!;
    private Repository<Customer> _repository = null!;

    public async Task InitializeAsync()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);
        await _context.Database.EnsureCreatedAsync();
        _repository = new Repository<Customer>(_context);
    }

    public async Task DisposeAsync()
    {
        await _context.Database.EnsureDeletedAsync();
        await _context.DisposeAsync();
    }

    [Fact]
    public async Task CreateAsync_ShouldInsertCustomerIntoDatabase()
    {
        // Arrange
        var customer = new Customer
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@example.com"
        };

        // Act
        var result = await _repository.CreateAsync(customer);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().BeGreaterThan(0);
        result.FirstName.Should().Be("John");
        result.LastName.Should().Be("Doe");
        result.Email.Should().Be("john.doe@example.com");

        // Verify it's actually in the database
        var customerFromDb = await _context.Customers.FindAsync(result.Id);
        customerFromDb.Should().NotBeNull();
        customerFromDb.FirstName.Should().Be("John");
    }

    [Fact]
    public async Task GetByIdAsync_WhenCustomerExists_ShouldReturnCustomer()
    {
        // Arrange
        var customer = new Customer
        {
            FirstName = "Jane",
            LastName = "Smith",
            Email = "jane.smith@example.com"
        };

        _context.Customers.Add(customer);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByIdAsync(customer.Id);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(customer.Id);
        result.FirstName.Should().Be("Jane");
        result.LastName.Should().Be("Smith");
        result.Email.Should().Be("jane.smith@example.com");
    }

    [Fact]
    public async Task GetByIdAsync_WhenCustomerDoesNotExist_ShouldReturnNull()
    {
        // Arrange
        var nonExistentId = 999;

        // Act
        var result = await _repository.GetByIdAsync(nonExistentId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetAllAsync_WhenCustomersExist_ShouldReturnAllCustomers()
    {
        // Arrange
        var customers = new List<Customer>
        {
            new() { FirstName = "John", LastName = "Doe", Email = "john@example.com" },
            new() { FirstName = "Jane", LastName = "Smith", Email = "jane@example.com" },
            new() { FirstName = "Bob", LastName = "Johnson", Email = "bob@example.com" }
        };

        _context.Customers.AddRange(customers);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetAllAsync();
        var customerList = result.ToList();

        // Assert
        customerList.Should().HaveCount(3);
        customerList.Should().Contain(c => c.FirstName == "John" && c.LastName == "Doe");
        customerList.Should().Contain(c => c.FirstName == "Jane" && c.LastName == "Smith");
        customerList.Should().Contain(c => c.FirstName == "Bob" && c.LastName == "Johnson");
    }

    [Fact]
    public async Task GetAllAsync_WhenNoCustomersExist_ShouldReturnEmptyList()
    {
        // Act
        var result = await _repository.GetAllAsync();
        var customerList = result.ToList();

        // Assert
        customerList.Should().BeEmpty();
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateCustomerInDatabase()
    {
        // Arrange - Create and insert a customer
        var customer = new Customer
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@example.com"
        };

        _context.Customers.Add(customer);
        await _context.SaveChangesAsync();

        // Modify the customer
        customer.FirstName = "UpdatedJohn";
        customer.LastName = "UpdatedDoe";
        customer.Email = "updated.john@example.com";

        // Act
        var result = await _repository.UpdateAsync(customer);

        // Assert
        result.Should().NotBeNull();
        result.FirstName.Should().Be("UpdatedJohn");
        result.LastName.Should().Be("UpdatedDoe");
        result.Email.Should().Be("updated.john@example.com");

        // Verify it's actually updated in the database
        var customerFromDb = await _context.Customers.FindAsync(customer.Id);
        customerFromDb.Should().NotBeNull();
        customerFromDb.FirstName.Should().Be("UpdatedJohn");
        customerFromDb.LastName.Should().Be("UpdatedDoe");
        customerFromDb.Email.Should().Be("updated.john@example.com");
    }

    [Fact]
    public async Task DeleteAsync_WhenCustomerExists_ShouldRemoveFromDatabaseAndReturnTrue()
    {
        // Arrange
        var customer = new Customer
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@example.com"
        };

        _context.Customers.Add(customer);
        await _context.SaveChangesAsync();
        var customerId = customer.Id;

        // Act
        var result = await _repository.DeleteAsync(customerId);

        // Assert
        result.Should().BeTrue();

        // Verify it's actually removed from the database
        var customerFromDb = await _context.Customers.FindAsync(customerId);
        customerFromDb.Should().BeNull();
    }

    [Fact]
    public async Task DeleteAsync_WhenCustomerDoesNotExist_ShouldReturnFalse()
    {
        // Arrange
        var nonExistentId = 999;

        // Act
        var result = await _repository.DeleteAsync(nonExistentId);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task ExistsAsync_WhenCustomerExists_ShouldReturnTrue()
    {
        // Arrange
        var customer = new Customer
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@example.com"
        };

        _context.Customers.Add(customer);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.ExistsAsync(customer.Id);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsAsync_WhenCustomerDoesNotExist_ShouldReturnFalse()
    {
        // Arrange
        var nonExistentId = 999;

        // Act
        var result = await _repository.ExistsAsync(nonExistentId);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task Repository_ShouldWorkWithEntityFrameworkQueries()
    {
        // Arrange - Add customers with different attributes
        var customers = new List<Customer>
        {
            new() { FirstName = "John", LastName = "Doe", Email = "john@example.com" },
            new() { FirstName = "Jane", LastName = "Doe", Email = "jane@example.com" },
            new() { FirstName = "Bob", LastName = "Smith", Email = "bob@smith.com" }
        };

        _context.Customers.AddRange(customers);
        await _context.SaveChangesAsync();

        // Act - Test complex queries work with the repository's context
        var doeCustomers = await _context.Customers
            .Where(c => c.LastName == "Doe")
            .ToListAsync();

        var smithCustomers = await _context.Customers
            .Where(c => c.LastName == "Smith")
            .ToListAsync();

        // Assert
        doeCustomers.Should().HaveCount(2);
        doeCustomers.Should().AllSatisfy(c => c.LastName.Should().Be("Doe"));

        smithCustomers.Should().HaveCount(1);
        smithCustomers[0].FirstName.Should().Be("Bob");
        smithCustomers[0].Email.Should().Be("bob@smith.com");
    }

    [Fact]
    public async Task Repository_ShouldHandleConcurrentOperations()
    {
        // Arrange
        var customer1 = new Customer { FirstName = "John", LastName = "Doe", Email = "john@example.com" };
        var customer2 = new Customer { FirstName = "Jane", LastName = "Smith", Email = "jane@example.com" };

        // Act - Perform concurrent operations
        var task1 = _repository.CreateAsync(customer1);
        var task2 = _repository.CreateAsync(customer2);

        var results = await Task.WhenAll(task1, task2);

        // Assert
        results.Should().HaveCount(2);
        results[0].Should().NotBeNull();
        results[1].Should().NotBeNull();
        results[0].Id.Should().BeGreaterThan(0);
        results[1].Id.Should().BeGreaterThan(0);
        results[0].Id.Should().NotBe(results[1].Id);

        // Verify both are in database
        var allCustomers = await _repository.GetAllAsync();
        allCustomers.Should().HaveCount(2);
    }
}
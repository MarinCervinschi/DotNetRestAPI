using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using src.Core.Entities;
using src.Infrastructure.Data;
using src.Infrastructure.Repositories;
using src.UnitTests.Core.Builders;

namespace src.UnitTests.Infrastructure.Repositories;

public class RepositoryTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly Repository<Customer> _repository;

    public RepositoryTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);
        _repository = new Repository<Customer>(_context);
    }

    [Fact]
    public async Task GetByIdAsync_WhenEntityExists_ReturnsEntity()
    {
        // Arrange
        var customer = CustomerBuilder.Default()
            .WithFirstName("Test")
            .WithLastName("Customer")
            .WithEmail("test@test.com")
            .Build();
        await _context.Customers.AddAsync(customer);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByIdAsync(customer.Id);

        // Assert
        result.Should().NotBeNull();
        result!.FirstName.Should().Be("Test");
        result.LastName.Should().Be("Customer");
    }

    [Fact]
    public async Task GetByIdAsync_WhenEntityDoesNotExist_ReturnsNull()
    {
        // Act
        var result = await _repository.GetByIdAsync(999);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task CreateAsync_ShouldAddEntityToDatabase()
    {
        // Arrange
        var customer = CustomerBuilder.Default()
            .WithFirstName("New")
            .WithLastName("Customer")
            .WithEmail("new@test.com")
            .Build();

        // Act
        var result = await _repository.CreateAsync(customer);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().BeGreaterThan(0);

        var savedEntity = await _context.Customers.FindAsync(result.Id);
        savedEntity.Should().NotBeNull();
        savedEntity!.FirstName.Should().Be("New");
        savedEntity.LastName.Should().Be("Customer");
    }

    [Fact]
    public async Task DeleteAsync_WhenEntityExists_ReturnsTrue()
    {
        // Arrange
        var customer = CustomerBuilder.Default()
            .WithFirstName("To")
            .WithLastName("Delete")
            .WithEmail("delete@test.com")
            .Build();
        await _context.Customers.AddAsync(customer);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.DeleteAsync(customer.Id);

        // Assert
        result.Should().BeTrue();

        var deletedEntity = await _context.Customers.FindAsync(customer.Id);
        deletedEntity.Should().BeNull();
    }

    [Fact]
    public async Task DeleteAsync_WhenEntityDoesNotExist_ReturnsFalse()
    {
        // Act
        var result = await _repository.DeleteAsync(999);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task ExistsAsync_WhenEntityExists_ReturnsTrue()
    {
        // Arrange
        var customer = CustomerBuilder.Default()
            .WithFirstName("Exists")
            .WithLastName("Test")
            .WithEmail("exists@test.com")
            .Build();
        await _context.Customers.AddAsync(customer);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.ExistsAsync(customer.Id);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsAsync_WhenEntityDoesNotExist_ReturnsFalse()
    {
        // Act
        var result = await _repository.ExistsAsync(999);

        // Assert
        result.Should().BeFalse();
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
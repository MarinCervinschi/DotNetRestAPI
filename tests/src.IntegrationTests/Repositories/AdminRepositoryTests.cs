using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using src.Core.Interfaces.Repositories;
using src.IntegrationTests.Base;
using src.UnitTests.Core.Builders;

namespace src.IntegrationTests.Repositories;

public class AdminRepositoryTests : IntegrationTestBase
{
    private IAdminRepository _adminRepository = null!;

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        _adminRepository = Factory.Services.GetRequiredService<IAdminRepository>();
    }

    [Fact]
    public async Task CreateAsync_WithValidAdmin_ShouldPersistToDatabase()
    {
        // Arrange
        var admin = AdminBuilder.New()
            .WithUsername("newadmin")
            .WithEmail("newadmin@test.com")
            .WithPasswordHash("hashedpassword123")
            .Build();

        // Act
        var createdAdmin = await _adminRepository.CreateAsync(admin);

        // Assert
        createdAdmin.Should().NotBeNull();
        createdAdmin.Id.Should().BeGreaterThan(0);
        createdAdmin.Username.Should().Be("newadmin");
        createdAdmin.Email.Should().Be("newadmin@test.com");

        // Verify persistence
        var retrievedAdmin = await _adminRepository.GetByIdAsync(createdAdmin.Id);
        retrievedAdmin.Should().NotBeNull();
        retrievedAdmin!.Username.Should().Be("newadmin");
    }

    [Fact]
    public async Task GetByUsernameAsync_WithExistingUsername_ShouldReturnAdmin()
    {
        // Arrange
        var admin = AdminBuilder.New()
            .WithUsername("testuser")
            .WithEmail("testuser@test.com")
            .Build();

        var createdAdmin = await _adminRepository.CreateAsync(admin);

        // Act
        var result = await _adminRepository.GetByUsernameAsync("testuser");

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(createdAdmin.Id);
        result.Username.Should().Be("testuser");
        result.Email.Should().Be("testuser@test.com");
    }

    [Fact]
    public async Task GetByUsernameAsync_WithNonExistentUsername_ShouldReturnNull()
    {
        // Act
        var result = await _adminRepository.GetByUsernameAsync("nonexistent");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetAllAsync_WithMultipleAdmins_ShouldReturnAllAdmins()
    {
        // Arrange
        var admin1 = AdminBuilder.New().WithUsername("admin1").WithEmail("admin1@test.com").Build();
        var admin2 = AdminBuilder.New().WithUsername("admin2").WithEmail("admin2@test.com").Build();

        await _adminRepository.CreateAsync(admin1);
        await _adminRepository.CreateAsync(admin2);

        // Act
        var result = await _adminRepository.GetAllAsync();

        // Assert
        var adminList = result.ToList();
        adminList.Should().HaveCount(c => c >= 2);
        adminList.Should().Contain(a => a.Username == "admin1");
        adminList.Should().Contain(a => a.Username == "admin2");
    }

    [Fact]
    public async Task UpdateAsync_WithValidChanges_ShouldPersistChanges()
    {
        // Arrange
        var admin = AdminBuilder.New()
            .WithUsername("updatetest")
            .WithEmail("original@test.com")
            .Build();

        var createdAdmin = await _adminRepository.CreateAsync(admin);
        createdAdmin.Email = "updated@test.com";

        // Act
        var updatedAdmin = await _adminRepository.UpdateAsync(createdAdmin);

        // Assert
        updatedAdmin.Email.Should().Be("updated@test.com");

        // Verify persistence
        var retrievedAdmin = await _adminRepository.GetByIdAsync(createdAdmin.Id);
        retrievedAdmin!.Email.Should().Be("updated@test.com");
    }

    [Fact]
    public async Task DeleteAsync_WithExistingId_ShouldRemoveFromDatabase()
    {
        // Arrange
        var admin = AdminBuilder.New()
            .WithUsername("todelete")
            .WithEmail("todelete@test.com")
            .Build();

        var createdAdmin = await _adminRepository.CreateAsync(admin);

        // Act
        var deleteResult = await _adminRepository.DeleteAsync(createdAdmin.Id);

        // Assert
        deleteResult.Should().BeTrue();

        // Verify removal
        var retrievedAdmin = await _adminRepository.GetByIdAsync(createdAdmin.Id);
        retrievedAdmin.Should().BeNull();
    }

    [Fact]
    public async Task DeleteAsync_WithNonExistentId_ShouldReturnFalse()
    {
        // Act
        var result = await _adminRepository.DeleteAsync(99999);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task ExistsAsync_WithExistingId_ShouldReturnTrue()
    {
        // Arrange
        var admin = AdminBuilder.New().WithUsername("existstest").Build();
        var createdAdmin = await _adminRepository.CreateAsync(admin);

        // Act
        var exists = await _adminRepository.ExistsAsync(createdAdmin.Id);

        // Assert
        exists.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsAsync_WithNonExistentId_ShouldReturnFalse()
    {
        // Act
        var exists = await _adminRepository.ExistsAsync(99999);

        // Assert
        exists.Should().BeFalse();
    }
}
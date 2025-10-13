using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using src.Core.Entities;
using src.Core.Interfaces;
using src.IntegrationTests.Base;
using src.UnitTests.Core.Builders;

namespace src.IntegrationTests.Repositories;

public class BookRepositoryTests : IntegrationTestBase
{
    private IRepository<Book> _bookRepository = null!;

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        _bookRepository = Factory.Services.GetRequiredService<IRepository<Book>>();
    }

    [Fact]
    public async Task CreateAsync_WithValidBook_ShouldPersistToDatabase()
    {
        // Arrange
        var book = BookBuilder.New()
            .WithTitle("Integration Test Book")
            .WithAuthor("Test Author")
            .WithIsbn("1234567890123")
            .WithAvailableStatus()
            .Build();

        // Act
        var createdBook = await _bookRepository.CreateAsync(book);

        // Assert
        createdBook.Should().NotBeNull();
        createdBook.Id.Should().BeGreaterThan(0);
        createdBook.Title.Should().Be("Integration Test Book");
        createdBook.Author.Should().Be("Test Author");
        createdBook.ISBN.Should().Be("1234567890123");
        createdBook.Status.Should().Be(BookStatus.Available);

        // Verify persistence
        var retrievedBook = await _bookRepository.GetByIdAsync(createdBook.Id);
        retrievedBook.Should().NotBeNull();
        retrievedBook!.Title.Should().Be("Integration Test Book");
    }

    [Fact]
    public async Task GetAllAsync_WithMultipleBooks_ShouldReturnAllBooks()
    {
        // Arrange
        var book1 = BookBuilder.New().WithTitle("Book 1").WithIsbn("1111111111111").Build();
        var book2 = BookBuilder.New().WithTitle("Book 2").WithIsbn("2222222222222").Build();
        var book3 = BookBuilder.New().WithTitle("Book 3").WithIsbn("3333333333333").Build();
        
        await _bookRepository.CreateAsync(book1);
        await _bookRepository.CreateAsync(book2);
        await _bookRepository.CreateAsync(book3);

        // Act
        var result = await _bookRepository.GetAllAsync();

        // Assert
        var bookList = result.ToList();
        bookList.Should().HaveCount(c => c >= 3);
        bookList.Should().Contain(b => b.Title == "Book 1");
        bookList.Should().Contain(b => b.Title == "Book 2");
        bookList.Should().Contain(b => b.Title == "Book 3");
    }

    [Fact]
    public async Task UpdateAsync_WithValidChanges_ShouldPersistChanges()
    {
        // Arrange
        var book = BookBuilder.New()
            .WithTitle("Original Title")
            .WithAuthor("Original Author")
            .WithIsbn("9999999999999")
            .WithAvailableStatus()
            .Build();
        
        var createdBook = await _bookRepository.CreateAsync(book);
        createdBook.Title = "Updated Title";
        createdBook.Status = BookStatus.Unavailable;

        // Act
        var updatedBook = await _bookRepository.UpdateAsync(createdBook);

        // Assert
        updatedBook.Title.Should().Be("Updated Title");
        updatedBook.Status.Should().Be(BookStatus.Unavailable);

        // Verify persistence
        var retrievedBook = await _bookRepository.GetByIdAsync(createdBook.Id);
        retrievedBook!.Title.Should().Be("Updated Title");
        retrievedBook.Status.Should().Be(BookStatus.Unavailable);
    }

    [Fact]
    public async Task DeleteAsync_WithExistingId_ShouldRemoveFromDatabase()
    {
        // Arrange
        var book = BookBuilder.New()
            .WithTitle("Book to Delete")
            .WithIsbn("0000000000000")
            .Build();
        
        var createdBook = await _bookRepository.CreateAsync(book);

        // Act
        var deleteResult = await _bookRepository.DeleteAsync(createdBook.Id);

        // Assert
        deleteResult.Should().BeTrue();

        // Verify removal
        var retrievedBook = await _bookRepository.GetByIdAsync(createdBook.Id);
        retrievedBook.Should().BeNull();
    }

    [Fact]
    public async Task DeleteAsync_WithNonExistentId_ShouldReturnFalse()
    {
        // Act
        var result = await _bookRepository.DeleteAsync(99999);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task ExistsAsync_WithExistingId_ShouldReturnTrue()
    {
        // Arrange
        var book = BookBuilder.New().WithTitle("Exists Test").WithIsbn("5555555555555").Build();
        var createdBook = await _bookRepository.CreateAsync(book);

        // Act
        var exists = await _bookRepository.ExistsAsync(createdBook.Id);

        // Assert
        exists.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsAsync_WithNonExistentId_ShouldReturnFalse()
    {
        // Act
        var exists = await _bookRepository.ExistsAsync(99999);

        // Assert
        exists.Should().BeFalse();
    }

    [Fact]
    public async Task GetByIdAsync_WithExistingId_ShouldReturnCorrectBook()
    {
        // Arrange
        var book = BookBuilder.New()
            .WithTitle("Specific Book")
            .WithAuthor("Specific Author")
            .WithIsbn("7777777777777")
            .Build();
        
        var createdBook = await _bookRepository.CreateAsync(book);

        // Act
        var result = await _bookRepository.GetByIdAsync(createdBook.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(createdBook.Id);
        result.Title.Should().Be("Specific Book");
        result.Author.Should().Be("Specific Author");
        result.ISBN.Should().Be("7777777777777");
    }

    [Fact]
    public async Task GetByIdAsync_WithNonExistentId_ShouldReturnNull()
    {
        // Act
        var result = await _bookRepository.GetByIdAsync(99999);

        // Assert
        result.Should().BeNull();
    }
}

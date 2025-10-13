using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using src.API.DTOs;
using src.Core.Entities;
using src.Core.Interfaces;
using src.IntegrationTests.Base;
using src.UnitTests.Core.Builders;
using System.Net;
using src.Core.Interfaces.Repositories;
using Xunit;

namespace src.IntegrationTests.API;

public class BooksControllerTests : IntegrationTestBase
{
    private IRepository<Book> _bookRepository = null!;
    private IAdminRepository _adminRepository = null!;

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        _bookRepository = Factory.Services.GetRequiredService<IRepository<Book>>();
        _adminRepository = Factory.Services.GetRequiredService<IAdminRepository>();

        // Setup admin and authenticate for protected endpoints if needed
        await SetupAuthenticationAsync();
    }

    private async Task SetupAuthenticationAsync()
    {
        var passwordHash = BCrypt.Net.BCrypt.HashPassword("password123");
        var admin = AdminBuilder.New()
            .WithUsername("testadmin")
            .WithPasswordHash(passwordHash)
            .Build();

        await _adminRepository.CreateAsync(admin);
        SetAuthenticationHeader(admin.Id, admin.Username);
    }

    [Fact]
    public async Task GetAllBooks_ShouldReturnAllBooks()
    {
        // Arrange
        var book1 = BookBuilder.New().WithTitle("Book 1").WithAuthor("Author 1").WithIsbn("1234567890").Build();
        var book2 = BookBuilder.New().WithTitle("Book 2").WithAuthor("Author 2").WithIsbn("0987654321").Build();

        await _bookRepository.CreateAsync(book1);
        await _bookRepository.CreateAsync(book2);

        // Act
        var response = await HttpClient.GetAsync("/Books");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var books = await DeserializeResponseAsync<IEnumerable<BookDto>>(response);
        books.Should().NotBeNull();
        var booksList = books!.ToList();
        booksList.Should().HaveCountGreaterThanOrEqualTo(2);
    }

    [Fact]
    public async Task GetBook_WithValidId_ShouldReturnBook()
    {
        // Arrange
        var book = BookBuilder.New()
            .WithTitle("Test Book")
            .WithAuthor("Test Author")
            .WithIsbn("1234567890")
            .Build();

        var createdBook = await _bookRepository.CreateAsync(book);

        // Act
        var response = await HttpClient.GetAsync($"/Books/{createdBook.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var returnedBook = await DeserializeResponseAsync<BookDto>(response);
        returnedBook.Should().NotBeNull();
        returnedBook.Id.Should().Be(createdBook.Id);
        returnedBook.Title.Should().Be("Test Book");
        returnedBook.Author.Should().Be("Test Author");
        returnedBook.ISBN.Should().Be("1234567890");
    }

    [Fact]
    public async Task GetBook_WithInvalidId_ShouldReturnNotFound()
    {
        // Act
        var response = await HttpClient.GetAsync("/Books/99999");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task SearchBooks_ByTitle_ShouldReturnMatchingBooks()
    {
        // Arrange
        var book1 = BookBuilder.New().WithTitle("C# Programming").WithAuthor("Author 1").WithIsbn("1111111111").Build();
        var book2 = BookBuilder.New().WithTitle("Java Programming").WithAuthor("Author 2").WithIsbn("2222222222")
            .Build();
        var book3 = BookBuilder.New().WithTitle("Python Guide").WithAuthor("Author 3").WithIsbn("3333333333").Build();

        await _bookRepository.CreateAsync(book1);
        await _bookRepository.CreateAsync(book2);
        await _bookRepository.CreateAsync(book3);

        // Act
        var response = await HttpClient.GetAsync("/Books/search?title=Programming");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var books = await DeserializeResponseAsync<IEnumerable<BookDto>>(response);
        books.Should().NotBeNull();
        var booksList = books!.ToList();
        booksList.Should().HaveCount(2);
        booksList.Should().OnlyContain(b => b.Title.Contains("Programming"));
    }

    [Fact]
    public async Task SearchBooks_ByAuthor_ShouldReturnMatchingBooks()
    {
        // Arrange
        var book1 = BookBuilder.New().WithTitle("Book 1").WithAuthor("John Smith").WithIsbn("1111111111").Build();
        var book2 = BookBuilder.New().WithTitle("Book 2").WithAuthor("Jane Doe").WithIsbn("2222222222").Build();

        await _bookRepository.CreateAsync(book1);
        await _bookRepository.CreateAsync(book2);

        // Act
        var response = await HttpClient.GetAsync("/Books/search?author=John");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var books = await DeserializeResponseAsync<IEnumerable<BookDto>>(response);
        books.Should().NotBeNull();
        var booksList = books!.ToList();
        booksList.Should().HaveCount(1);
        booksList.First().Author.Should().Contain("John");
    }

    [Fact]
    public async Task SearchBooks_ByStatus_ShouldReturnMatchingBooks()
    {
        // Arrange
        var availableBook = BookBuilder.New().WithTitle("Available Book").WithIsbn("1111111111")
            .WithStatus(BookStatus.Available).Build();
        var unavailableBook = BookBuilder.New().WithTitle("Unavailable Book").WithIsbn("2222222222")
            .WithStatus(BookStatus.Unavailable).Build();

        await _bookRepository.CreateAsync(availableBook);
        await _bookRepository.CreateAsync(unavailableBook);

        // Act
        var response = await HttpClient.GetAsync("/Books/search?status=Available");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var books = await DeserializeResponseAsync<IEnumerable<BookDto>>(response);
        books.Should().NotBeNull();
        var booksList = books!.ToList();
        booksList.Should().OnlyContain(b => b.Status == BookStatus.Available);
    }

    [Fact]
    public async Task CreateBook_WithValidData_ShouldReturnCreated()
    {
        // Arrange
        var bookCreateDto = new BookCreateDto
        {
            Title = "New Test Book",
            Author = "Test Author",
            ISBN = "9876543210",
            Status = BookStatus.Available
        };

        // Act
        var response = await HttpClient.PostAsync("/Books", CreateJsonContent(bookCreateDto));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var createdBook = await DeserializeResponseAsync<BookDto>(response);
        createdBook.Should().NotBeNull();
        createdBook.Title.Should().Be("New Test Book");
        createdBook.Author.Should().Be("Test Author");
        createdBook.ISBN.Should().Be("9876543210");
        createdBook.Status.Should().Be(BookStatus.Available);

        // Verify location header
        response.Headers.Location.Should().NotBeNull();
        response.Headers.Location!.ToString().Should().Contain($"/Books/{createdBook.Id}");
    }

    [Fact]
    public async Task CreateBook_WithDuplicateISBN_ShouldReturnConflict()
    {
        // Arrange
        var existingBook = BookBuilder.New().WithIsbn("1234567890").Build();
        await _bookRepository.CreateAsync(existingBook);

        var bookCreateDto = new BookCreateDto
        {
            Title = "Another Book",
            Author = "Another Author",
            ISBN = "1234567890", // Same ISBN
            Status = BookStatus.Available
        };

        // Act
        var response = await HttpClient.PostAsync("/Books", CreateJsonContent(bookCreateDto));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task CreateBook_WithInvalidData_ShouldReturnBadRequest()
    {
        // Arrange
        var bookCreateDto = new BookCreateDto
        {
            Title = "", // Invalid - empty title
            Author = "Test Author",
            ISBN = "123", // Invalid - too short
            Status = BookStatus.Available
        };

        // Act
        var response = await HttpClient.PostAsync("/Books", CreateJsonContent(bookCreateDto));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UpdateBook_WithValidData_ShouldReturnUpdatedBook()
    {
        // Arrange
        var book = BookBuilder.New()
            .WithTitle("Original Title")
            .WithAuthor("Original Author")
            .WithIsbn("1234567890")
            .Build();

        var createdBook = await _bookRepository.CreateAsync(book);

        var bookUpdateDto = new BookUpdateDto
        {
            Title = "Updated Title",
            Author = "Updated Author",
            ISBN = "0987654321",
            Status = BookStatus.Unavailable
        };

        // Act
        var response = await HttpClient.PutAsync($"/Books/{createdBook.Id}", CreateJsonContent(bookUpdateDto));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var updatedBook = await DeserializeResponseAsync<BookDto>(response);
        updatedBook.Should().NotBeNull();
        updatedBook.Id.Should().Be(createdBook.Id);
        updatedBook.Title.Should().Be("Updated Title");
        updatedBook.Author.Should().Be("Updated Author");
        updatedBook.ISBN.Should().Be("0987654321");
        updatedBook.Status.Should().Be(BookStatus.Unavailable);
    }

    [Fact]
    public async Task UpdateBook_WithInvalidId_ShouldReturnNotFound()
    {
        // Arrange
        var bookUpdateDto = new BookUpdateDto
        {
            Title = "Updated Title",
            Author = "Updated Author",
            ISBN = "0987654321",
            Status = BookStatus.Available
        };

        // Act
        var response = await HttpClient.PutAsync("/Books/99999", CreateJsonContent(bookUpdateDto));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateBook_WithDuplicateISBN_ShouldReturnConflict()
    {
        // Arrange
        var book1 = BookBuilder.New().WithIsbn("1111111111").Build();
        var book2 = BookBuilder.New().WithIsbn("2222222222").Build();

        await _bookRepository.CreateAsync(book1);
        var createdBook2 = await _bookRepository.CreateAsync(book2);

        var bookUpdateDto = new BookUpdateDto
        {
            Title = "Updated Title",
            Author = "Updated Author",
            ISBN = "1111111111", // Same as book1
            Status = BookStatus.Available
        };

        // Act
        var response = await HttpClient.PutAsync($"/Books/{createdBook2.Id}", CreateJsonContent(bookUpdateDto));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task UpdateBook_WithInvalidData_ShouldReturnBadRequest()
    {
        // Arrange
        var book = BookBuilder.New().Build();
        var createdBook = await _bookRepository.CreateAsync(book);

        var bookUpdateDto = new BookUpdateDto
        {
            Title = "", // Invalid - empty title
            Author = "Updated Author",
            ISBN = "123", // Invalid - too short
            Status = BookStatus.Available
        };

        // Act
        var response = await HttpClient.PutAsync($"/Books/{createdBook.Id}", CreateJsonContent(bookUpdateDto));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task DeleteBook_WithValidId_ShouldReturnNoContent()
    {
        // Arrange
        var book = BookBuilder.New().Build();
        var createdBook = await _bookRepository.CreateAsync(book);

        // Act
        var response = await HttpClient.DeleteAsync($"/Books/{createdBook.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify book is actually deleted
        var getResponse = await HttpClient.GetAsync($"/Books/{createdBook.Id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteBook_WithInvalidId_ShouldReturnNotFound()
    {
        // Act
        var response = await HttpClient.DeleteAsync("/Books/99999");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
using FluentAssertions;
using src.API.DTOs;
using src.Core.Entities;
using src.IntegrationTests.Base;
using src.IntegrationTests.Helpers;
using System.Net;

namespace src.IntegrationTests.API;

public class BooksControllerTests : IntegrationTestBase
{
    private ApiTestHelper _apiHelper = null!;

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        _apiHelper = new ApiTestHelper(this);
    }

    [Fact]
    public async Task GetAllBooks_ShouldReturnAllBooks()
    {
        // Arrange
        await _apiHelper.SetupAuthenticationAsync();
        await _apiHelper.CreateMultipleTestBooksAsync();

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
        await _apiHelper.SetupAuthenticationAsync();
        var book = await _apiHelper.CreateTestBookAsync("Test Book", "Test Author", "1234567890");

        // Act
        var response = await HttpClient.GetAsync($"/Books/{book.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var returnedBook = await DeserializeResponseAsync<BookDto>(response);
        returnedBook.Should().NotBeNull();
        returnedBook.Id.Should().Be(book.Id);
        returnedBook.Title.Should().Be("Test Book");
        returnedBook.Author.Should().Be("Test Author");
        returnedBook.ISBN.Should().Be("1234567890");
    }

    [Fact]
    public async Task GetBook_WithInvalidId_ShouldReturnNotFound()
    {
        // Arrange
        await _apiHelper.SetupAuthenticationAsync();

        // Act
        var response = await HttpClient.GetAsync("/Books/99999");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task SearchBooks_ByTitle_ShouldReturnMatchingBooks()
    {
        // Arrange
        await _apiHelper.SetupAuthenticationAsync();
        await _apiHelper.CreateMultipleTestBooksAsync();

        // Act
        var response = await HttpClient.GetAsync("/Books/search?title=Programming");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var books = await DeserializeResponseAsync<IEnumerable<BookDto>>(response);
        books.Should().NotBeNull();
        var booksList = books!.ToList();
        booksList.Should().HaveCountGreaterThanOrEqualTo(1);
        booksList.Should().OnlyContain(b => b.Title.Contains("Programming"));
    }

    [Fact]
    public async Task SearchBooks_ByAuthor_ShouldReturnMatchingBooks()
    {
        // Arrange
        await _apiHelper.SetupAuthenticationAsync();
        await _apiHelper.CreateTestBookAsync("Book by John Smith", "John Smith", "4444444444");

        // Act
        var response = await HttpClient.GetAsync("/Books/search?author=John");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var books = await DeserializeResponseAsync<IEnumerable<BookDto>>(response);
        books.Should().NotBeNull();
        var booksList = books!.ToList();
        booksList.Should().HaveCountGreaterThanOrEqualTo(1);
        booksList.Should().OnlyContain(b => b.Author.Contains("John"));
    }

    [Fact]
    public async Task SearchBooks_ByStatus_ShouldReturnMatchingBooks()
    {
        // Arrange
        await _apiHelper.SetupAuthenticationAsync();
        await _apiHelper.CreateTestBookAsync("Available Book", "Author", "1111111111", BookStatus.Available);
        await _apiHelper.CreateTestBookAsync("Unavailable Book", "Author", "2222222222", BookStatus.Unavailable);

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
    public async Task CreateBook_WithValidDataAndAuthentication_ShouldReturnCreated()
    {
        // Arrange
        await _apiHelper.SetupAuthenticationAsync();
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
    public async Task CreateBook_WithoutAuthentication_ShouldReturnCreated()
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
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task CreateBook_WithInvalidTitle_ShouldReturnBadRequest(string title)
    {
        // Arrange
        await _apiHelper.SetupAuthenticationAsync();
        var bookCreateDto = new BookCreateDto
        {
            Title = title,
            Author = "Valid Author",
            ISBN = "1234567890",
            Status = BookStatus.Available
        };

        // Act
        var response = await HttpClient.PostAsync("/Books", CreateJsonContent(bookCreateDto));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("123")]
    [InlineData("12345678901234567890")]
    public async Task CreateBook_WithInvalidISBN_ShouldReturnBadRequest(string isbn)
    {
        // Arrange
        await _apiHelper.SetupAuthenticationAsync();
        var bookCreateDto = new BookCreateDto
        {
            Title = "Valid Title",
            Author = "Valid Author",
            ISBN = isbn,
            Status = BookStatus.Available
        };

        // Act
        var response = await HttpClient.PostAsync("/Books", CreateJsonContent(bookCreateDto));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}

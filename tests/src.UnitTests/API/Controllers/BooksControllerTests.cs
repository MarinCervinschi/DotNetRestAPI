using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using src.API.Controllers;
using src.API.DTOs;
using src.Core.Entities;
using src.Core.Interfaces.Services;
using src.UnitTests.Core.Builders;

namespace src.UnitTests.API.Controllers;

public class BooksControllerTests
{
    private readonly Mock<IBookService> _mockBookService;
    private readonly BooksController _controller;

    public BooksControllerTests()
    {
        _mockBookService = new Mock<IBookService>();
        var mockLogger = new Mock<ILogger<BooksController>>();
        _controller = new BooksController(_mockBookService.Object, mockLogger.Object);
    }

    [Fact]
    public async Task GetAllBooks_ReturnsOkWithBookList()
    {
        // Arrange
        var bookDtos = new List<BookDto>
        {
            BookBuilder.New().WithId(1).WithTitle("Book 1").WithIsbn("1111111111").BuildDto(),
            BookBuilder.New().WithId(2).WithTitle("Book 2").WithIsbn("2222222222").BuildDto()
        };

        _mockBookService.Setup(s => s.GetAllBooksAsync())
            .ReturnsAsync(bookDtos);

        // Act
        var result = await _controller.GetAllBooks();

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(200);

        var returnedBooks = okResult.Value.Should().BeAssignableTo<IEnumerable<BookDto>>().Subject;
        var bookList = returnedBooks.ToList();
        bookList.Should().HaveCount(2);
        bookList[0].Id.Should().Be(1);
        bookList[0].Title.Should().Be("Book 1");
        bookList[1].Id.Should().Be(2);
        bookList[1].Title.Should().Be("Book 2");

        _mockBookService.Verify(s => s.GetAllBooksAsync(), Times.Once);
    }

    [Fact]
    public async Task GetAllBooks_WhenNoBooksExist_ReturnsOkWithEmptyList()
    {
        // Arrange
        _mockBookService.Setup(s => s.GetAllBooksAsync())
            .ReturnsAsync(new List<BookDto>());

        // Act
        var result = await _controller.GetAllBooks();

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(200);

        var returnedBooks = okResult.Value.Should().BeAssignableTo<IEnumerable<BookDto>>().Subject;
        returnedBooks.Should().BeEmpty();

        _mockBookService.Verify(s => s.GetAllBooksAsync(), Times.Once);
    }

    [Fact]
    public async Task GetBook_WithValidId_ReturnsOkWithBook()
    {
        // Arrange
        var bookId = 1;
        var bookDto = BookBuilder.New()
            .WithId(bookId)
            .WithTitle("Test Book")
            .WithAuthor("Test Author")
            .WithIsbn("1234567890")
            .BuildDto();

        _mockBookService.Setup(s => s.GetBookByIdAsync(bookId))
            .ReturnsAsync(bookDto);

        // Act
        var result = await _controller.GetBook(bookId);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(200);

        var returnedBook = okResult.Value.Should().BeOfType<BookDto>().Subject;
        returnedBook.Id.Should().Be(bookId);
        returnedBook.Title.Should().Be("Test Book");
        returnedBook.Author.Should().Be("Test Author");
        returnedBook.ISBN.Should().Be("1234567890");

        _mockBookService.Verify(s => s.GetBookByIdAsync(bookId), Times.Once);
    }

    [Fact]
    public async Task GetBook_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        var bookId = 999;
        _mockBookService.Setup(s => s.GetBookByIdAsync(bookId))
            .ReturnsAsync((BookDto?)null);

        // Act
        var result = await _controller.GetBook(bookId);

        // Assert
        result.Result.Should().BeOfType<NotFoundResult>();
        var notFoundResult = result.Result as NotFoundResult;
        notFoundResult!.StatusCode.Should().Be(404);

        _mockBookService.Verify(s => s.GetBookByIdAsync(bookId), Times.Once);
    }

    [Fact]
    public async Task CreateBook_WithValidData_ReturnsCreated()
    {
        // Arrange
        var createDto = BookBuilder.New()
            .WithTitle("New Book")
            .WithAuthor("New Author")
            .WithIsbn("9876543210")
            .BuildCreateDto();

        var createdBook = BookBuilder.New()
            .WithId(1)
            .WithTitle("New Book")
            .WithAuthor("New Author")
            .WithIsbn("9876543210")
            .BuildDto();

        _mockBookService.Setup(s => s.CreateBookAsync(createDto))
            .ReturnsAsync(createdBook);

        // Act
        var result = await _controller.CreateBook(createDto);

        // Assert
        var createdResult = result.Result.Should().BeOfType<CreatedAtActionResult>().Subject;
        createdResult.StatusCode.Should().Be(201);
        createdResult.ActionName.Should().Be(nameof(BooksController.GetBook));
        createdResult.RouteValues!["id"].Should().Be(1);

        var returnedBook = createdResult.Value.Should().BeOfType<BookDto>().Subject;
        returnedBook.Id.Should().Be(1);
        returnedBook.Title.Should().Be("New Book");
        returnedBook.Author.Should().Be("New Author");
        returnedBook.ISBN.Should().Be("9876543210");

        _mockBookService.Verify(s => s.CreateBookAsync(createDto), Times.Once);
    }

    [Fact]
    public async Task CreateBook_WithInvalidModelState_ReturnsBadRequest()
    {
        // Arrange
        var createDto = BookBuilder.New()
            .WithTitle("")
            .WithAuthor("")
            .WithIsbn("invalid")
            .BuildCreateDto();

        _controller.ModelState.AddModelError("Title", "Title is required");
        _controller.ModelState.AddModelError("Author", "Author is required");
        _controller.ModelState.AddModelError("ISBN", "Invalid ISBN format");

        // Act
        var result = await _controller.CreateBook(createDto);

        // Assert
        var badRequestResult = result.Result.Should().BeOfType<BadRequestObjectResult>().Subject;
        badRequestResult.StatusCode.Should().Be(400);
        badRequestResult.Value.Should().BeOfType<SerializableError>();

        _mockBookService.Verify(s => s.CreateBookAsync(It.IsAny<BookCreateDto>()), Times.Never);
    }

    [Fact]
    public async Task CreateBook_WithDuplicateIsbn_ReturnsConflict()
    {
        // Arrange
        var createDto = BookBuilder.New()
            .WithTitle("New Book")
            .WithAuthor("New Author")
            .WithIsbn("1234567890")
            .BuildCreateDto();

        _mockBookService.Setup(s => s.CreateBookAsync(createDto))
            .ThrowsAsync(new InvalidOperationException("A book with ISBN '1234567890' already exists."));

        // Act
        var result = await _controller.CreateBook(createDto);

        // Assert
        var conflictResult = result.Result.Should().BeOfType<ConflictObjectResult>().Subject;
        conflictResult.StatusCode.Should().Be(409);


        _mockBookService.Verify(s => s.CreateBookAsync(createDto), Times.Once);
    }

    [Fact]
    public async Task CreateBook_WhenServiceThrowsException_ReturnsInternalServerError()
    {
        // Arrange
        var createDto = BookBuilder.New()
            .WithTitle("New Book")
            .WithAuthor("New Author")
            .WithIsbn("9876543210")
            .BuildCreateDto();

        _mockBookService.Setup(s => s.CreateBookAsync(createDto))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _controller.CreateBook(createDto);

        // Assert
        var statusCodeResult = result.Result.Should().BeOfType<ObjectResult>().Subject;
        statusCodeResult.StatusCode.Should().Be(500);
        statusCodeResult.Value.Should().Be("Internal server error");

        _mockBookService.Verify(s => s.CreateBookAsync(createDto), Times.Once);
    }

    [Fact]
    public async Task UpdateBook_WithValidData_ReturnsOkWithUpdatedBook()
    {
        // Arrange
        var bookId = 1;
        var updateDto = BookBuilder.New()
            .WithTitle("Updated Book")
            .WithAuthor("Updated Author")
            .WithIsbn("9999999999")
            .WithUnavailableStatus()
            .BuildUpdateDto();

        var updatedBook = BookBuilder.New()
            .WithId(bookId)
            .WithTitle("Updated Book")
            .WithAuthor("Updated Author")
            .WithIsbn("9999999999")
            .WithUnavailableStatus()
            .BuildDto();

        _mockBookService.Setup(s => s.UpdateBookAsync(bookId, updateDto))
            .ReturnsAsync(updatedBook);

        // Act
        var result = await _controller.UpdateBook(bookId, updateDto);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(200);

        var returnedBook = okResult.Value.Should().BeOfType<BookDto>().Subject;
        returnedBook.Id.Should().Be(bookId);
        returnedBook.Title.Should().Be("Updated Book");
        returnedBook.Author.Should().Be("Updated Author");
        returnedBook.ISBN.Should().Be("9999999999");
        returnedBook.Status.Should().Be(BookStatus.Unavailable);

        _mockBookService.Verify(s => s.UpdateBookAsync(bookId, updateDto), Times.Once);
    }

    [Fact]
    public async Task UpdateBook_WithInvalidModelState_ReturnsBadRequest()
    {
        // Arrange
        var bookId = 1;
        var updateDto = BookBuilder.New()
            .WithTitle("")
            .WithAuthor("")
            .WithIsbn("invalid")
            .BuildUpdateDto();

        _controller.ModelState.AddModelError("Title", "Title is required");
        _controller.ModelState.AddModelError("Author", "Author is required");
        _controller.ModelState.AddModelError("ISBN", "Invalid ISBN format");

        // Act
        var result = await _controller.UpdateBook(bookId, updateDto);

        // Assert
        var badRequestResult = result.Result.Should().BeOfType<BadRequestObjectResult>().Subject;
        badRequestResult.StatusCode.Should().Be(400);
        badRequestResult.Value.Should().BeOfType<SerializableError>();

        _mockBookService.Verify(s => s.UpdateBookAsync(It.IsAny<int>(), It.IsAny<BookUpdateDto>()), Times.Never);
    }

    [Fact]
    public async Task UpdateBook_WhenBookNotExists_ReturnsNotFound()
    {
        // Arrange
        var bookId = 999;
        var updateDto = BookBuilder.New()
            .WithTitle("Updated Book")
            .WithAuthor("Updated Author")
            .WithIsbn("9999999999")
            .BuildUpdateDto();

        _mockBookService.Setup(s => s.UpdateBookAsync(bookId, updateDto))
            .ThrowsAsync(new KeyNotFoundException($"Book with ID {bookId} not found."));

        // Act
        var result = await _controller.UpdateBook(bookId, updateDto);

        // Assert
        result.Result.Should().BeOfType<NotFoundResult>();
        var notFoundResult = result.Result as NotFoundResult;
        notFoundResult!.StatusCode.Should().Be(404);

        _mockBookService.Verify(s => s.UpdateBookAsync(bookId, updateDto), Times.Once);
    }

    [Fact]
    public async Task UpdateBook_WithDuplicateIsbn_ReturnsConflict()
    {
        // Arrange
        var bookId = 1;
        var updateDto = BookBuilder.New()
            .WithTitle("Updated Book")
            .WithAuthor("Updated Author")
            .WithIsbn("1234567890")
            .BuildUpdateDto();

        _mockBookService.Setup(s => s.UpdateBookAsync(bookId, updateDto))
            .ThrowsAsync(new InvalidOperationException("A book with ISBN '1234567890' already exists."));

        // Act
        var result = await _controller.UpdateBook(bookId, updateDto);

        // Assert
        var conflictResult = result.Result.Should().BeOfType<ConflictObjectResult>().Subject;
        conflictResult.StatusCode.Should().Be(409);

        _mockBookService.Verify(s => s.UpdateBookAsync(bookId, updateDto), Times.Once);
    }

    [Fact]
    public async Task DeleteBook_WhenBookExists_ReturnsNoContent()
    {
        // Arrange
        var bookId = 1;
        _mockBookService.Setup(s => s.DeleteBookAsync(bookId))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.DeleteBook(bookId);

        // Assert
        result.Should().BeOfType<NoContentResult>();
        var noContentResult = result as NoContentResult;
        noContentResult!.StatusCode.Should().Be(204);

        _mockBookService.Verify(s => s.DeleteBookAsync(bookId), Times.Once);
    }

    [Fact]
    public async Task DeleteBook_WhenBookNotExists_ReturnsNotFound()
    {
        // Arrange
        var bookId = 999;
        _mockBookService.Setup(s => s.DeleteBookAsync(bookId))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.DeleteBook(bookId);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
        var notFoundResult = result as NotFoundResult;
        notFoundResult!.StatusCode.Should().Be(404);

        _mockBookService.Verify(s => s.DeleteBookAsync(bookId), Times.Once);
    }

    #region SearchBooks Tests

    [Fact]
    public async Task SearchBooks_WithTitleOnly_ReturnsOkWithMatchingBooks()
    {
        // Arrange
        var searchTitle = "Great";
        var bookDtos = new List<BookDto>
        {
            BookBuilder.New().WithId(1).WithTitle("The Great Gatsby").WithAuthor("F. Scott Fitzgerald").BuildDto(),
            BookBuilder.New().WithId(2).WithTitle("Great Expectations").WithAuthor("Charles Dickens").BuildDto()
        };

        _mockBookService.Setup(s => s.SearchBooksAsync(searchTitle, null, null))
            .ReturnsAsync(bookDtos);

        // Act
        var result = await _controller.SearchBooks(title: searchTitle);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(200);

        var returnedBooks = okResult.Value.Should().BeAssignableTo<IEnumerable<BookDto>>().Subject;
        var bookList = returnedBooks.ToList();
        bookList.Should().HaveCount(2);
        bookList.Should().Contain(b => b.Title == "The Great Gatsby");
        bookList.Should().Contain(b => b.Title == "Great Expectations");

        _mockBookService.Verify(s => s.SearchBooksAsync(searchTitle, null, null), Times.Once);
    }

    [Fact]
    public async Task SearchBooks_WithAuthorOnly_ReturnsOkWithMatchingBooks()
    {
        // Arrange
        var searchAuthor = "Dickens";
        var bookDtos = new List<BookDto>
        {
            BookBuilder.New().WithId(1).WithTitle("Great Expectations").WithAuthor("Charles Dickens").BuildDto(),
            BookBuilder.New().WithId(2).WithTitle("A Tale of Two Cities").WithAuthor("Charles Dickens").BuildDto()
        };

        _mockBookService.Setup(s => s.SearchBooksAsync(null, searchAuthor, null))
            .ReturnsAsync(bookDtos);

        // Act
        var result = await _controller.SearchBooks(author: searchAuthor);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(200);

        var returnedBooks = okResult.Value.Should().BeAssignableTo<IEnumerable<BookDto>>().Subject;
        var bookList = returnedBooks.ToList();
        bookList.Should().HaveCount(2);
        bookList.Should().Contain(b => b.Author == "Charles Dickens");

        _mockBookService.Verify(s => s.SearchBooksAsync(null, searchAuthor, null), Times.Once);
    }

    [Fact]
    public async Task SearchBooks_WithStatusOnly_ReturnsOkWithMatchingBooks()
    {
        // Arrange
        var searchStatus = BookStatus.Unavailable;
        var bookDtos = new List<BookDto>
        {
            BookBuilder.New().WithId(1).WithTitle("Unavailable Book 1").WithUnavailableStatus().BuildDto(),
            BookBuilder.New().WithId(2).WithTitle("Unavailable Book 2").WithUnavailableStatus().BuildDto()
        };

        _mockBookService.Setup(s => s.SearchBooksAsync(null, null, searchStatus))
            .ReturnsAsync(bookDtos);

        // Act
        var result = await _controller.SearchBooks(status: searchStatus);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(200);

        var returnedBooks = okResult.Value.Should().BeAssignableTo<IEnumerable<BookDto>>().Subject;
        var bookList = returnedBooks.ToList();
        bookList.Should().HaveCount(2);
        bookList.Should().AllSatisfy(b => b.Status.Should().Be(BookStatus.Unavailable));

        _mockBookService.Verify(s => s.SearchBooksAsync(null, null, searchStatus), Times.Once);
    }

    [Fact]
    public async Task SearchBooks_WithMultipleCriteria_ReturnsOkWithMatchingBooks()
    {
        // Arrange
        var searchTitle = "Great";
        var searchAuthor = "Fitzgerald";
        var searchStatus = BookStatus.Available;

        var bookDto = BookBuilder.New()
            .WithId(1)
            .WithTitle("The Great Gatsby")
            .WithAuthor("F. Scott Fitzgerald")
            .WithAvailableStatus()
            .BuildDto();

        _mockBookService.Setup(s => s.SearchBooksAsync(searchTitle, searchAuthor, searchStatus))
            .ReturnsAsync(new List<BookDto> { bookDto });

        // Act
        var result = await _controller.SearchBooks(title: searchTitle, author: searchAuthor, status: searchStatus);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(200);

        var returnedBooks = okResult.Value.Should().BeAssignableTo<IEnumerable<BookDto>>().Subject;
        var bookList = returnedBooks.ToList();
        bookList.Should().HaveCount(1);
        bookList[0].Title.Should().Be("The Great Gatsby");
        bookList[0].Author.Should().Be("F. Scott Fitzgerald");
        bookList[0].Status.Should().Be(BookStatus.Available);

        _mockBookService.Verify(s => s.SearchBooksAsync(searchTitle, searchAuthor, searchStatus), Times.Once);
    }

    [Fact]
    public async Task SearchBooks_WithNoParameters_ReturnsOkWithAllBooks()
    {
        // Arrange
        var bookDtos = new List<BookDto>
        {
            BookBuilder.New().WithId(1).WithTitle("Book 1").BuildDto(),
            BookBuilder.New().WithId(2).WithTitle("Book 2").BuildDto(),
            BookBuilder.New().WithId(3).WithTitle("Book 3").BuildDto()
        };

        _mockBookService.Setup(s => s.SearchBooksAsync(null, null, null))
            .ReturnsAsync(bookDtos);

        // Act
        var result = await _controller.SearchBooks();

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(200);

        var returnedBooks = okResult.Value.Should().BeAssignableTo<IEnumerable<BookDto>>().Subject;
        var bookList = returnedBooks.ToList();
        bookList.Should().HaveCount(3);

        _mockBookService.Verify(s => s.SearchBooksAsync(null, null, null), Times.Once);
    }

    [Fact]
    public async Task SearchBooks_WithNoMatches_ReturnsOkWithEmptyList()
    {
        // Arrange
        var searchTitle = "NonExistentBook";

        _mockBookService.Setup(s => s.SearchBooksAsync(searchTitle, null, null))
            .ReturnsAsync(new List<BookDto>());

        // Act
        var result = await _controller.SearchBooks(title: searchTitle);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(200);

        var returnedBooks = okResult.Value.Should().BeAssignableTo<IEnumerable<BookDto>>().Subject;
        returnedBooks.Should().BeEmpty();

        _mockBookService.Verify(s => s.SearchBooksAsync(searchTitle, null, null), Times.Once);
    }

    [Fact]
    public async Task SearchBooks_WithEmptyStringParameters_ReturnsOkWithAllBooks()
    {
        // Arrange
        var emptyTitle = "";
        var emptyAuthor = "   ";
        var bookDtos = new List<BookDto>
        {
            BookBuilder.New().WithId(1).WithTitle("Book 1").BuildDto(),
            BookBuilder.New().WithId(2).WithTitle("Book 2").BuildDto()
        };

        _mockBookService.Setup(s => s.SearchBooksAsync(emptyTitle, emptyAuthor, null))
            .ReturnsAsync(bookDtos);

        // Act
        var result = await _controller.SearchBooks(title: emptyTitle, author: emptyAuthor);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(200);

        var returnedBooks = okResult.Value.Should().BeAssignableTo<IEnumerable<BookDto>>().Subject;
        var bookList = returnedBooks.ToList();
        bookList.Should().HaveCount(2);

        _mockBookService.Verify(s => s.SearchBooksAsync(emptyTitle, emptyAuthor, null), Times.Once);
    }

    [Fact]
    public async Task SearchBooks_WithValidStatusEnum_ReturnsOkWithMatchingBooks()
    {
        // Arrange
        var searchStatus = BookStatus.Available;
        var bookDtos = new List<BookDto>
        {
            BookBuilder.New().WithId(1).WithTitle("Available Book 1").WithAvailableStatus().BuildDto(),
            BookBuilder.New().WithId(2).WithTitle("Available Book 2").WithAvailableStatus().BuildDto()
        };

        _mockBookService.Setup(s => s.SearchBooksAsync(null, null, searchStatus))
            .ReturnsAsync(bookDtos);

        // Act
        var result = await _controller.SearchBooks(status: searchStatus);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(200);

        var returnedBooks = okResult.Value.Should().BeAssignableTo<IEnumerable<BookDto>>().Subject;
        var bookList = returnedBooks.ToList();
        bookList.Should().HaveCount(2);
        bookList.Should().AllSatisfy(b => b.Status.Should().Be(BookStatus.Available));

        _mockBookService.Verify(s => s.SearchBooksAsync(null, null, searchStatus), Times.Once);
    }

    [Fact]
    public async Task SearchBooks_WithCaseInsensitiveSearch_ReturnsOkWithMatchingBooks()
    {
        // Arrange
        var searchTitle = "GATSBY";
        var searchAuthor = "FITZGERALD";
        var bookDto = BookBuilder.New()
            .WithId(1)
            .WithTitle("The Great Gatsby")
            .WithAuthor("F. Scott Fitzgerald")
            .BuildDto();

        _mockBookService.Setup(s => s.SearchBooksAsync(searchTitle, searchAuthor, null))
            .ReturnsAsync(new List<BookDto> { bookDto });

        // Act
        var result = await _controller.SearchBooks(title: searchTitle, author: searchAuthor);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(200);

        var returnedBooks = okResult.Value.Should().BeAssignableTo<IEnumerable<BookDto>>().Subject;
        var bookList = returnedBooks.ToList();
        bookList.Should().HaveCount(1);
        bookList[0].Title.Should().Be("The Great Gatsby");

        _mockBookService.Verify(s => s.SearchBooksAsync(searchTitle, searchAuthor, null), Times.Once);
    }

    #endregion
}
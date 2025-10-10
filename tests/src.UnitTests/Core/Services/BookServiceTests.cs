using FluentAssertions;
using Moq;
using src.Core.Entities;
using src.Core.Interfaces;
using src.Core.Services;
using src.UnitTests.Core.Builders;

namespace src.UnitTests.Core.Services;

public class BookServiceTests
{
    private readonly Mock<IRepository<Book>> _mockRepository;
    private readonly BookService _bookService;

    public BookServiceTests()
    {
        _mockRepository = new Mock<IRepository<Book>>();
        _bookService = new BookService(_mockRepository.Object);
    }

    [Fact]
    public async Task GetBookByIdAsync_WithValidId_ReturnsBookDto()
    {
        // Arrange
        var bookId = 1;
        var book = BookBuilder.New()
            .WithId(bookId)
            .WithTitle("Test Book")
            .WithAuthor("Test Author")
            .WithIsbn("1234567890")
            .Build();

        _mockRepository.Setup(r => r.GetByIdAsync(bookId))
            .ReturnsAsync(book);

        // Act
        var result = await _bookService.GetBookByIdAsync(bookId);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(bookId);
        result.Title.Should().Be("Test Book");
        result.Author.Should().Be("Test Author");
        result.ISBN.Should().Be("1234567890");
        result.Status.Should().Be(BookStatus.Available);

        _mockRepository.Verify(r => r.GetByIdAsync(bookId), Times.Once);
    }

    [Fact]
    public async Task GetBookByIdAsync_WithInvalidId_ReturnsNull()
    {
        // Arrange
        var bookId = 999;
        _mockRepository.Setup(r => r.GetByIdAsync(bookId))
            .ReturnsAsync((Book?)null);

        // Act
        var result = await _bookService.GetBookByIdAsync(bookId);

        // Assert
        result.Should().BeNull();
        _mockRepository.Verify(r => r.GetByIdAsync(bookId), Times.Once);
    }

    [Fact]
    public async Task GetAllBooksAsync_ReturnsAllBooks()
    {
        // Arrange
        var books = new List<Book>
        {
            BookBuilder.New().WithId(1).WithTitle("Book 1").WithIsbn("1111111111").Build(),
            BookBuilder.New().WithId(2).WithTitle("Book 2").WithIsbn("2222222222").Build(),
            BookBuilder.New().WithId(3).WithTitle("Book 3").WithIsbn("3333333333").Build()
        };

        _mockRepository.Setup(r => r.GetAllAsync())
            .ReturnsAsync(books);

        // Act
        var result = await _bookService.GetAllBooksAsync();

        // Assert
        var bookDtos = result.ToList();
        bookDtos.Should().HaveCount(3);
        bookDtos[0].Title.Should().Be("Book 1");
        bookDtos[1].Title.Should().Be("Book 2");
        bookDtos[2].Title.Should().Be("Book 3");

        _mockRepository.Verify(r => r.GetAllAsync(), Times.Once);
    }

    [Fact]
    public async Task GetAllBooksAsync_WhenNoBooksExist_ReturnsEmptyList()
    {
        // Arrange
        _mockRepository.Setup(r => r.GetAllAsync())
            .ReturnsAsync(new List<Book>());

        // Act
        var result = await _bookService.GetAllBooksAsync();

        // Assert
        result.Should().BeEmpty();
        _mockRepository.Verify(r => r.GetAllAsync(), Times.Once);
    }

    [Fact]
    public async Task CreateBookAsync_WithValidData_ReturnsBookDto()
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
            .Build();

        _mockRepository.Setup(r => r.GetAllAsync())
            .ReturnsAsync(new List<Book>());

        _mockRepository.Setup(r => r.CreateAsync(It.IsAny<Book>()))
            .ReturnsAsync(createdBook);

        // Act
        var result = await _bookService.CreateBookAsync(createDto);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(1);
        result.Title.Should().Be("New Book");
        result.Author.Should().Be("New Author");
        result.ISBN.Should().Be("9876543210");
        result.Status.Should().Be(BookStatus.Available);

        _mockRepository.Verify(r => r.GetAllAsync(), Times.Once);
        _mockRepository.Verify(r => r.CreateAsync(It.Is<Book>(b =>
            b.Title == "New Book" &&
            b.Author == "New Author" &&
            b.ISBN == "9876543210")), Times.Once);
    }

    [Fact]
    public async Task CreateBookAsync_WithExistingIsbn_ThrowsInvalidOperationException()
    {
        // Arrange
        var createDto = BookBuilder.New()
            .WithTitle("New Book")
            .WithAuthor("New Author")
            .WithIsbn("1234567890")
            .BuildCreateDto();

        var existingBooks = new List<Book>
        {
            BookBuilder.New().WithIsbn("1234567890").Build()
        };

        _mockRepository.Setup(r => r.GetAllAsync())
            .ReturnsAsync(existingBooks);

        // Act & Assert
        var exception =
            await Assert.ThrowsAsync<InvalidOperationException>(() => _bookService.CreateBookAsync(createDto));

        exception.Message.Should().Contain("A book with ISBN '1234567890' already exists.");
        _mockRepository.Verify(r => r.GetAllAsync(), Times.Once);
        _mockRepository.Verify(r => r.CreateAsync(It.IsAny<Book>()), Times.Never);
    }

    [Fact]
    public async Task UpdateBookAsync_WithValidData_ReturnsUpdatedBookDto()
    {
        // Arrange
        var bookId = 1;
        var updateDto = BookBuilder.New()
            .WithTitle("Updated Book")
            .WithAuthor("Updated Author")
            .WithIsbn("9999999999")
            .WithUnavailableStatus()
            .BuildUpdateDto();

        var existingBook = BookBuilder.New()
            .WithId(bookId)
            .WithTitle("Original Book")
            .WithAuthor("Original Author")
            .WithIsbn("1111111111")
            .Build();

        var updatedBook = BookBuilder.New()
            .WithId(bookId)
            .WithTitle("Updated Book")
            .WithAuthor("Updated Author")
            .WithIsbn("9999999999")
            .WithUnavailableStatus()
            .Build();

        _mockRepository.Setup(r => r.GetByIdAsync(bookId))
            .ReturnsAsync(existingBook);

        _mockRepository.Setup(r => r.GetAllAsync())
            .ReturnsAsync(new List<Book> { existingBook });

        _mockRepository.Setup(r => r.UpdateAsync(It.IsAny<Book>()))
            .ReturnsAsync(updatedBook);

        // Act
        var result = await _bookService.UpdateBookAsync(bookId, updateDto);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(bookId);
        result.Title.Should().Be("Updated Book");
        result.Author.Should().Be("Updated Author");
        result.ISBN.Should().Be("9999999999");
        result.Status.Should().Be(BookStatus.Unavailable);

        _mockRepository.Verify(r => r.GetByIdAsync(bookId), Times.Once);
        _mockRepository.Verify(r => r.GetAllAsync(), Times.Once);
        _mockRepository.Verify(r => r.UpdateAsync(It.IsAny<Book>()), Times.Once);
    }

    [Fact]
    public async Task UpdateBookAsync_WithNonExistentId_ThrowsKeyNotFoundException()
    {
        // Arrange
        var bookId = 999;
        var updateDto = BookBuilder.New().BuildUpdateDto();

        _mockRepository.Setup(r => r.GetByIdAsync(bookId))
            .ReturnsAsync((Book?)null);

        // Act & Assert
        var exception =
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _bookService.UpdateBookAsync(bookId, updateDto));

        exception.Message.Should().Contain($"Book with ID {bookId} not found.");
        _mockRepository.Verify(r => r.GetByIdAsync(bookId), Times.Once);
        _mockRepository.Verify(r => r.UpdateAsync(It.IsAny<Book>()), Times.Never);
    }

    [Fact]
    public async Task UpdateBookAsync_WithDuplicateIsbn_ThrowsInvalidOperationException()
    {
        // Arrange
        var bookId = 1;
        var duplicateIsbn = "9876543210";
        var updateDto = BookBuilder.New()
            .WithIsbn(duplicateIsbn)
            .BuildUpdateDto();

        var existingBook = BookBuilder.New()
            .WithId(bookId)
            .WithIsbn("1111111111")
            .Build();

        var otherBookWithSameIsbn = BookBuilder.New()
            .WithId(2)
            .WithIsbn(duplicateIsbn)
            .Build();

        var allBooks = new List<Book> { existingBook, otherBookWithSameIsbn };

        _mockRepository.Setup(r => r.GetByIdAsync(bookId))
            .ReturnsAsync(existingBook);

        _mockRepository.Setup(r => r.GetAllAsync())
            .ReturnsAsync(allBooks);

        // Act & Assert
        var exception =
            await Assert.ThrowsAsync<InvalidOperationException>(() => _bookService.UpdateBookAsync(bookId, updateDto));

        exception.Message.Should().Contain($"A book with ISBN '{duplicateIsbn}' already exists.");
        _mockRepository.Verify(r => r.GetByIdAsync(bookId), Times.Once);
        _mockRepository.Verify(r => r.GetAllAsync(), Times.Once);
        _mockRepository.Verify(r => r.UpdateAsync(It.IsAny<Book>()), Times.Never);
    }

    [Fact]
    public async Task DeleteBookAsync_WhenBookExists_ReturnsTrue()
    {
        // Arrange
        var bookId = 1;
        _mockRepository.Setup(r => r.DeleteAsync(bookId))
            .ReturnsAsync(true);

        // Act
        var result = await _bookService.DeleteBookAsync(bookId);

        // Assert
        result.Should().BeTrue();
        _mockRepository.Verify(r => r.DeleteAsync(bookId), Times.Once);
    }

    [Fact]
    public async Task DeleteBookAsync_WhenBookNotExists_ReturnsFalse()
    {
        // Arrange
        var bookId = 999;
        _mockRepository.Setup(r => r.DeleteAsync(bookId))
            .ReturnsAsync(false);

        // Act
        var result = await _bookService.DeleteBookAsync(bookId);

        // Assert
        result.Should().BeFalse();
        _mockRepository.Verify(r => r.DeleteAsync(bookId), Times.Once);
    }

    [Fact]
    public async Task BookExistsAsync_WhenBookExists_ReturnsTrue()
    {
        // Arrange
        var bookId = 1;
        _mockRepository.Setup(r => r.ExistsAsync(bookId))
            .ReturnsAsync(true);

        // Act
        var result = await _bookService.BookExistsAsync(bookId);

        // Assert
        result.Should().BeTrue();
        _mockRepository.Verify(r => r.ExistsAsync(bookId), Times.Once);
    }

    [Fact]
    public async Task BookExistsAsync_WhenBookNotExists_ReturnsFalse()
    {
        // Arrange
        var bookId = 999;
        _mockRepository.Setup(r => r.ExistsAsync(bookId))
            .ReturnsAsync(false);

        // Act
        var result = await _bookService.BookExistsAsync(bookId);

        // Assert
        result.Should().BeFalse();
        _mockRepository.Verify(r => r.ExistsAsync(bookId), Times.Once);
    }

    [Fact]
    public async Task IsbnExistsAsync_WhenIsbnExists_ReturnsTrue()
    {
        // Arrange
        var isbn = "1234567890";
        var books = new List<Book>
        {
            BookBuilder.New().WithIsbn(isbn).Build(),
            BookBuilder.New().WithIsbn("9876543210").Build()
        };

        _mockRepository.Setup(r => r.GetAllAsync())
            .ReturnsAsync(books);

        // Act
        var result = await _bookService.IsbnExistsAsync(isbn);

        // Assert
        result.Should().BeTrue();
        _mockRepository.Verify(r => r.GetAllAsync(), Times.Once);
    }

    [Fact]
    public async Task IsbnExistsAsync_WhenIsbnNotExists_ReturnsFalse()
    {
        // Arrange
        var isbn = "9999999999";
        var books = new List<Book>
        {
            BookBuilder.New().WithIsbn("1234567890").Build(),
            BookBuilder.New().WithIsbn("9876543210").Build()
        };

        _mockRepository.Setup(r => r.GetAllAsync())
            .ReturnsAsync(books);

        // Act
        var result = await _bookService.IsbnExistsAsync(isbn);

        // Assert
        result.Should().BeFalse();
        _mockRepository.Verify(r => r.GetAllAsync(), Times.Once);
    }

    [Fact]
    public async Task IsbnExistsAsync_WithExcludeId_WhenIsbnExistsInOtherBook_ReturnsTrue()
    {
        // Arrange
        var isbn = "1234567890";
        var excludeBookId = 1;
        var books = new List<Book>
        {
            BookBuilder.New().WithId(1).WithIsbn(isbn).Build(),
            BookBuilder.New().WithId(2).WithIsbn(isbn).Build() // Same ISBN in different book
        };

        _mockRepository.Setup(r => r.GetAllAsync())
            .ReturnsAsync(books);

        // Act
        var result = await _bookService.IsbnExistsAsync(isbn, excludeBookId);

        // Assert
        result.Should().BeTrue();
        _mockRepository.Verify(r => r.GetAllAsync(), Times.Once);
    }

    [Fact]
    public async Task IsbnExistsAsync_WithExcludeId_WhenIsbnExistsOnlyInExcludedBook_ReturnsFalse()
    {
        // Arrange
        var isbn = "1234567890";
        var excludeBookId = 1;
        var books = new List<Book>
        {
            BookBuilder.New().WithId(1).WithIsbn(isbn).Build(), // This book should be excluded
            BookBuilder.New().WithId(2).WithIsbn("9876543210").Build()
        };

        _mockRepository.Setup(r => r.GetAllAsync())
            .ReturnsAsync(books);

        // Act
        var result = await _bookService.IsbnExistsAsync(isbn, excludeBookId);

        // Assert
        result.Should().BeFalse();
        _mockRepository.Verify(r => r.GetAllAsync(), Times.Once);
    }

    [Fact]
    public async Task SearchBooksAsync_WithTitleOnly_ReturnsMatchingBooks()
    {
        // Arrange
        var searchTitle = "Great";
        var books = new List<Book>
        {
            BookBuilder.New().WithId(1).WithTitle("The Great Gatsby").WithAuthor("F. Scott Fitzgerald").Build(),
            BookBuilder.New().WithId(2).WithTitle("Great Expectations").WithAuthor("Charles Dickens").Build(),
            BookBuilder.New().WithId(3).WithTitle("To Kill a Mockingbird").WithAuthor("Harper Lee").Build()
        };

        _mockRepository.Setup(r => r.GetAllAsync())
            .ReturnsAsync(books);

        // Act
        var result = await _bookService.SearchBooksAsync(title: searchTitle);

        // Assert
        var resultList = result.ToList();
        resultList.Should().HaveCount(2);
        resultList.Should().Contain(b => b.Title == "The Great Gatsby");
        resultList.Should().Contain(b => b.Title == "Great Expectations");
        resultList.Should().NotContain(b => b.Title == "To Kill a Mockingbird");

        _mockRepository.Verify(r => r.GetAllAsync(), Times.Once);
    }

    [Fact]
    public async Task SearchBooksAsync_WithAuthorOnly_ReturnsMatchingBooks()
    {
        // Arrange
        var searchAuthor = "Dickens";
        var books = new List<Book>
        {
            BookBuilder.New().WithId(1).WithTitle("Great Expectations").WithAuthor("Charles Dickens").Build(),
            BookBuilder.New().WithId(2).WithTitle("A Tale of Two Cities").WithAuthor("Charles Dickens").Build(),
            BookBuilder.New().WithId(3).WithTitle("The Great Gatsby").WithAuthor("F. Scott Fitzgerald").Build()
        };

        _mockRepository.Setup(r => r.GetAllAsync())
            .ReturnsAsync(books);

        // Act
        var result = await _bookService.SearchBooksAsync(author: searchAuthor);

        // Assert
        var resultList = result.ToList();
        resultList.Should().HaveCount(2);
        resultList.Should().Contain(b => b.Title == "Great Expectations");
        resultList.Should().Contain(b => b.Title == "A Tale of Two Cities");
        resultList.Should().NotContain(b => b.Author == "F. Scott Fitzgerald");

        _mockRepository.Verify(r => r.GetAllAsync(), Times.Once);
    }

    [Fact]
    public async Task SearchBooksAsync_WithStatusOnly_ReturnsMatchingBooks()
    {
        // Arrange
        var searchStatus = BookStatus.Unavailable;
        var books = new List<Book>
        {
            BookBuilder.New().WithId(1).WithTitle("Available Book").WithAvailableStatus().Build(),
            BookBuilder.New().WithId(2).WithTitle("Unavailable Book 1").WithUnavailableStatus().Build(),
            BookBuilder.New().WithId(3).WithTitle("Unavailable Book 2").WithUnavailableStatus().Build()
        };

        _mockRepository.Setup(r => r.GetAllAsync())
            .ReturnsAsync(books);

        // Act
        var result = await _bookService.SearchBooksAsync(status: searchStatus);

        // Assert
        var resultList = result.ToList();
        resultList.Should().HaveCount(2);
        resultList.Should().Contain(b => b.Title == "Unavailable Book 1");
        resultList.Should().Contain(b => b.Title == "Unavailable Book 2");
        resultList.Should().NotContain(b => b.Title == "Available Book");

        _mockRepository.Verify(r => r.GetAllAsync(), Times.Once);
    }

    [Fact]
    public async Task SearchBooksAsync_WithMultipleCriteria_ReturnsMatchingBooks()
    {
        // Arrange
        var searchTitle = "Great";
        var searchAuthor = "Fitzgerald";
        var searchStatus = BookStatus.Available;
        
        var books = new List<Book>
        {
            BookBuilder.New().WithId(1).WithTitle("The Great Gatsby").WithAuthor("F. Scott Fitzgerald").WithAvailableStatus().Build(),
            BookBuilder.New().WithId(2).WithTitle("Great Expectations").WithAuthor("Charles Dickens").WithAvailableStatus().Build(),
            BookBuilder.New().WithId(3).WithTitle("The Great Gatsby").WithAuthor("F. Scott Fitzgerald").WithUnavailableStatus().Build(),
            BookBuilder.New().WithId(4).WithTitle("Another Book").WithAuthor("F. Scott Fitzgerald").WithAvailableStatus().Build()
        };

        _mockRepository.Setup(r => r.GetAllAsync())
            .ReturnsAsync(books);

        // Act
        var result = await _bookService.SearchBooksAsync(title: searchTitle, author: searchAuthor, status: searchStatus);

        // Assert
        var resultList = result.ToList();
        resultList.Should().HaveCount(1);
        resultList[0].Title.Should().Be("The Great Gatsby");
        resultList[0].Author.Should().Be("F. Scott Fitzgerald");
        resultList[0].Status.Should().Be(BookStatus.Available);

        _mockRepository.Verify(r => r.GetAllAsync(), Times.Once);
    }

    [Fact]
    public async Task SearchBooksAsync_WithCaseInsensitiveTitle_ReturnsMatchingBooks()
    {
        // Arrange
        var searchTitle = "GREAT";
        var books = new List<Book>
        {
            BookBuilder.New().WithId(1).WithTitle("The Great Gatsby").Build(),
            BookBuilder.New().WithId(2).WithTitle("great expectations").Build(),
            BookBuilder.New().WithId(3).WithTitle("To Kill a Mockingbird").Build()
        };

        _mockRepository.Setup(r => r.GetAllAsync())
            .ReturnsAsync(books);

        // Act
        var result = await _bookService.SearchBooksAsync(title: searchTitle);

        // Assert
        var resultList = result.ToList();
        resultList.Should().HaveCount(2);
        resultList.Should().Contain(b => b.Title == "The Great Gatsby");
        resultList.Should().Contain(b => b.Title == "great expectations");

        _mockRepository.Verify(r => r.GetAllAsync(), Times.Once);
    }

    [Fact]
    public async Task SearchBooksAsync_WithCaseInsensitiveAuthor_ReturnsMatchingBooks()
    {
        // Arrange
        var searchAuthor = "FITZGERALD";
        var books = new List<Book>
        {
            BookBuilder.New().WithId(1).WithAuthor("F. Scott Fitzgerald").Build(),
            BookBuilder.New().WithId(2).WithAuthor("fitzgerald").Build(),
            BookBuilder.New().WithId(3).WithAuthor("Charles Dickens").Build()
        };

        _mockRepository.Setup(r => r.GetAllAsync())
            .ReturnsAsync(books);

        // Act
        var result = await _bookService.SearchBooksAsync(author: searchAuthor);

        // Assert
        var resultList = result.ToList();
        resultList.Should().HaveCount(2);
        resultList.Should().Contain(b => b.Author == "F. Scott Fitzgerald");
        resultList.Should().Contain(b => b.Author == "fitzgerald");

        _mockRepository.Verify(r => r.GetAllAsync(), Times.Once);
    }

    [Fact]
    public async Task SearchBooksAsync_WithNoMatches_ReturnsEmptyList()
    {
        // Arrange
        var searchTitle = "NonExistentBook";
        var books = new List<Book>
        {
            BookBuilder.New().WithId(1).WithTitle("The Great Gatsby").Build(),
            BookBuilder.New().WithId(2).WithTitle("Great Expectations").Build()
        };

        _mockRepository.Setup(r => r.GetAllAsync())
            .ReturnsAsync(books);

        // Act
        var result = await _bookService.SearchBooksAsync(title: searchTitle);

        // Assert
        result.Should().BeEmpty();
        _mockRepository.Verify(r => r.GetAllAsync(), Times.Once);
    }

    [Fact]
    public async Task SearchBooksAsync_WithNullAndEmptyParameters_ReturnsAllBooks()
    {
        // Arrange
        var books = new List<Book>
        {
            BookBuilder.New().WithId(1).WithTitle("Book 1").Build(),
            BookBuilder.New().WithId(2).WithTitle("Book 2").Build(),
            BookBuilder.New().WithId(3).WithTitle("Book 3").Build()
        };

        _mockRepository.Setup(r => r.GetAllAsync())
            .ReturnsAsync(books);

        // Act
        var result = await _bookService.SearchBooksAsync();

        // Assert
        var resultList = result.ToList();
        resultList.Should().HaveCount(3);
        resultList.Should().Contain(b => b.Title == "Book 1");
        resultList.Should().Contain(b => b.Title == "Book 2");
        resultList.Should().Contain(b => b.Title == "Book 3");

        _mockRepository.Verify(r => r.GetAllAsync(), Times.Once);
    }

    [Fact]
    public async Task SearchBooksAsync_WithEmptyStringParameters_ReturnsAllBooks()
    {
        // Arrange
        var books = new List<Book>
        {
            BookBuilder.New().WithId(1).WithTitle("Book 1").Build(),
            BookBuilder.New().WithId(2).WithTitle("Book 2").Build()
        };

        _mockRepository.Setup(r => r.GetAllAsync())
            .ReturnsAsync(books);

        // Act
        var result = await _bookService.SearchBooksAsync(title: "", author: "   ");

        // Assert
        var resultList = result.ToList();
        resultList.Should().HaveCount(2);

        _mockRepository.Verify(r => r.GetAllAsync(), Times.Once);
    }

    [Fact]
    public async Task SearchBooksAsync_WithPartialMatches_ReturnsMatchingBooks()
    {
        // Arrange
        var books = new List<Book>
        {
            BookBuilder.New().WithId(1).WithTitle("Harry Potter and the Philosopher's Stone").WithAuthor("J.K. Rowling").Build(),
            BookBuilder.New().WithId(2).WithTitle("Harry Potter and the Chamber of Secrets").WithAuthor("J.K. Rowling").Build(),
            BookBuilder.New().WithId(3).WithTitle("The Hobbit").WithAuthor("J.R.R. Tolkien").Build()
        };

        _mockRepository.Setup(r => r.GetAllAsync())
            .ReturnsAsync(books);

        // Act
        var result = await _bookService.SearchBooksAsync(title: "Potter", author: "Rowling");

        // Assert
        var resultList = result.ToList();
        resultList.Should().HaveCount(2);
        resultList.Should().Contain(b => b.Title.Contains("Harry Potter"));
        resultList.Should().NotContain(b => b.Title == "The Hobbit");

        _mockRepository.Verify(r => r.GetAllAsync(), Times.Once);
    }
}
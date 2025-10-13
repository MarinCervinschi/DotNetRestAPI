using src.API.DTOs;
using src.Core.Entities;

namespace src.UnitTests.Core.Builders;

public class BookBuilder
{
    private static int _nextId = 1;
    private int _id;
    private string _title = "Default Title";
    private string _author = "Default Author";
    private string _isbn = "1234567890";
    private BookStatus _status = BookStatus.Available;

    public BookBuilder()
    {
        _id = Interlocked.Increment(ref _nextId);
        _isbn = $"123456789{_id:D4}"; // Make ISBN unique
        _title = $"Book {_id}";
    }

    public BookBuilder WithId(int id)
    {
        _id = id;
        return this;
    }

    public BookBuilder WithTitle(string title)
    {
        _title = title;
        return this;
    }

    public BookBuilder WithAuthor(string author)
    {
        _author = author;
        return this;
    }

    public BookBuilder WithIsbn(string isbn)
    {
        _isbn = isbn;
        return this;
    }

    public BookBuilder WithStatus(BookStatus status)
    {
        _status = status;
        return this;
    }

    public BookBuilder WithAvailableStatus()
    {
        _status = BookStatus.Available;
        return this;
    }

    public BookBuilder WithUnavailableStatus()
    {
        _status = BookStatus.Unavailable;
        return this;
    }

    public Book Build()
    {
        return new Book
        {
            Id = _id,
            Title = _title,
            Author = _author,
            ISBN = _isbn,
            Status = _status
        };
    }

    public BookDto BuildDto()
    {
        return new BookDto
        {
            Id = _id,
            Title = _title,
            Author = _author,
            ISBN = _isbn,
            Status = _status
        };
    }

    public BookCreateDto BuildCreateDto()
    {
        return new BookCreateDto
        {
            Title = _title,
            Author = _author,
            ISBN = _isbn,
            Status = _status
        };
    }

    public BookUpdateDto BuildUpdateDto()
    {
        return new BookUpdateDto
        {
            Title = _title,
            Author = _author,
            ISBN = _isbn,
            Status = _status
        };
    }

    public static BookBuilder New() => new();

    public static BookBuilder ABook() => new();

    public static BookBuilder ABookWithTitle(string title) => new BookBuilder().WithTitle(title);

    public static BookBuilder ABookWithIsbn(string isbn) => new BookBuilder().WithIsbn(isbn);

    public static BookBuilder AnUnavailableBook() => new BookBuilder().WithUnavailableStatus();

    public static BookBuilder AValidBook() => new BookBuilder()
        .WithTitle("The Great Gatsby")
        .WithAuthor("F. Scott Fitzgerald")
        .WithIsbn("9780743273565")
        .WithAvailableStatus();
}
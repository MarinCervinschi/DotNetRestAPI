using src.API.DTOs;
using src.Core.Entities;
using src.Core.Interfaces;
using src.Core.Interfaces.Services;
using Microsoft.EntityFrameworkCore;

namespace src.Core.Services;

public class BookService(IRepository<Book> repository)
    : IBookService
{
    public async Task<BookDto?> GetBookByIdAsync(int id)
    {
        var book = await repository.GetByIdAsync(id);
        return book?.ToDto();
    }

    public async Task<IEnumerable<BookDto>> GetAllBooksAsync()
    {
        var books = await repository.GetAllAsync();
        return books.Select(book => book.ToDto());
    }

    public async Task<BookDto> CreateBookAsync(BookCreateDto entity)
    {
        if (await IsbnExistsAsync(entity.ISBN))
        {
            throw new InvalidOperationException($"A book with ISBN '{entity.ISBN}' already exists.");
        }

        var book = new Book
        {
            Title = entity.Title,
            Author = entity.Author,
            ISBN = entity.ISBN,
            Status = entity.Status
        };

        var createdBook = await repository.CreateAsync(book);
        return createdBook.ToDto();
    }

    public async Task<BookDto> UpdateBookAsync(int id, BookUpdateDto entity)
    {
        var existingBook = await repository.GetByIdAsync(id);
        if (existingBook == null)
        {
            throw new KeyNotFoundException($"Book with ID {id} not found.");
        }

        if (await IsbnExistsAsync(entity.ISBN, id))
        {
            throw new InvalidOperationException($"A book with ISBN '{entity.ISBN}' already exists.");
        }

        existingBook.Title = entity.Title;
        existingBook.Author = entity.Author;
        existingBook.ISBN = entity.ISBN;
        existingBook.Status = entity.Status;

        var updatedBook = await repository.UpdateAsync(existingBook);
        return updatedBook.ToDto();
    }

    public async Task<bool> DeleteBookAsync(int id)
    {
        return await repository.DeleteAsync(id);
    }

    public async Task<bool> BookExistsAsync(int id)
    {
        return await repository.ExistsAsync(id);
    }

    public async Task<bool> IsbnExistsAsync(string isbn)
    {
        var books = await repository.GetAllAsync();
        return books.Any(b => b.ISBN.Equals(isbn, StringComparison.OrdinalIgnoreCase));
    }

    public async Task<bool> IsbnExistsAsync(string isbn, int excludeBookId)
    {
        var books = await repository.GetAllAsync();
        return books.Any(b => b.ISBN.Equals(isbn, StringComparison.OrdinalIgnoreCase) && b.Id != excludeBookId);
    }
}

public static class BookExtensions
{
    public static BookDto ToDto(this Book book)
    {
        return new BookDto
        {
            Id = book.Id,
            Title = book.Title,
            Author = book.Author,
            ISBN = book.ISBN,
            Status = book.Status,
            //Reservations = book.Reservations?.Select(r => r.ToDto()).ToList() ?? new List<ReservationReadDto>()
        };
    }
}
using src.API.DTOs;
using src.Core.Entities;

namespace src.Core.Interfaces.Services;

public interface IBookService
{
    Task<BookDto?> GetBookByIdAsync(int id);
    Task<IEnumerable<BookDto>> GetAllBooksAsync();
    Task<BookDto> CreateBookAsync(BookCreateDto entity);
    Task<BookDto> UpdateBookAsync(int id, BookUpdateDto entity);
    Task<bool> DeleteBookAsync(int id);
    Task<bool> BookExistsAsync(int id);
    Task<bool> IsbnExistsAsync(string isbn);
    Task<bool> IsbnExistsAsync(string isbn, int excludeBookId);
    Task<IEnumerable<BookDto>> SearchBooksAsync(string? title = null, string? author = null, BookStatus? status = null);
}
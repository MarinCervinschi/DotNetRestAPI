using Microsoft.AspNetCore.Mvc;
using src.API.DTOs;
using src.Core.Interfaces.Services;
using src.Core.Entities;

namespace src.API.Controllers;

[ApiController]
[Route("[controller]")]
[Produces("application/json")]
public class BooksController(IBookService bookService, ILogger<BooksController> logger)
    : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<BookDto>>> GetAllBooks()
    {
        logger.LogInformation("Getting all books");
        var books = await bookService.GetAllBooksAsync();
        return Ok(books);
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<BookDto>> GetBook(int id)
    {
        logger.LogInformation("Getting book with id {Id}", id);
        var book = await bookService.GetBookByIdAsync(id);

        if (book != null) return Ok(book);
        logger.LogWarning("Book with id {Id} not found", id);
        return NotFound();
    }

    [HttpGet("search")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<BookDto>>> SearchBooks(
        [FromQuery] string? title = null,
        [FromQuery] string? author = null,
        [FromQuery] BookStatus? status = null)
    {
        logger.LogInformation("Searching books with title: {Title}, author: {Author}, status: {Status}",
            title, author, status);

        var books = await bookService.SearchBooksAsync(title, author, status);
        return Ok(books);
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<BookDto>> CreateBook(BookCreateDto bookCreateDto)
    {
        if (!ModelState.IsValid)
        {
            logger.LogWarning("Invalid model state for creating book");
            return BadRequest(ModelState);
        }

        logger.LogInformation("Creating new book with ISBN: {ISBN}", bookCreateDto.ISBN);
        try
        {
            var bookReadDto = await bookService.CreateBookAsync(bookCreateDto);
            return CreatedAtAction(nameof(GetBook), new { id = bookReadDto.Id }, bookReadDto);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("ISBN"))
        {
            logger.LogWarning("Attempt to create book with duplicate ISBN: {ISBN}", bookCreateDto.ISBN);
            return Conflict(new { message = ex.Message, field = "ISBN" });
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error occurred while creating book");
            return StatusCode(StatusCodes.Status500InternalServerError, "Internal server error");
        }
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<BookDto>> UpdateBook(int id, BookUpdateDto bookUpdateDto)
    {
        if (!ModelState.IsValid)
        {
            logger.LogWarning("Invalid model state for updating book with id {Id}", id);
            return BadRequest(ModelState);
        }

        logger.LogInformation("Updating book with id {Id}", id);

        try
        {
            var updatedBook = await bookService.UpdateBookAsync(id, bookUpdateDto);
            return Ok(updatedBook);
        }
        catch (KeyNotFoundException)
        {
            logger.LogWarning("Book with id {Id} not found", id);
            return NotFound();
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("ISBN"))
        {
            logger.LogWarning("Attempt to update book {Id} with duplicate ISBN: {ISBN}", id, bookUpdateDto.ISBN);
            return Conflict(new { message = ex.Message, field = "ISBN" });
        }
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteBook(int id)
    {
        logger.LogInformation("Deleting book with id {Id}", id);

        var deleted = await bookService.DeleteBookAsync(id);
        if (deleted) return NoContent();
        logger.LogWarning("Book with id {Id} not found", id);
        return NotFound();
    }
}
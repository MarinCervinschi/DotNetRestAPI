using System.ComponentModel.DataAnnotations;
using src.Core.Entities;

namespace src.API.DTOs;

public class BookDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public string ISBN { get; set; } = string.Empty;

    public BookStatus Status { get; set; }
}

public class BookCreateDto
{
    [Required(ErrorMessage = "Title is required")]
    [StringLength(200, MinimumLength = 1, ErrorMessage = "Title must be between 1 and 200 characters")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Author is required")]
    [StringLength(100, MinimumLength = 1, ErrorMessage = "Author must be between 1 and 100 characters")]
    public string Author { get; set; } = string.Empty;

    [Required(ErrorMessage = "ISBN is required")]
    [StringLength(13, MinimumLength = 10, ErrorMessage = "ISBN must be between 10 and 13 characters")]
    public string ISBN { get; set; } = string.Empty;

    public BookStatus Status { get; set; } = BookStatus.Available;
}

public class BookUpdateDto
{
    [Required(ErrorMessage = "Title is required")]
    [StringLength(200, MinimumLength = 1, ErrorMessage = "Title must be between 1 and 200 characters")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Author is required")]
    [StringLength(100, MinimumLength = 1, ErrorMessage = "Author must be between 1 and 100 characters")]
    public string Author { get; set; } = string.Empty;

    [Required(ErrorMessage = "ISBN is required")]
    [StringLength(13, MinimumLength = 10, ErrorMessage = "ISBN must be between 10 and 13 characters")]
    public string ISBN { get; set; } = string.Empty;

    public BookStatus Status { get; set; }
}

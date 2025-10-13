using System.ComponentModel.DataAnnotations;

namespace src.API.DTOs;

public class ReservationDto
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public int BookId { get; set; }
    public DateTime ReservationDate { get; set; }
    public DateTime ExpirationDate { get; set; }

    // Navigation properties as DTOs
    public CustomerDto? Customer { get; set; }
    public BookDto? Book { get; set; }
}

public class ReservationCreateDto
{
    [Required(ErrorMessage = "CustomerId is required")]
    [Range(1, int.MaxValue, ErrorMessage = "CustomerId must be greater than 0")]
    public int CustomerId { get; set; }

    [Required(ErrorMessage = "BookId is required")]
    [Range(1, int.MaxValue, ErrorMessage = "BookId must be greater than 0")]
    public int BookId { get; set; }
}

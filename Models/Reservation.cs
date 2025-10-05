using System.ComponentModel.DataAnnotations;

namespace DotNetRestAPI.Models;

public class Reservation
{
    public int Id { get; set; }

    // Foreign Keys
    [Required] public int CustomerId { get; set; }

    [Required] public int BookId { get; set; }

    // Date fields
    [Required] public DateTime ReservationDate { get; set; } = DateTime.UtcNow;

    [Required] public DateTime ExpirationDate { get; set; }

    // Navigation Properties
    public Customer Customer { get; set; } = null!;
    public Book Book { get; set; } = null!;
}
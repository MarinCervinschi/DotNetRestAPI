using System.ComponentModel.DataAnnotations;
using src.Core.Interfaces;

namespace src.Core.Entities;

public class Reservation : IEntity
{
    public int Id { get; set; }

    // Foreign Keys
    [Required] public int CustomerId { get; set; }

    [Required] public int BookId { get; set; }

    // Date fields
    [Required] public DateTime ReservationDate { get; set; }

    [Required] public DateTime ExpirationDate { get; set; }

    // Navigation Properties
    public Customer Customer { get; set; } = null!;
    public Book Book { get; set; } = null!;
}

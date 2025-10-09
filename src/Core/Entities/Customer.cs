using System.ComponentModel.DataAnnotations;
using src.Core.Interfaces;

namespace src.Core.Entities;

public class Customer : IEntity
{
    public int Id { get; set; }

    [Required] [MaxLength(100)] public string FirstName { get; set; } = string.Empty;

    [Required] [MaxLength(100)] public string LastName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [MaxLength(255)]
    public string Email { get; set; } = string.Empty;

    // Navigation Properties
    public List<Reservation> Reservations { get; set; } = new();
}
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace DotNetRestAPI.Core.Entities;

public enum BookStatus
{
    Available = 0,
    Unavailable = 1,
}

public class Book
{
    public int Id { get; set; }

    [Required] [MaxLength(200)] public string Title { get; set; } = string.Empty;

    [Required] [MaxLength(100)] public string Author { get; set; } = string.Empty;

    [Required] [MaxLength(13)] public string ISBN { get; set; } = string.Empty;

    public BookStatus Status { get; set; } = BookStatus.Available;
    public List<Reservation> Reservations { get; set; } = [];
}
using System.ComponentModel.DataAnnotations;
using src.Core.Interfaces;

namespace src.Core.Entities;

public class Admin : IEntity
{
    public int Id { get; set; }

    [Required][MaxLength(50)] public string Username { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [MaxLength(255)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MinLength(6)]
    [MaxLength(255)]
    public string PasswordHash { get; set; } = string.Empty;
}

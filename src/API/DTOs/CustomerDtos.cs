using System.ComponentModel.DataAnnotations;

namespace src.API.DTOs;

public class CustomerDto
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    // public List<ReservationReadDto> Reservations { get; set; } = new();
}

public class CustomerCreateDto
{
    [Required(ErrorMessage = "FirstName is required")]
    [StringLength(100, MinimumLength = 1, ErrorMessage = "FirstName must be between 1 and 100 characters")]
    public string FirstName { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "LastName is required")]
    [StringLength(100, MinimumLength = 1, ErrorMessage = "LastName must be between 1 and 100 characters")]
    public string LastName { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Email must be a valid email address")]
    [MaxLength(255)]
    public string Email { get; set; } = string.Empty;
}

public class CustomerUpdateDto
{
    [Required(ErrorMessage = "FirstName is required")]
    [StringLength(100, MinimumLength = 1, ErrorMessage = "FirstName must be between 1 and 100 characters")]
    public string FirstName { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "LastName is required")]
    [StringLength(100, MinimumLength = 1, ErrorMessage = "LastName must be between 1 and 100 characters")]
    public string LastName { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Email must be a valid email address")]
    [MaxLength(255)]
    public string Email { get; set; } = string.Empty;
}
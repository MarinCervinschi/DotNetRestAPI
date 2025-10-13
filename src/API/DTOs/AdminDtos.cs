using System.ComponentModel.DataAnnotations;

namespace src.API.DTOs;

public class AdminDto
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}

public class AdminLoginDto
{
    [Required(ErrorMessage = "Username is required")]
    [MaxLength(50, ErrorMessage = "Username cannot exceed 50 characters")]
    public string Username { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password is required")]
    [MinLength(6, ErrorMessage = "Password must be at least 6 characters long")]
    [MaxLength(255, ErrorMessage = "Password cannot exceed 255 characters")]
    public string Password { get; set; } = string.Empty;
}

public class AdminCreateDto
{
    [Required(ErrorMessage = "Username is required")]
    [MaxLength(50, ErrorMessage = "Username cannot exceed 50 characters")]
    public string Username { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Please enter a valid email address")]
    [MaxLength(255, ErrorMessage = "Email cannot exceed 255 characters")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password is required")]
    [MinLength(6, ErrorMessage = "Password must be at least 6 characters long")]
    [MaxLength(255, ErrorMessage = "Password cannot exceed 255 characters")]
    public string Password { get; set; } = string.Empty;
}

public class LoginResponseDto
{
    public string Token { get; set; } = string.Empty;
    public AdminDto Admin { get; set; } = null!;
}

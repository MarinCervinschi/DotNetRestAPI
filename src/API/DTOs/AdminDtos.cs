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
    [Required] public string Username { get; set; } = string.Empty;

    [Required] public string Password { get; set; } = string.Empty;
}

public class LoginResponseDto
{
    public string Token { get; set; } = string.Empty;
    public AdminDto Admin { get; set; } = null!;
}
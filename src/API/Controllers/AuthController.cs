using Microsoft.AspNetCore.Mvc;
using src.API.DTOs;
using src.Core.Interfaces.Services;

namespace src.API.Controllers;

/// <summary>
/// Authentication endpoints - No authentication required
/// </summary>
[ApiController]
[Route("[controller]")]
[Produces("application/json")]
public class AuthController(IAuthService authService, ILogger<AuthController> logger) : ControllerBase
{
    /// <summary>
    /// Admin login - Returns JWT token for authentication
    /// </summary>
    /// <param name="loginDto">Login credentials (username and password)</param>
    [HttpPost("login")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<LoginResponseDto>> Login([FromBody] AdminLoginDto? loginDto)
    {
        if (loginDto == null)
        {
            logger.LogWarning("Login attempt with null data");
            return BadRequest("Login data is required");
        }

        if (!ModelState.IsValid)
        {
            logger.LogWarning("Invalid model state for login attempt");
            return BadRequest(ModelState);
        }

        logger.LogInformation("Login attempt for username: {Username}", loginDto.Username);

        var result = await authService.LoginAsync(loginDto);

        if (result == null)
        {
            logger.LogWarning("Failed login attempt for username: {Username}", loginDto.Username);
            return Unauthorized("Invalid username or password");
        }

        logger.LogInformation("Successful login for username: {Username}", loginDto.Username);
        return Ok(result);
    }
}
using Microsoft.AspNetCore.Mvc;
using src.API.DTOs;
using src.Core.Interfaces.Services;

namespace src.API.Controllers;

[ApiController]
[Route("[controller]")]
[Produces("application/json")]
public class AuthController(IAuthService authService, ILogger<AuthController> logger) : ControllerBase
{
    [HttpPost("login")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<LoginResponseDto>> Login([FromBody] AdminLoginDto loginDto)
    {
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
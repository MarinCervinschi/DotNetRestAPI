using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using src.API.DTOs;
using src.Core.Interfaces.Services;

namespace src.API.Controllers;

/// <summary>
/// Admin management endpoints - requires JWT authentication
/// </summary>
[ApiController]
[Route("[controller]")]
[Produces("application/json")]
[Authorize]
public class AdminController(IAdminService adminService, ILogger<AdminController> logger)
    : ControllerBase
{
    /// <summary>
    /// Get admin information by username
    /// </summary>
    /// <param name="username">Admin username</param>
    [HttpGet("{username}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AdminDto>> GetByUsername(string username)
    {
        logger.LogInformation("Getting admin by username: {Username}", username);

        var admin = await adminService.GetByUsernameAsync(username);

        if (admin == null)
        {
            return NotFound();
        }

        return Ok(admin);
    }

    /// <summary>
    /// Create a new admin account
    /// </summary>
    /// <param name="createDto">Admin data (username, email, password)</param>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AdminDto>> CreateAdmin([FromBody] AdminCreateDto createDto)
    {
        if (!ModelState.IsValid)
        {
            logger.LogWarning("Invalid model state for creating admin");
            return BadRequest(ModelState);
        }

        logger.LogInformation("Creating new admin with username: {Username}", createDto.Username);

        var admin = await adminService.CreateAdminAsync(createDto.Username, createDto.Email, createDto.Password);

        return CreatedAtAction(nameof(GetByUsername), new { username = admin.Username }, admin);
    }
}
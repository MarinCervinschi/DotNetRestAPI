using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using src.API.DTOs;
using src.Core.Interfaces.Services;

namespace src.API.Controllers;

[ApiController]
[Route("[controller]")]
[Produces("application/json")]
[Authorize]
public class AdminController(IAdminService adminService, ILogger<AdminController> logger)
    : ControllerBase
{
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

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AdminDto>> CreateAdmin([FromBody] AdminCreateDto createDto)
    {
        logger.LogInformation("Creating new admin with username: {Username}", createDto.Username);

        var admin = await adminService.CreateAdminAsync(createDto.Username, createDto.Email, createDto.Password);

        return CreatedAtAction(nameof(GetByUsername), new { username = admin.Username }, admin);
    }
}
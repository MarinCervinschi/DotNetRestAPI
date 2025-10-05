using Microsoft.AspNetCore.Mvc;

namespace DotNetRestAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HomeController : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        return Ok(new { message = "Hello World", timestamp = DateTime.UtcNow });
    }

    [HttpGet("hello")]
    public IActionResult HelloWorld()
    {
        return Ok(new
        {
            message = "Hello World from .NET REST API!",
            version = "1.0.0",
            environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"
        });
    }
}
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using src.API.DTOs;
using src.Core.Interfaces.Services;

namespace src.API.Controllers;

/// <summary>
/// Customer management endpoints - requires JWT authentication
/// </summary>
[ApiController]
[Route("[controller]")]
[Produces("application/json")]
[Authorize]
public class CustomersController(ICustomerService customerService, ILogger<CustomersController> logger)
    : ControllerBase
{
    /// <summary>
    /// Get all customers
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IEnumerable<CustomerDto>>> GetAllCustomers()
    {
        logger.LogInformation("Getting all customers");
        var customers = await customerService.GetAllCustomersAsync();
        return Ok(customers);
    }

    /// <summary>
    /// Get customer by ID
    /// </summary>
    /// <param name="id">Customer ID</param>
    [HttpGet("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<CustomerDto>> GetCustomer(int id)
    {
        logger.LogInformation("Getting customer with id {Id}", id);
        var customer = await customerService.GetCustomerByIdAsync(id);

        if (customer != null) return Ok(customer);
        logger.LogWarning("Customer with id {Id} not found", id);
        return NotFound();
    }

    /// <summary>
    /// Create a new customer - Admin only
    /// </summary>
    /// <param name="customerCreateDto">Customer data</param>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<CustomerDto>> CreateCustomer(CustomerCreateDto customerCreateDto)
    {
        if (!ModelState.IsValid)
        {
            logger.LogWarning("Invalid model state for creating customer");
            return BadRequest(ModelState);
        }

        logger.LogInformation("Creating new customer");
        try
        {
            var customerReadDto = await customerService.CreateCustomerAsync(customerCreateDto);
            return CreatedAtAction(nameof(GetCustomer), new { id = customerReadDto.Id }, customerReadDto);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error occurred while creating customer");
            return StatusCode(StatusCodes.Status500InternalServerError, "Internal server error");
        }
    }

    /// <summary>
    /// Update customer - Admin only
    /// </summary>
    /// <param name="id">Customer ID</param>
    /// <param name="customerUpdateDto">Updated customer data</param>
    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<CustomerDto>> UpdateCustomer(int id, CustomerUpdateDto customerUpdateDto)
    {
        if (!ModelState.IsValid)
        {
            logger.LogWarning("Invalid model state for updating customer with id {Id}", id);
            return BadRequest(ModelState);
        }

        logger.LogInformation("Updating customer with id {Id}", id);

        try
        {
            var updatedCustomer = await customerService.UpdateCustomerAsync(id, customerUpdateDto);
            return Ok(updatedCustomer);
        }
        catch (KeyNotFoundException)
        {
            logger.LogWarning("Customer with id {Id} not found", id);
            return NotFound();
        }
    }

    /// <summary>
    /// Delete customer - Admin only
    /// </summary>
    /// <param name="id">Customer ID</param>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> DeleteCustomer(int id)
    {
        logger.LogInformation("Deleting customer with id {Id}", id);

        var deleted = await customerService.DeleteCustomerAsync(id);
        if (deleted) return NoContent();
        logger.LogWarning("Customer with id {Id} not found", id);
        return NotFound();
    }
}
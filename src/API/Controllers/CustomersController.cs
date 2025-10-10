using Microsoft.AspNetCore.Mvc;
using src.API.DTOs;
using src.Core.Interfaces.Services;

namespace src.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class CustomersController(ICustomerService customerService, ILogger<CustomersController> logger)
    : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<CustomerDto>>> GetAllCustomers()
    {
        logger.LogInformation("Getting all customers");
        var customers = await customerService.GetAllCustomersAsync();
        return Ok(customers);
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CustomerDto>> GetCustomer(int id)
    {
        logger.LogInformation("Getting customer with id {Id}", id);
        var customer = await customerService.GetCustomerByIdAsync(id);

        if (customer != null) return Ok(customer);
        logger.LogWarning("Customer with id {Id} not found", id);
        return NotFound();
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
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

    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
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

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteCustomer(int id)
    {
        logger.LogInformation("Deleting customer with id {Id}", id);

        var deleted = await customerService.DeleteCustomerAsync(id);
        if (deleted) return NoContent();
        logger.LogWarning("Customer with id {Id} not found", id);
        return NotFound();
    }
}
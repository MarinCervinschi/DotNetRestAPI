using Microsoft.AspNetCore.Mvc;
using src.API.DTOs;
using src.Core.Interfaces.Services;

namespace src.API.Controllers;

[ApiController]
[Route("[controller]")]
[Produces("application/json")]
public class ReservationsController(IReservationService reservationService, ILogger<ReservationsController> logger)
    : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<ReservationDto>>> GetAllReservations()
    {
        logger.LogInformation("Getting all reservations");
        var reservations = await reservationService.GetAllReservationsAsync();
        return Ok(reservations);
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ReservationDto>> GetReservation(int id)
    {
        logger.LogInformation("Getting reservation with id {Id}", id);
        var reservation = await reservationService.GetReservationByIdAsync(id);

        if (reservation != null) return Ok(reservation);
        logger.LogWarning("Reservation with id {Id} not found", id);
        return NotFound();
    }

    [HttpGet("customer/{customerId:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<ReservationDto>>> GetReservationsByCustomer(int customerId)
    {
        logger.LogInformation("Getting reservations for customer with id {CustomerId}", customerId);
        var reservations = await reservationService.GetReservationsByCustomerIdAsync(customerId);
        return Ok(reservations);
    }

    [HttpGet("book/{bookId:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<ReservationDto>>> GetReservationsByBook(int bookId)
    {
        logger.LogInformation("Getting reservations for book with id {BookId}", bookId);
        var reservations = await reservationService.GetReservationsByBookIdAsync(bookId);
        return Ok(reservations);
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ReservationDto>> CreateReservation(ReservationCreateDto reservationCreateDto)
    {
        if (!ModelState.IsValid)
        {
            logger.LogWarning("Invalid model state for creating reservation");
            return BadRequest(ModelState);
        }

        logger.LogInformation("Creating new reservation for customer {CustomerId} and book {BookId}",
            reservationCreateDto.CustomerId, reservationCreateDto.BookId);
        try
        {
            var reservationDto = await reservationService.CreateReservationAsync(reservationCreateDto);
            return CreatedAtAction(nameof(GetReservation), new { id = reservationDto.Id }, reservationDto);
        }
        catch (KeyNotFoundException ex)
        {
            logger.LogWarning("Resource not found while creating reservation: {Message}", ex.Message);
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            logger.LogWarning("Invalid operation while creating reservation: {Message}", ex.Message);
            return Conflict(new { message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            logger.LogWarning("Invalid argument while creating reservation: {Message}", ex.Message);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error occurred while creating reservation");
            return StatusCode(StatusCodes.Status500InternalServerError, "Internal server error");
        }
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteReservation(int id)
    {
        logger.LogInformation("Deleting reservation with id {Id}", id);

        var deleted = await reservationService.DeleteReservationAsync(id);
        if (deleted) return NoContent();
        logger.LogWarning("Reservation with id {Id} not found", id);
        return NotFound();
    }
}
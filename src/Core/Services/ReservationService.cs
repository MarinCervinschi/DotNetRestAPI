using src.API.DTOs;
using src.Core.Entities;
using src.Core.Interfaces.Repositories;
using src.Core.Interfaces.Services;

namespace src.Core.Services;

public class ReservationService(
    IReservationRepository reservationRepository,
    ICustomerService customerService,
    IBookService bookService)
    : IReservationService
{
    public async Task<ReservationDto?> GetReservationByIdAsync(int id)
    {
        var reservation = await reservationRepository.GetByIdAsync(id);
        if (reservation == null) return null;

        return await reservation.ToDtoAsync(customerService, bookService);
    }

    public async Task<IEnumerable<ReservationDto>> GetAllReservationsAsync()
    {
        var reservations = await reservationRepository.GetAllAsync();
        var reservationDtos = new List<ReservationDto>();

        foreach (var reservation in reservations)
        {
            var dto = await reservation.ToDtoAsync(customerService, bookService);
            reservationDtos.Add(dto);
        }

        return reservationDtos;
    }

    public async Task<IEnumerable<ReservationDto>> GetReservationsByCustomerIdAsync(int customerId)
    {
        var reservations = await reservationRepository.GetAllAsync();
        var customerReservations = reservations.Where(r => r.CustomerId == customerId);
        var reservationDtos = new List<ReservationDto>();

        foreach (var reservation in customerReservations)
        {
            var dto = await reservation.ToDtoAsync(customerService, bookService);
            reservationDtos.Add(dto);
        }

        return reservationDtos;
    }

    public async Task<IEnumerable<ReservationDto>> GetReservationsByBookIdAsync(int bookId)
    {
        var reservations = await reservationRepository.GetAllAsync();
        var bookReservations = reservations.Where(r => r.BookId == bookId);
        var reservationDtos = new List<ReservationDto>();

        foreach (var reservation in bookReservations)
        {
            var dto = await reservation.ToDtoAsync(customerService, bookService);
            reservationDtos.Add(dto);
        }

        return reservationDtos;
    }

    public async Task<ReservationDto> CreateReservationAsync(ReservationCreateDto entity)
    {
        if (!await customerService.CustomerExistsAsync(entity.CustomerId))
        {
            throw new KeyNotFoundException($"Customer with ID {entity.CustomerId} not found.");
        }

        var reservation = new Reservation
        {
            CustomerId = entity.CustomerId,
            BookId = entity.BookId,
            ReservationDate = DateTime.UtcNow,
            ExpirationDate = DateTime.UtcNow.AddDays(7)
        };

        var createdReservation = await reservationRepository.CreateAsync(reservation);
        return await createdReservation.ToDtoAsync(customerService, bookService);
    }

    public async Task<bool> DeleteReservationAsync(int id)
    {
        return await reservationRepository.DeleteAsync(id);
    }

    public async Task<bool> ReservationExistsAsync(int id)
    {
        return await reservationRepository.ExistsAsync(id);
    }
}

public static class ReservationExtensions
{
    public static async Task<ReservationDto> ToDtoAsync(this Reservation reservation, ICustomerService customerService,
        IBookService bookService)
    {
        var customer = await customerService.GetCustomerByIdAsync(reservation.CustomerId);
        var book = await bookService.GetBookByIdAsync(reservation.BookId);

        return new ReservationDto
        {
            Id = reservation.Id,
            CustomerId = reservation.CustomerId,
            BookId = reservation.BookId,
            ReservationDate = reservation.ReservationDate,
            ExpirationDate = reservation.ExpirationDate,
            Customer = customer,
            Book = book
        };
    }

    public static ReservationDto ToDto(this Reservation reservation)
    {
        return new ReservationDto
        {
            Id = reservation.Id,
            CustomerId = reservation.CustomerId,
            BookId = reservation.BookId,
            ReservationDate = reservation.ReservationDate,
            ExpirationDate = reservation.ExpirationDate
        };
    }
}
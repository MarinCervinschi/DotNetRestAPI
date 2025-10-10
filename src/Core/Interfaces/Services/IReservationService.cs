using src.API.DTOs;

namespace src.Core.Interfaces.Services;

public interface IReservationService
{
    Task<ReservationDto?> GetReservationByIdAsync(int id);
    Task<IEnumerable<ReservationDto>> GetAllReservationsAsync();
    Task<IEnumerable<ReservationDto>> GetReservationsByCustomerIdAsync(int customerId);
    Task<IEnumerable<ReservationDto>> GetReservationsByBookIdAsync(int bookId);
    Task<ReservationDto> CreateReservationAsync(ReservationCreateDto entity);
    Task<bool> DeleteReservationAsync(int id);
    Task<bool> ReservationExistsAsync(int id);
}
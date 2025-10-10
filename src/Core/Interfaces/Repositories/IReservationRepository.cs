using src.Core.Entities;
using src.Core.Interfaces;

namespace src.Core.Interfaces.Repositories;

public interface IReservationRepository : IRepository<Reservation>
{
    Task<Reservation> CreateReservationWithBookUpdateAsync(Reservation reservation);
    Task<bool> DeleteReservationWithBookUpdateAsync(int id);
    Task<bool> RestoreBookAvailabilityAsync(int bookId);
}
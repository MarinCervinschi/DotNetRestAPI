using src.Core.Entities;
using src.Core.Interfaces;

namespace src.Core.Interfaces.Repositories;

public interface IReservationRepository : IRepository<Reservation>
{
    Task<bool> RestoreBookAvailabilityAsync(int bookId);
}

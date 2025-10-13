using src.Core.Entities;
using src.Core.Interfaces.Repositories;
using src.Infrastructure.Data;

namespace src.Infrastructure.Repositories;

public class ReservationRepository(ApplicationDbContext context)
    : Repository<Reservation>(context), IReservationRepository
{
    private readonly ApplicationDbContext _context = context;

    public override async Task<Reservation> CreateAsync(Reservation reservation)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var book = await _context.Books.FindAsync(reservation.BookId);
            if (book == null)
            {
                throw new KeyNotFoundException($"Book with ID {reservation.BookId} not found.");
            }

            if (book.Status != BookStatus.Available)
            {
                throw new InvalidOperationException(
                    $"Book with ID {reservation.BookId} is not available for reservation.");
            }

            var createdReservation = await base.CreateAsync(reservation);

            book.Status = BookStatus.Unavailable;
            _context.Books.Update(book);

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return createdReservation;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public override async Task<bool> DeleteAsync(int id)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var reservation = await GetByIdAsync(id);
            if (reservation == null)
            {
                return false;
            }

            var deleted = await base.DeleteAsync(id);
            if (!deleted)
            {
                return false;
            }

            var book = await _context.Books.FindAsync(reservation.BookId);
            if (book != null)
            {
                book.Status = BookStatus.Available;
                _context.Books.Update(book);
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return true;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<bool> RestoreBookAvailabilityAsync(int bookId)
    {
        try
        {
            var book = await _context.Books.FindAsync(bookId);
            if (book == null)
            {
                return false;
            }

            book.Status = BookStatus.Available;
            _context.Books.Update(book);
            await _context.SaveChangesAsync();

            return true;
        }
        catch
        {
            return false;
        }
    }
}

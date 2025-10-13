using src.API.DTOs;
using src.Core.Entities;

namespace src.UnitTests.Core.Builders;

public class ReservationBuilder
{
    private static int _nextId = 1;
    private int _id;
    private int _customerId = 1;
    private int _bookId = 1;
    private DateTime _reservationDate = DateTime.UtcNow;
    private DateTime _expirationDate = DateTime.UtcNow.AddDays(7);

    public ReservationBuilder()
    {
        _id = Interlocked.Increment(ref _nextId);
    }

    public static ReservationBuilder New() => new();

    public ReservationBuilder WithId(int id)
    {
        _id = id;
        return this;
    }

    public ReservationBuilder WithCustomerId(int customerId)
    {
        _customerId = customerId;
        return this;
    }

    public ReservationBuilder WithBookId(int bookId)
    {
        _bookId = bookId;
        return this;
    }

    public ReservationBuilder WithReservationDate(DateTime reservationDate)
    {
        _reservationDate = reservationDate;
        return this;
    }

    public ReservationBuilder WithExpirationDate(DateTime expirationDate)
    {
        _expirationDate = expirationDate;
        return this;
    }

    public Reservation Build()
    {
        return new Reservation
        {
            Id = _id,
            CustomerId = _customerId,
            BookId = _bookId,
            ReservationDate = _reservationDate,
            ExpirationDate = _expirationDate
        };
    }

    public ReservationDto BuildDto()
    {
        return new ReservationDto
        {
            Id = _id,
            CustomerId = _customerId,
            BookId = _bookId,
            ReservationDate = _reservationDate,
            ExpirationDate = _expirationDate
        };
    }

    public ReservationCreateDto BuildCreateDto()
    {
        return new ReservationCreateDto
        {
            CustomerId = _customerId,
            BookId = _bookId
        };
    }
}

using src.API.DTOs;
using src.Core.Entities;

namespace src.UnitTests.Core.Builders;

public class ReservationBuilder
{
    private int _id = 1;
    private int _customerId = 1;
    private int _bookId = 1;
    private DateTime _reservationDate = DateTime.UtcNow;
    private DateTime _expirationDate = DateTime.UtcNow.AddDays(7);
    private CustomerDto? _customer;
    private BookDto? _book;

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

    public ReservationBuilder WithCustomer(CustomerDto customer)
    {
        _customer = customer;
        return this;
    }

    public ReservationBuilder WithBook(BookDto book)
    {
        _book = book;
        return this;
    }

    public ReservationBuilder WithExpiredDate()
    {
        _expirationDate = DateTime.UtcNow.AddDays(-1);
        return this;
    }

    public ReservationBuilder WithFutureExpiration(int days)
    {
        _expirationDate = DateTime.UtcNow.AddDays(days);
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
            ExpirationDate = _expirationDate,
            Customer = _customer,
            Book = _book
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

    public static ReservationBuilder New() => new();

    public static ReservationBuilder AReservation() => new();

    public static ReservationBuilder AReservationForCustomer(int customerId) => 
        new ReservationBuilder().WithCustomerId(customerId);

    public static ReservationBuilder AReservationForBook(int bookId) => 
        new ReservationBuilder().WithBookId(bookId);

    public static ReservationBuilder AnExpiredReservation() => 
        new ReservationBuilder().WithExpiredDate();

    public static ReservationBuilder AValidReservation() => new ReservationBuilder()
        .WithCustomerId(1)
        .WithBookId(1)
        .WithReservationDate(DateTime.UtcNow)
        .WithFutureExpiration(14);

    public static ReservationBuilder AReservationWithCustomerAndBook(int customerId, int bookId) => 
        new ReservationBuilder()
            .WithCustomerId(customerId)
            .WithBookId(bookId);
}

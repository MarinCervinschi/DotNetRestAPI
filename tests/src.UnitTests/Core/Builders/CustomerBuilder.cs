using src.Core.Entities;

namespace src.UnitTests.Core.Builders;

public class CustomerBuilder
{
    private readonly Customer _customer = new()
    {
        Id = 1,
        FirstName = "John",
        LastName = "Doe",
        Email = "john.doe@example.com",
        Reservations = new List<Reservation>()
    };

    public CustomerBuilder WithId(int id)
    {
        _customer.Id = id;
        return this;
    }

    public CustomerBuilder WithFirstName(string firstName)
    {
        _customer.FirstName = firstName;
        return this;
    }

    public CustomerBuilder WithLastName(string lastName)
    {
        _customer.LastName = lastName;
        return this;
    }

    public CustomerBuilder WithEmail(string email)
    {
        _customer.Email = email;
        return this;
    }

    public CustomerBuilder WithReservations(List<Reservation> reservations)
    {
        _customer.Reservations = reservations;
        return this;
    }

    public Customer Build() => _customer;

    public static CustomerBuilder Default() => new CustomerBuilder();
}
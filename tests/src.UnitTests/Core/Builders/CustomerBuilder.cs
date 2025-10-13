using src.API.DTOs;
using src.Core.Entities;

namespace src.UnitTests.Core.Builders;

public class CustomerBuilder
{
    private static int _nextId = 1;
    private int _id;
    private string _firstName = "John";
    private string _lastName = "Doe";
    private string _email = "john.doe@example.com";

    public CustomerBuilder()
    {
        _id = Interlocked.Increment(ref _nextId);
        _email = $"customer{_id}@example.com"; // Make email unique too
    }

    public static CustomerBuilder New() => new();

    public static CustomerBuilder Default() => new();

    public CustomerBuilder WithId(int id)
    {
        _id = id;
        return this;
    }

    public CustomerBuilder WithFirstName(string firstName)
    {
        _firstName = firstName;
        return this;
    }

    public CustomerBuilder WithLastName(string lastName)
    {
        _lastName = lastName;
        return this;
    }

    public CustomerBuilder WithEmail(string email)
    {
        _email = email;
        return this;
    }

    public Customer Build()
    {
        return new Customer
        {
            Id = _id,
            FirstName = _firstName,
            LastName = _lastName,
            Email = _email
        };
    }

    public CustomerDto BuildDto()
    {
        return new CustomerDto
        {
            Id = _id,
            FirstName = _firstName,
            LastName = _lastName,
            Email = _email
        };
    }

    public CustomerCreateDto BuildCreateDto()
    {
        return new CustomerCreateDto
        {
            FirstName = _firstName,
            LastName = _lastName,
            Email = _email
        };
    }
}
using src.API.DTOs;
using src.Core.Entities;

namespace src.UnitTests.Core.Builders;

public class AdminBuilder
{
    private static int _nextId = 1;
    private int _id;
    private string _username = "admin";
    private string _email = "admin@example.com";
    private string _passwordHash = "$2a$11$abcdefghijklmnopqrstuvwxyz123456789";

    public AdminBuilder()
    {
        _id = Interlocked.Increment(ref _nextId);
        _username = $"admin{_id}";
        _email = $"admin{_id}@example.com";
    }

    public static AdminBuilder New() => new();

    public AdminBuilder WithId(int id)
    {
        _id = id;
        return this;
    }

    public AdminBuilder WithUsername(string username)
    {
        _username = username;
        return this;
    }

    public AdminBuilder WithEmail(string email)
    {
        _email = email;
        return this;
    }

    public AdminBuilder WithPasswordHash(string passwordHash)
    {
        _passwordHash = passwordHash;
        return this;
    }

    public Admin Build()
    {
        return new Admin
        {
            Id = _id,
            Username = _username,
            Email = _email,
            PasswordHash = _passwordHash
        };
    }

    public AdminDto BuildDto()
    {
        return new AdminDto
        {
            Id = _id,
            Username = _username,
            Email = _email
        };
    }

    public AdminCreateDto BuildCreateDto()
    {
        return new AdminCreateDto
        {
            Username = _username,
            Email = _email,
            Password = "password123" // Plain password for creation
        };
    }

    public AdminLoginDto BuildLoginDto()
    {
        return new AdminLoginDto
        {
            Username = _username,
            Password = "password123"
        };
    }
}

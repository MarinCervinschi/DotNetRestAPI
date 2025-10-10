# DotNetRestAPI

REST API for library management with .NET 9 and PostgreSQL.

## Main Commands

### Setup & Launch

```bash
# Install EF Core Tools
dotnet tool install --global dotnet-ef

# Start PostgreSQL database
docker-compose up -d postgres

# Apply migrations
dotnet ef database update --project src

# Start API
dotnet run --project src
```

### Database

```bash
# Create new migration
dotnet ef migrations add <MigrationName> --project src --output-dir Infrastructure/Data/Migrations

# Apply migrations
dotnet ef database update --project src

# Remove last migration (if not applied)
dotnet ef migrations remove --project src

# Reset database
dotnet ef database drop --project src && dotnet ef database update --project src
```

### Testing

```bash
# Run all tests
dotnet test

# Run tests with coverage
dotnet test --collect:"XPlat Code Coverage"
```

## API Endpoints

**Base URL:** `http://localhost:5163/`

### Books

- `GET /books` - List all books
- `GET /books/{id}` - Get book details
- `GET /books/search` - Search books by title, author, and status
- `POST /books` - Create new book
- `PUT /books/{id}` - Update book
- `DELETE /books/{id}` - Delete book

### Customers

- `GET /customers` - List all customers
- `GET /customers/{id}` - Get customer details
- `POST /customers` - Create new customer
- `PUT /customers/{id}` - Update customer
- `DELETE /customers/{id}` - Delete customer

### Reservations

- `GET /reservations` - List all reservations
- `GET /reservations/{id}` - Get reservation details
- `GET /reservations/customer/{customerId}` - Get reservations by customer
- `GET /reservations/book/{bookId}` - Get reservations by book
- `POST /reservations` - Create new reservation
- `DELETE /reservations/{id}` - Delete reservation

### Health & Info

- `GET /health` - Application status
- **Swagger UI:** `http://localhost:5000/swagger` (development only)

## Technologies

- **.NET 9** - Main framework
- **PostgreSQL** - Database
- **Entity Framework Core** - ORM
- **Docker** - Containerization
- **xUnit + FluentAssertions + Moq** - Testing
- **Swagger/OpenAPI** - API documentation

## Project Structure

```
src/
├── API/           # Controllers, DTOs, Configuration
├── Core/          # Entities, Services, Interfaces
└── Infrastructure/ # Data Access, Repositories

tests/
└── src.UnitTests/ # Unit Tests with Builders
```

## Features

- **Complete CRUD** for Books, Customers, Reservations
- **Automatic input validation** with Data Annotations
- **Health checks** for database
- **Structured logging**
- **Background service** for expired reservations management
- **Clean Architecture** with layer separation
- **Complete unit tests** with 90%+ coverage

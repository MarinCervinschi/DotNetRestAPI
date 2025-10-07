# DotNetRestAPI

## Database Commands

### Install EF Core Tools (if needed)
```bash
dotnet tool install --global dotnet-ef
```

### Migration Commands
```bash
# Create a new migration
dotnet ef migrations add <MigrationName>

# Create migration in specific directory
dotnet ef migrations add <MigrationName> --output-dir Infrastructure/Data/Migrations

# Example: create initial migration
dotnet ef migrations add InitialCreate

# Apply migrations to database
dotnet ef database update

# Apply a specific migration
dotnet ef database update <MigrationName>

# Remove the last migration (only if not yet applied)
dotnet ef migrations remove

# View migration status
dotnet ef migrations list

# Generate SQL script for migrations
dotnet ef migrations script

# Generate SQL script from specific migration
dotnet ef migrations script <FromMigration> <ToMigration>
```

### Custom Migration Directory
```bash
dotnet ef migrations add InitialCreate --output-dir Infrastructure/Data/Migrations
```

### Database Commands
```bash
# Create database (if it doesn't exist)
dotnet ef database update

# Drop database
dotnet ef database drop

# View database information
dotnet ef dbcontext info

# Generate model from existing database (reverse engineering)
dotnet ef dbcontext scaffold "ConnectionString" Npgsql.EntityFrameworkCore.PostgreSQL
```

## Project Structure

### 1. Core/ (Domain Layer)
```text
Core/
├── Entities/
│   ├── Book.cs           ← Book domain entity
│   ├── Customer.cs       ← Customer domain entity  
│   ├── Reservation.cs    ← Reservation domain entity
│   └── Common/
│       └── BaseEntity.cs ← Base entity with Id, timestamps
├── Interfaces/
│   ├── Repositories/
│   │   ├── IBookRepository.cs
│   │   ├── ICustomerRepository.cs  
│   │   └── IReservationRepository.cs
│   └── Services/
│       ├── IBookService.cs
│       ├── ICustomerService.cs
│       └── IReservationService.cs
└── Services/
    ├── BookService.cs    ← Book business logic
    ├── CustomerService.cs ← Customer business logic
    └── ReservationService.cs ← Reservation business logic
```

### 2. Infrastructure/ (Data Access Layer)
```text
Infrastructure/
├── Data/
│   ├── ApplicationDbContext.cs ← EF Context
│   ├── Configurations/
│   │   ├── BookConfiguration.cs
│   │   ├── CustomerConfiguration.cs
│   │   └── ReservationConfiguration.cs
│   └── Migrations/
├── Repositories/
│   ├── BookRepository.cs
│   ├── CustomerRepository.cs
│   ├── ReservationRepository.cs  
│   └── Common/
│       └── BaseRepository.cs
└── External/
    └── EmailService.cs   ← External services (email, etc.)
```

### 3. API/ (Presentation Layer)
```text
API/
├── Controllers/
│   ├── BooksController.cs
│   ├── CustomersController.cs
│   └── ReservationsController.cs
├── DTOs/
│   ├── Book/
│   │   ├── BookDto.cs
│   │   ├── CreateBookDto.cs
│   │   └── UpdateBookDto.cs
│   ├── Customer/
│   │   ├── CustomerDto.cs
│   │   ├── CreateCustomerDto.cs
│   │   └── UpdateCustomerDto.cs
│   └── Reservation/
│       ├── ReservationDto.cs
│       ├── CreateReservationDto.cs
│       └── UpdateReservationDto.cs
├── Configuration/
│   ├── DependencyInjection.cs
│   └── DatabaseConfig.cs
└── Middleware/
    └── ExceptionMiddleware.cs
```
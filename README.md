# DotNetRestAPI

## Structure of the project

### 1. Core/ (Domain Layer)
```text
Core/
├── Entities/
│   ├── Book.cs           ← Entità dominio libro
│   ├── Customer.cs       ← Entità dominio cliente  
│   ├── Reservation.cs    ← Entità dominio prenotazione
│   └── Common/
│       └── BaseEntity.cs ← Entità base con Id, timestamps
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
    ├── BookService.cs    ← Logica business libri
    ├── CustomerService.cs ← Logica business clienti
    └── ReservationService.cs ← Logica business prenotazioni
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
    └── EmailService.cs   ← Servizi esterni (email, etc.)
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
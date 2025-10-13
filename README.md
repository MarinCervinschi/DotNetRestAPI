# DotNetRestAPI

[![.NET](https://img.shields.io/badge/.NET-9.0-512BD4?style=flat&logo=dotnet)](https://dotnet.microsoft.com/)
[![C#](https://img.shields.io/badge/C%23-12.0-239120?style=flat&logo=csharp)](https://docs.microsoft.com/en-us/dotnet/csharp/)
[![ASP.NET Core](https://img.shields.io/badge/ASP.NET%20Core-9.0-512BD4?style=flat&logo=dotnet)](https://docs.microsoft.com/en-us/aspnet/core/)
[![PostgreSQL](https://img.shields.io/badge/PostgreSQL-16-336791?style=flat&logo=postgresql&logoColor=white)](https://www.postgresql.org/)
[![Entity Framework](https://img.shields.io/badge/Entity%20Framework-9.0-512BD4?style=flat&logo=microsoft)](https://docs.microsoft.com/en-us/ef/)
[![Docker](https://img.shields.io/badge/Docker-Compose-2496ED?style=flat&logo=docker&logoColor=white)](https://www.docker.com/)
[![JWT](https://img.shields.io/badge/JWT-Authentication-000000?style=flat&logo=jsonwebtokens)](https://jwt.io/)
[![Swagger](https://img.shields.io/badge/Swagger-OpenAPI-85EA2D?style=flat&logo=swagger&logoColor=black)](https://swagger.io/)
[![xUnit](https://img.shields.io/badge/Tests-xUnit-512BD4?style=flat&logo=dotnet)](https://xunit.net/)
[![License](https://img.shields.io/badge/License-MIT-green?style=flat)](LICENSE)

REST API for library management with .NET 9, PostgreSQL and JWT Authentication.

## Features

- 📚 **Book Management** - CRUD operations for books (public endpoints)
- 👥 **Customer Management** - CRUD operations for customers (protected)
- 📋 **Reservation Management** - Book reservation system (protected)
- 🔐 **JWT Authentication** - Admin authentication with token-based security
- 🌱 **Database Seeding** - On-demand data population for development
- 🛡️ **Custom Middleware** - Request logging, error handling, JWT validation
- 📊 **Health Checks** - Database connectivity monitoring
- 📖 **Swagger/OpenAPI** - Interactive API documentation

## Authentication

The API uses JWT (JSON Web Tokens) for authentication:

- **Public Endpoints**: `/books/*` - No authentication required
- **Protected Endpoints**: `/customers/*`, `/reservations/*`, `/admin/*` - JWT token required
- **Auth Endpoint**: `/auth/login` - Get JWT token

### Default Admin Accounts (after seeding)

- **Username**: `admin` / **Password**: `admin123`
- **Username**: `superadmin` / **Password**: `super123`

## Main Commands

### Setup & Launch

```bash
# Install EF Core Tools
dotnet tool install --global dotnet-ef

# Start PostgreSQL database
docker-compose up -d postgres

# Start API (creates database automatically)
dotnet run --project src
```

### Database Management

To use these commands ensure you are in the `src` directory:

```bash
cd src
```

#### Migration Commands

```bash
# Create new migration
dotnet ef migrations add <MigrationName>

# Apply migrations
dotnet ef database update

# Remove last migration (if not applied)
dotnet ef migrations remove

# Reset database
dotnet ef database drop && dotnet ef database update
```

#### Database Seeding

The project includes a sophisticated seeding system for development:

```bash
# Populate database with sample data (runs migrations + seeding then exits)
dotnet run -- --seed

# Normal app startup (no seeding)
dotnet run
```

**What gets seeded:**
- 👑 **2 Admin accounts** with hashed passwords
- 📚 **5 Sample books** (Clean Code, Design Patterns, etc.)
- 👥 **3 Sample customers** (Mario Rossi, Luigi Bianchi, Anna Verdi)

The seeding is **idempotent** - you can run it multiple times safely.

## API Usage

### Authentication Flow

1. **Login to get JWT token:**
```bash
curl -X POST http://localhost:5000/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "username": "admin",
    "password": "admin123"
  }'
```

2. **Use token for protected endpoints:**
```bash
curl -X GET http://localhost:5000/customers \
  -H "Authorization: Bearer YOUR_JWT_TOKEN"
```

### Endpoint Overview

#### Public Endpoints
- `GET /books` - List all books
- `GET /books/{id}` - Get specific book
- `POST /auth/login` - Admin login

#### Protected Endpoints (JWT Required)
- `GET /customers` - List all customers
- `POST /customers` - Create customer (admin only)
- `PUT /customers/{id}` - Update customer (admin only)
- `DELETE /customers/{id}` - Delete customer (admin only)
- `GET /reservations` - List all reservations
- `POST /reservations` - Create reservation
- `DELETE /reservations/{id}` - Delete reservation

#### Admin Management (JWT Required)
- `GET /admin/{username}` - Get admin info
- `POST /admin` - Create new admin

## Docker Setup

### Quick Start with Deployment Script

Use the automated deployment script for easy management:

```bash
# Interactive mode (recommended)
./scripts/deploy.sh

# Direct commands
./scripts/deploy.sh 2    # Start services
./scripts/deploy.sh 4    # Restart services
./scripts/deploy.sh 7    # Check status
```

### Manual Docker Commands

```bash
# Setup environment
cp .env.example .env
# Edit .env with your database credentials

# Start all services
docker compose up --build

# Background mode
docker compose up -d

# View logs
docker compose logs -f dotnetrestapi

# Stop services
docker compose down
```

### Access Points

- **API**: http://localhost:5131
- **Swagger UI**: http://localhost:5131/swagger
- **Health Check**: http://localhost:5131/health

### Deploy Script Options

The `deploy.sh` script provides:

1. **Build application** - Rebuild Docker image
2. **Start services** - Launch API + PostgreSQL with health checks
3. **Stop services** - Graceful shutdown
4. **Restart services** - Full rebuild and restart
5. **View logs** - Follow real-time logs
6. **Complete reset** - Remove all data (⚠️ destructive)
7. **Services status** - Check container health

**Prerequisites**: Docker Desktop installed and running.

## Development

### Project Structure

```
src/
├── API/
│   ├── Controllers/        # REST controllers
│   ├── DTOs/              # Data transfer objects
│   ├── Configuration/     # App configuration
│   └── Middleware/        # Custom middleware
├── Core/
│   ├── Entities/          # Domain models
│   ├── Interfaces/        # Service/repository contracts
│   └── Services/          # Business logic
├── Infrastructure/
│   ├── Data/              # EF Core context & configurations
│   │   └── Seeding/       # Database seeding system
│   └── Repositories/      # Data access layer
└── HttpTests/             # HTTP test files
```

### Custom Middleware

The API includes several custom middleware for enhanced functionality:

- **Global Exception Handling** - Catches all errors and returns consistent JSON responses
- **Request Logging** - Logs all HTTP requests with timing information
- **JWT Validation** - Additional JWT token logging and validation
- **Route-Specific Middleware** - Custom logic for specific API routes

### Configuration

JWT settings in `appsettings.json`:

```json
{
  "Jwt": {
    "Key": "your-secret-key-here-minimum-256-bits-long",
    "Issuer": "DotNetRestAPI",
    "Audience": "DotNetRestAPI-Users",
    "ExpiryInMinutes": 60
  }
}
```

### Testing

The project implements comprehensive testing with both **Unit Tests** and **Integration Tests**.

**Test Structure:**
- `tests/src.UnitTests/` - Fast, isolated component tests
- `tests/src.IntegrationTests/` - End-to-end API tests

**Run Tests:**
```bash
# All tests
dotnet test

# Unit tests only
dotnet test tests/src.UnitTests

# Integration tests only
dotnet test tests/src.IntegrationTests

# With coverage
dotnet test --collect:"XPlat Code Coverage"
```

**HTTP Testing:**
Use the provided HTTP test files in `/HttpTests/`:
- `auth.http` - Authentication endpoints
- `books.http` - Book management
- `customers.http` - Customer management
- `reservations.http` - Reservation management

For detailed test documentation, see [tests/README.md](tests/README.md).

## Health Checks

Monitor API health at:
- `/health` - Overall health status
- `/health/db` - Database connectivity
- `/health/api` - API responsiveness

## Documentation

- **Swagger UI**: Available at `/swagger` in development mode
- **OpenAPI**: Available at `/openapi/v1.json`

## Technologies

- **.NET 9** - Latest .NET framework
- **ASP.NET Core** - Web API framework
- **Entity Framework Core** - ORM for PostgreSQL
- **PostgreSQL** - Primary database
- **JWT Bearer** - Authentication
- **BCrypt** - Password hashing
- **Swagger/OpenAPI** - API documentation
- **Docker Compose** - Database containerization

# Test Documentation

## Overview
This project implements a comprehensive testing strategy using **Unit Tests** and **Integration Tests** to ensure code quality and functionality.

## Test Structure
```
tests/
├── src.UnitTests/           # Fast, isolated tests
│   ├── API/Controllers/     # Controller unit tests
│   └── Core/
│       ├── Builders/        # Test data builders
│       └── Services/        # Service layer tests
└── src.IntegrationTests/    # End-to-end API tests
    ├── API/                 # Controller integration tests
    ├── Base/                # Test base classes
    ├── Helpers/             # Test utilities
    ├── Infrastructure/      # Database test setup
    └── Repositories/        # Repository integration tests
```

## Testing Libraries
- **xUnit**: Testing framework
- **FluentAssertions**: Readable assertions
- **Moq**: Mocking framework
- **AutoFixture**: Test data generation
- **Microsoft.AspNetCore.Mvc.Testing**: Integration testing
- **EntityFrameworkCore.InMemory**: In-memory database

## Unit Tests
**Purpose**: Test individual components in isolation

**Coverage**:
- Controllers (AdminController, AuthController, BooksController, CustomersController, ReservationsController)
- Services (business logic validation)
- Test builders for consistent test data

**Characteristics**:
- Fast execution
- No external dependencies
- Mocked dependencies
- Focused on single responsibility

## Integration Tests
**Purpose**: Test complete API workflows end-to-end

**Coverage**:
- Full HTTP request/response cycles
- Authentication flows
- Database operations
- Controller integration with services

**Characteristics**:
- Real HTTP calls via TestServer
- In-memory database
- Complete application pipeline
- Real dependency injection

## Running Tests

### All Tests
```bash
dotnet test
```

### Unit Tests Only
```bash
dotnet test tests/src.UnitTests
```

### Integration Tests Only
```bash
dotnet test tests/src.IntegrationTests
```

### With Coverage
```bash
dotnet test --collect:"XPlat Code Coverage"
```

## Test Guidelines
- **Arrange-Act-Assert** pattern
- **One assertion per test** (where possible)
- **Descriptive test names** explaining scenario
- **Use builders** for consistent test data
- **Mock external dependencies** in unit tests
- **Test both success and failure scenarios**

## Example Test Structure
```csharp
[Fact]
public async Task GetUser_WithValidId_ReturnsUser()
{
    // Arrange
    var userId = 1;
    var expectedUser = UserBuilder.Default().WithId(userId).Build();

    // Act
    var result = await _controller.GetUser(userId);

    // Assert
    result.Should().BeOfType<OkObjectResult>();
}
```

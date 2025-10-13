# .NET API Style Guide

## Introduction

This style guide outlines the coding conventions for .NET API applications developed at our organization. It's based on
Microsoft's official C# coding conventions and .NET API guidelines, with specific adaptations for our development
practices.

## Key Principles

* **Readability:** Code should be clear and self-explanatory.
* **Maintainability:** Code should be easy to modify and extend.
* **Consistency:** Follow consistent patterns across all API projects.
* **Focus:** Only implement what is required - avoid adding unnecessary features or code.

## Core Rules

### Stay on Task

* **Implement only what is requested:** Do not add features, endpoints, or functionality that weren't asked for.
* **No speculative code:** Don't add "nice-to-have" features or prepare for future requirements.
* **Follow the specification:** If requirements are unclear, ask for clarification rather than making assumptions.

### Naming Conventions

* **Classes:** PascalCase - `UserController`, `OrderService`, `ProductRepository`
* **Methods:** PascalCase - `GetUserById()`, `CreateOrder()`, `ValidateInput()`
* **Properties:** PascalCase - `UserId`, `TotalAmount`, `CreatedAt`
* **Private fields:** camelCase with underscore prefix - `_dbContext`, `_logger`, `_configuration`
* **Parameters:** camelCase - `userId`, `orderRequest`, `cancellationToken`
* **Local variables:** camelCase - `result`, `isValid`, `orderList`
* **Interfaces:** PascalCase with "I" prefix - `IUserService`, `IOrderRepository`
* **Constants:** PascalCase - `MaxPageSize`, `DefaultTimeout`

### File Organization

* **One class per file:** Each file should contain a single public class.
* **File name matches class name:** `UserController.cs` contains `UserController` class.
* **Namespace structure:** Match folder structure - `CompanyName.ProjectName.Controllers`

### API Structure

```
src/
├── API/
│   ├── Configuration/     # Dependency Injection, Swagger, JWT, etc.
│   ├── Controllers/       # API Controllers (thin layer)
│   ├── DTOs/             # Data Transfer Objects (Requests/Responses)
│   └── Middleware/       # Custom middleware (logging, exception handling)
├── Core/
│   ├── Entities/         # Domain entities/models
│   ├── Interfaces/       # Repository and service interfaces
│   └── Services/         # Business logic services
├── Infrastructure/
│   ├── Data/            # DbContext, configurations
│   └── Repositories/    # Data access implementations
├── HttpTests/           # HTTP request files for testing
├── Properties/          # Launch settings
├── Program.cs          # Application entry point
└── appsettings*.json   # Configuration files
```

**Layer responsibilities:**

* **API Layer:** Controllers, DTOs, Configuration, Middleware
* **Core Layer:** Business logic, entities, interfaces (domain-driven)
* **Infrastructure Layer:** Data access, external services, implementations

### Controller Guidelines

* **Keep controllers thin:** Business logic belongs in services, not controllers.
* **Use proper HTTP verbs:** GET, POST, PUT, DELETE, PATCH
* **Return appropriate status codes:** 200, 201, 204, 400, 404, 500
* **Use action result types:** `ActionResult<T>`, `IActionResult`

Example:

```csharp
[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly ILogger<UsersController> _logger;

    public UsersController(IUserService userService, ILogger<UsersController> logger)
    {
        _userService = userService;
        _logger = logger;
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserResponse>> GetUser(int id)
    {
        var user = await _userService.GetByIdAsync(id);

        if (user == null)
        {
            return NotFound();
        }

        return Ok(user);
    }
}
```

### Dependency Injection

* **Constructor injection:** Always use constructor injection for dependencies.
* **Register in Program.cs:** Use appropriate service lifetimes (Scoped, Singleton, Transient).
* **Avoid service locator pattern:** Don't inject `IServiceProvider` to resolve services manually.

### Comments

* **Comment only when necessary:** Code should be self-explanatory through clear naming.
* **Explain "why", not "what":** Don't describe what the code does if it's obvious.
* **Document non-obvious logic:** Complex algorithms, workarounds, or business rules need explanation.
* **Remove commented-out code:** Delete it instead of leaving it in the codebase.

**Good comment:**

```csharp
// Using exponential backoff to handle transient database errors
await RetryAsync(() => _dbContext.SaveChangesAsync());
```

**Bad comment:**

```csharp
// Get user by id
var user = await _userService.GetByIdAsync(id);
```

### XML Documentation

* **Document public APIs:** Use XML comments for controllers and public service methods.
* **Keep it concise:** Don't write lengthy documentation for simple operations.

```csharp
/// <summary>
/// Retrieves a user by their unique identifier.
/// </summary>
/// <param name="id">The user ID.</param>
/// <returns>The user details or 404 if not found.</returns>
[HttpGet("{id}")]
public async Task<ActionResult<UserResponse>> GetUser(int id)
{
    // implementation
}
```

### Error Handling

* **Use specific exceptions:** Don't catch generic `Exception` unless at the top level.
* **Implement global exception handling:** Use middleware for centralized error handling.
* **Log exceptions properly:** Include context and relevant data.
* **Don't expose sensitive information:** Return generic error messages to clients.

### Async/Await

* **Use async all the way:** Don't block on async operations with `.Result` or `.Wait()`.
* **Pass CancellationToken:** Include `CancellationToken` parameters in async methods.
* **Avoid async void:** Only use for event handlers.

### Validation

* **Validate input:** Use model validation attributes or FluentValidation.
* **Validate at the edge:** Validate in controllers before passing to services.
* **Return validation errors:** Use `BadRequest()` with ModelState errors.

```csharp
[HttpPost]
public async Task<ActionResult<OrderResponse>> CreateOrder([FromBody] CreateOrderRequest request)
{
    if (!ModelState.IsValid)
    {
        return BadRequest(ModelState);
    }

    var result = await _orderService.CreateAsync(request);
    return CreatedAtAction(nameof(GetOrder), new { id = result.Id }, result);
}
```

### Logging

* **Use structured logging:** Use ILogger with structured data.
* **Log at appropriate levels:** Debug, Information, Warning, Error, Critical.
* **Include correlation IDs:** For tracking requests across services.
* **Don't log sensitive data:** Avoid logging passwords, tokens, or personal information.

```csharp
_logger.LogInformation("User {UserId} created order {OrderId}", userId, orderId);
_logger.LogError(ex, "Failed to process payment for order {OrderId}", orderId);
```

### Configuration

* **Use appsettings.json:** Store configuration in JSON files.
* **Use IOptions pattern:** Strongly-typed configuration objects.
* **Separate by environment:** appsettings.Development.json, appsettings.Production.json
* **Never commit secrets:** Use user secrets for development, Azure Key Vault for production.

### Code Formatting

* **Line length:** Maximum 120 characters.
* **Indentation:** 4 spaces (not tabs).
* **Braces:** Opening brace on same line (K&R style).
* **Use EditorConfig:** Enforce formatting rules automatically.

## Tooling

* **IDE:** Visual Studio or JetBrains Rider
* **Code formatter:** Built-in formatter with EditorConfig
* **Analyzer:** Enable .NET analyzers and treat warnings as errors
* **API testing:** Swagger/OpenAPI for documentation and testing

## What NOT to Do

* ❌ Don't add features that weren't requested
* ❌ Don't leave TODO comments - either do it or create a task
* ❌ Don't comment obvious code
* ❌ Don't catch and ignore exceptions
* ❌ Don't use magic numbers or strings - define constants
* ❌ Don't make assumptions about requirements
* ❌ Don't over-engineer solutions

## Summary

Write clear, focused code that does exactly what's needed. If the code is self-explanatory, skip the comment. If you're
tempted to add extra features, don't. Follow the requirements, keep it simple, and maintain consistency.

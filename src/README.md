# DotNetRestAPI

## Development Guidelines

This project follows the coding standards defined in [StyleGuide.md](.agent/styleGuide.md).

### Key Reminders:
- ✅ Implement only what is requested
- ✅ Use PascalCase for classes, methods, properties
- ✅ Use camelCase with underscore prefix for private fields
- ✅ Comment only when necessary (explain "why", not "what")
- ✅ Use async/await properly with CancellationToken
- ✅ Validate input at controller level
- ✅ Use structured logging
- ✅ Maximum 120 characters per line

### Before committing:
1. Run `dotnet format` to ensure code formatting
2. Check that all tests pass
3. Verify no TODO comments remain
4. Ensure no magic numbers/strings exist

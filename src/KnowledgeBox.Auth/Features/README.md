# MediatR Pattern Implementation

This project uses the MediatR library to implement the Mediator pattern, which helps to decouple controllers from services and provides a clean way to implement the CQRS (Command Query Responsibility Segregation) pattern.

## Key Benefits

1. **Decoupling**: Controllers no longer directly depend on services, reducing tight coupling between layers
2. **Single Responsibility**: Each handler has a single responsibility - handling a specific command or query
3. **Extensibility**: Easy to add cross-cutting concerns via pipeline behaviors
4. **Testability**: Easy to test individual handlers in isolation

## Structure

The MediatR implementation is organized into features:

```
Features/
  ├── Authentication/
  │   ├── Commands/
  │   │   ├── SignupCommand.cs
  │   │   ├── SignupCommandHandler.cs
  │   ├── Queries/
  │   │   ├── LoginQuery.cs
  │   │   ├── LoginQueryHandler.cs
  ├── User/
  │   ├── Queries/
  │   │   ├── GetCurrentUserQuery.cs
  │   │   ├── GetCurrentUserQueryHandler.cs
```

## Using MediatR

### Sending Commands (Write Operations)

```csharp
// In a controller
var command = new SignupCommand(username, email, password, firstName, lastName);
var result = await _mediator.Send(command);
```

### Sending Queries (Read Operations)

```csharp
// In a controller
var query = new LoginQuery(username, password);
var result = await _mediator.Send(query);
```

### Creating Commands

Commands represent write operations and should be named with a verb in imperative form:

```csharp
public record SignupCommand(
    string Username,
    string Email,
    string Password,
    string FirstName,
    string LastName) : IRequest<UserSignupResponse>;
```

### Creating Queries

Queries represent read operations and should be named with a noun or verb in interrogative form:

```csharp
public record LoginQuery(string Username, string Password) : IRequest<LoginResponse>;
```

### Creating Handlers

Each command or query must have exactly one handler:

```csharp
public class SignupCommandHandler : IRequestHandler<SignupCommand, UserSignupResponse>
{
    // Dependencies injected via constructor
    
    public async Task<UserSignupResponse> Handle(SignupCommand request, CancellationToken cancellationToken)
    {
        // Implementation
    }
}
```

## Pipeline Behaviors

MediatR allows you to add behaviors that are executed for all commands and queries, which are useful for cross-cutting concerns like:

- Logging
- Validation
- Exception handling
- Performance monitoring
- Caching
- Transaction management

See `Infrastructure/MediatR/LoggingBehavior.cs` for an example implementation. 
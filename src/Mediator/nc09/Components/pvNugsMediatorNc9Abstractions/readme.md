# pvNugsMediatorNc9Abstractions

🎯 A lightweight, flexible mediator pattern implementation for .NET 9 that decouples application components through request/response and publish/subscribe messaging patterns.

## Features

✨ **Request/Response Pattern**: Send commands and queries to single handlers with typed responses  
📢 **Publish/Subscribe Pattern**: Broadcast notifications to multiple handlers concurrently  
🔄 **Pipeline Behaviors**: Add cross-cutting concerns like logging, validation, and caching  
🎭 **Unit Type Support**: Handle void-like operations with type-safe `Unit` return type  
🔌 **Dependency Injection Ready**: Seamlessly integrates with Microsoft.Extensions.DependencyInjection  
📝 **Fully Documented**: Comprehensive XML documentation with IntelliSense support  
🧪 **Testable**: Design promotes clean architecture and testability  
⚡ **Async First**: Built for modern asynchronous programming patterns  
🔁 **Backward Compatible**: PvNugs interfaces extend base interfaces for maximum flexibility  
🔍 **Handler Introspection**: Discover and validate registered handlers at runtime (PvNugs exclusive)

## Architecture & Backward Compatibility

This package provides **two layers of interfaces** for maximum flexibility:

### Base Interfaces (`Mediator` namespace)
Framework-agnostic interfaces that define the core mediator pattern:
- `IMediator`, `IRequest<TResponse>`, `IRequestHandler<TRequest, TResponse>`
- `INotification`, `INotificationHandler<TNotification>`
- `IPipelineBehavior<TRequest, TResponse>`

### PvNugs Interfaces (`pvNugs` namespace)
Branded interfaces that extend the base interfaces:
- `IPvNugsMediator : IMediator`
- `IPvNugsMediatorRequest<TResponse> : IRequest<TResponse>`
- `IPvNugsMediatorRequestHandler<TRequest, TResponse> : IRequestHandler<TRequest, TResponse>`
- And all other PvNugs-branded equivalents

### Why This Dual Design?

**Backward Compatibility** 🔁
```csharp
// Register a PvNugs implementation
services.AddScoped<IPvNugsMediator, PvNugsMediatorImplementation>();

// Can be injected as IPvNugsMediator
public class NewService(IPvNugsMediator mediator) { }

// Or as IMediator for backward compatibility with existing code
public class LegacyService(IMediator mediator) { } // Same implementation works!
```

**Flexibility** 🎯
- Use **PvNugs interfaces** in your new code for branding
- Use **base interfaces** for library-agnostic code
- Mix and match as needed - they're fully compatible!

**PvNugs Exclusive Features** 🌟
```csharp
// IPvNugsMediator adds value beyond IMediator
var pvNugsMediator = app.Services.GetRequiredService<IPvNugsMediator>();

// Handler introspection (not available in base IMediator)
var handlers = pvNugsMediator.GetRegisteredHandlers();
foreach (var handler in handlers)
{
    Console.WriteLine($"{handler.RegistrationType}: {handler.ImplementationType.Name}");
}
```

**Type Safety** ✅
```csharp
// PvNugs pipeline only accepts PvNugs requests (stronger typing)
public class MyPipeline<TRequest, TResponse> 
    : IPvNugsMediatorPipelineRequestHandler<TRequest, TResponse>
    where TRequest : IPvNugsMediatorRequest<TResponse> // PvNugs constraint
{ }

// Base pipeline accepts any request
public class GenericPipeline<TRequest, TResponse> 
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse> // Base constraint
{ }
```

## Installation

Install via NuGet Package Manager:
```shell
Install-Package pvNugsMediatorNc9Abstractions
```

Or via .NET CLI:
```shell
dotnet add package pvNugsMediatorNc9Abstractions
```

## Core Components

### 🎯 Mediator Interfaces

- **`IMediator`**: Base mediator interface for routing requests and publishing notifications
- **`IPvNugsMediator`**: PvNugs-branded mediator that extends `IMediator` for backward compatibility

### 📨 Request/Response

**Base Interfaces:**
- **`IRequest<TResponse>`**: Base marker interface for requests expecting a response
- **`IRequest`**: Base convenience interface for requests returning `Unit` (void-like)
- **`IRequestHandler<TRequest, TResponse>`**: Base handler for processing requests
- **`IRequestHandler<TRequest>`**: Base handler for void-like requests

**PvNugs Interfaces:**
- **`IPvNugsMediatorRequest<TResponse>`**: PvNugs-branded request interface
- **`IPvNugsMediatorRequest`**: PvNugs-branded void-like request interface
- **`IPvNugsMediatorRequestHandler<TRequest, TResponse>`**: PvNugs-branded request handler
- **`IPvNugsMediatorRequestHandler<TRequest>`**: PvNugs-branded void-like request handler

### 📢 Publish/Subscribe

**Base Interfaces:**
- **`INotification`**: Base marker interface for notifications
- **`INotificationHandler<TNotification>`**: Base handler for processing notifications

**PvNugs Interfaces:**
- **`IPvNugsMediatorNotification`**: PvNugs-branded notification interface
- **`IPvNugsNotificationHandler<TNotification>`**: PvNugs-branded notification handler

### 🔄 Pipeline Behaviors

**Base Interfaces:**
- **`IPipelineBehavior<TRequest, TResponse>`**: Base interface for pipeline behaviors
- **`RequestHandlerDelegate<TResponse>`**: Delegate for invoking the next handler in the pipeline

**PvNugs Interfaces:**
- **`IPvNugsMediatorPipelineRequestHandler<TRequest, TResponse>`**: PvNugs-branded pipeline behavior

### 🎭 Unit Type

- **`Unit`**: Represents a void-like return type for requests that don't return meaningful data

## Quick Start

### 1️⃣ Request/Response Pattern

**Define a Query Request**
```csharp
using pvNugsMediatorNc9Abstractions;

public class GetUserByIdRequest : IPvNugsMediatorRequest<User>
{
    public int UserId { get; init; }
}

public class GetUserByIdHandler : IPvNugsMediatorRequestHandler<GetUserByIdRequest, User>
{
    private readonly IUserRepository _userRepository;
    
    public GetUserByIdHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }
    
    public async Task<User> HandleAsync(
        GetUserByIdRequest request, 
        CancellationToken cancellationToken)
    {
        return await _userRepository.GetByIdAsync(request.UserId, cancellationToken);
    }
}
```

**Define a Command Request (returns Unit)**
```csharp
public class DeleteUserRequest : IPvNugsMediatorRequest
{
    public int UserId { get; init; }
}

public class DeleteUserHandler : IPvNugsMediatorRequestHandler<DeleteUserRequest>
{
    private readonly IUserRepository _userRepository;
    
    public DeleteUserHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }
    
    public async Task<Unit> HandleAsync(
        DeleteUserRequest request, 
        CancellationToken cancellationToken)
    {
        await _userRepository.DeleteAsync(request.UserId, cancellationToken);
        return Unit.Value;
    }
}
```

### 2️⃣ Publish/Subscribe Pattern

**Define a Notification**
```csharp
public class UserCreatedNotification : IPvNugsMediatorNotification
{
    public int UserId { get; init; }
    public string Email { get; init; }
}

// First Handler - Send Welcome Email
public class SendWelcomeEmailHandler : IPvNugsNotificationHandler<UserCreatedNotification>
{
    private readonly IEmailService _emailService;
    
    public SendWelcomeEmailHandler(IEmailService emailService)
    {
        _emailService = emailService;
    }
    
    public async Task HandleAsync(
        UserCreatedNotification notification, 
        CancellationToken cancellationToken)
    {
        await _emailService.SendWelcomeEmailAsync(notification.Email, cancellationToken);
    }
}

// Second Handler - Log Event
public class LogUserCreationHandler : IPvNugsNotificationHandler<UserCreatedNotification>
{
    private readonly ILogger _logger;
    
    public LogUserCreationHandler(ILogger logger)
    {
        _logger = logger;
    }
    
    public async Task HandleAsync(
        UserCreatedNotification notification, 
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("User {UserId} created", notification.UserId);
        await Task.CompletedTask;
    }
}
```

### 3️⃣ Pipeline Behaviors

**Logging Pipeline**
```csharp
public class LoggingPipeline<TRequest, TResponse> 
    : IPvNugsMediatorPipelineRequestHandler<TRequest, TResponse>
    where TRequest : IPvNugsMediatorRequest<TResponse>
{
    private readonly ILogger _logger;
    
    public LoggingPipeline(ILogger logger)
    {
        _logger = logger;
    }
    
    public async Task<TResponse> HandleAsync(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        _logger.LogInformation("Handling {RequestName}", requestName);
        
        var stopwatch = Stopwatch.StartNew();
        
        try
        {
            var response = await next();
            stopwatch.Stop();
            
            _logger.LogInformation(
                "Handled {RequestName} in {ElapsedMs}ms", 
                requestName, 
                stopwatch.ElapsedMilliseconds);
            
            return response;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, 
                "Error handling {RequestName} after {ElapsedMs}ms", 
                requestName, 
                stopwatch.ElapsedMilliseconds);
            throw;
        }
    }
}
```

**Validation Pipeline**
```csharp
public class ValidationPipeline<TRequest, TResponse> 
    : IPvNugsMediatorPipelineRequestHandler<TRequest, TResponse>
    where TRequest : IPvNugsMediatorRequest<TResponse>
{
    public async Task<TResponse> HandleAsync(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (request is IValidatable validatable)
        {
            var errors = validatable.Validate();
            if (errors.Any())
            {
                throw new ValidationException(errors);
            }
        }
        
        return await next();
    }
}
```

### 4️⃣ Dependency Injection Setup

```csharp
using Microsoft.Extensions.DependencyInjection;
using pvNugsMediatorNc9Abstractions;

var services = new ServiceCollection();

// Register handlers
services.AddTransient<IPvNugsMediatorRequestHandler<GetUserByIdRequest, User>, GetUserByIdHandler>();
services.AddTransient<IPvNugsMediatorRequestHandler<DeleteUserRequest>, DeleteUserHandler>();

// Register notification handlers (can have multiple for same notification)
services.AddTransient<IPvNugsNotificationHandler<UserCreatedNotification>, SendWelcomeEmailHandler>();
services.AddTransient<IPvNugsNotificationHandler<UserCreatedNotification>, LogUserCreationHandler>();

// Register pipeline behaviors (executed in order)
services.AddTransient<IPvNugsMediatorPipelineRequestHandler<GetUserByIdRequest, User>, LoggingPipeline<GetUserByIdRequest, User>>();
services.AddTransient<IPvNugsMediatorPipelineRequestHandler<GetUserByIdRequest, User>, ValidationPipeline<GetUserByIdRequest, User>>();

// Register the mediator implementation (from pvNugsMediatorNc9 package)
services.TryAddPvNugsMediator(); // Requires pvNugsMediatorNc9 package
```

### 5️⃣ Using the Mediator

```csharp
public class UserService
{
    private readonly IPvNugsMediator _mediator;
    
    public UserService(IPvNugsMediator mediator)
    {
        _mediator = mediator;
    }
    
    public async Task<User> GetUserAsync(int userId)
    {
        // Send query request and get response
        var user = await _mediator.SendAsync(new GetUserByIdRequest { UserId = userId });
        return user;
    }
    
    public async Task DeleteUserAsync(int userId)
    {
        // Send command request (returns Unit)
        await _mediator.SendAsync(new DeleteUserRequest { UserId = userId });
    }
    
    public async Task CreateUserAsync(User user)
    {
        // Save user...
        
        // Publish notification to all subscribers
        await _mediator.PublishAsync(new UserCreatedNotification 
        { 
            UserId = user.Id, 
            Email = user.Email 
        });
    }
}
```

### 6️⃣ Using Base Interfaces (Alternative)

You can also use base interfaces for framework-agnostic code:

```csharp
using pvNugsMediatorNc9Abstractions.Mediator; // Base interfaces

// Define using base interfaces
public class GetProductRequest : IRequest<Product>
{
    public int ProductId { get; init; }
}

public class GetProductHandler : IRequestHandler<GetProductRequest, Product>
{
    public async Task<Product> HandleAsync(
        GetProductRequest request, 
        CancellationToken cancellationToken)
    {
        // Implementation
        return new Product();
    }
}

// Inject using base interface
public class ProductService
{
    private readonly IMediator _mediator; // Base interface
    
    public ProductService(IMediator mediator)
    {
        _mediator = mediator;
    }
    
    public async Task<Product> GetProductAsync(int id)
    {
        return await _mediator.SendAsync(new GetProductRequest { ProductId = id });
    }
}
```

**Note**: Both approaches work with the same implementation - choose based on your preference!

## Design Patterns

### 🎯 CQRS (Command Query Responsibility Segregation)

The mediator pattern naturally supports CQRS by separating:
- **Commands**: Requests that modify state (often return `Unit`)
- **Queries**: Requests that read data (return typed results)

```csharp
// Command
public class CreateUserCommand : IPvNugsMediatorRequest<Guid>
{
    public string Username { get; init; }
    public string Email { get; init; }
}

// Query
public class GetUserQuery : IPvNugsMediatorRequest<User>
{
    public int UserId { get; init; }
}
```

### 🔄 Chain of Responsibility

Pipeline behaviors implement the chain of responsibility pattern:
```
Request → ValidationPipeline → LoggingPipeline → CachingPipeline → Handler → Response
```

### 📢 Observer Pattern

Notifications implement the observer pattern, allowing multiple handlers to react to events without tight coupling.

## Advanced Scenarios

### Polymorphic Notifications

```csharp
object notification = new UserCreatedNotification { UserId = 123, Email = "user@example.com" };
await _mediator.PublishAsync(notification); // Uses runtime type resolution
```

### Cancellation Support

```csharp
var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
var user = await _mediator.SendAsync(
    new GetUserByIdRequest { UserId = 123 }, 
    cts.Token);
```

### Generic Pipeline for All Requests

```csharp
// This pipeline will be registered for ALL request types
services.AddTransient(typeof(IPvNugsMediatorPipelineRequestHandler<,>), typeof(LoggingPipeline<,>));
```

### Handler Introspection (PvNugs Exclusive) 🔍

The `IPvNugsMediator` interface provides a powerful introspection feature to discover all registered handlers in the DI container:

```csharp
public class Startup
{
    public void Configure(IApplicationBuilder app)
    {
        var mediator = app.ApplicationServices.GetRequiredService<IPvNugsMediator>();
        
        // Get all registered handlers
        var registrations = mediator.GetRegisteredHandlers();
        
        Console.WriteLine($"\n📋 Registered Mediator Components ({registrations.Count()}):\n");
        
        // Group by type for better organization
        var grouped = registrations.GroupBy(r => r.RegistrationType);
        
        foreach (var group in grouped)
        {
            Console.WriteLine($"  {group.Key}: {group.Count()}");
            foreach (var reg in group.OrderBy(r => r.ImplementationType.Name))
            {
                Console.WriteLine($"    • {reg.ImplementationType.Name}");
                if (reg.MessageType != null)
                {
                    Console.WriteLine($"      Handles: {reg.MessageType.Name}");
                }
                if (reg.ResponseType != null)
                {
                    Console.WriteLine($"      Returns: {reg.ResponseType.Name}");
                }
                Console.WriteLine($"      Lifetime: {reg.Lifetime}");
            }
            Console.WriteLine();
        }
    }
}
```

**Use Cases:**

**1. Validation During Startup**
```csharp
// Ensure all expected handlers are registered
var mediator = services.BuildServiceProvider().GetRequiredService<IPvNugsMediator>();
var handlers = mediator.GetRegisteredHandlers();

var requiredHandlers = new[] { "GetUserByIdRequest", "CreateUserRequest", "DeleteUserRequest" };
var registeredRequests = handlers
    .Where(h => h.MessageType != null)
    .Select(h => h.MessageType!.Name)
    .ToHashSet();

foreach (var required in requiredHandlers)
{
    if (!registeredRequests.Contains(required))
    {
        throw new InvalidOperationException($"Required handler for {required} is not registered!");
    }
}
```

**2. Health Check Endpoint**
```csharp
app.MapGet("/health/mediator", (IPvNugsMediator mediator) =>
{
    var handlers = mediator.GetRegisteredHandlers();
    
    return Results.Ok(new
    {
        Status = "Healthy",
        TotalHandlers = handlers.Count(),
        RequestHandlers = handlers.Count(r => r.RegistrationType.Contains("Request")),
        NotificationHandlers = handlers.Count(r => r.RegistrationType.Contains("Notification")),
        PipelineBehaviors = handlers.Count(r => r.RegistrationType.Contains("Pipeline")),
        Details = handlers.Select(h => new
        {
            h.RegistrationType,
            Implementation = h.ImplementationType.Name,
            Message = h.MessageType?.Name,
            Response = h.ResponseType?.Name,
            h.Lifetime
        })
    });
});
```

**3. Generate Documentation**
```csharp
// Auto-generate markdown documentation of all handlers
var mediator = services.BuildServiceProvider().GetRequiredService<IPvNugsMediator>();
var handlers = mediator.GetRegisteredHandlers();

var markdown = new StringBuilder();
markdown.AppendLine("# Available Handlers\n");

var requestHandlers = handlers.Where(h => h.RegistrationType.Contains("Request"));
markdown.AppendLine("## Request Handlers\n");
foreach (var handler in requestHandlers.OrderBy(h => h.MessageType?.Name))
{
    markdown.AppendLine($"- **{handler.MessageType?.Name}**");
    markdown.AppendLine($"  - Handler: `{handler.ImplementationType.Name}`");
    markdown.AppendLine($"  - Returns: `{handler.ResponseType?.Name ?? "Unit"}`");
    markdown.AppendLine();
}

File.WriteAllText("handlers.md", markdown.ToString());
```

**4. Development Dashboard**
```csharp
#if DEBUG
// Show registered handlers in console during development
app.Lifetime.ApplicationStarted.Register(() =>
{
    var mediator = app.Services.GetRequiredService<IPvNugsMediator>();
    var handlers = mediator.GetRegisteredHandlers();
    
    Console.WriteLine("\n" + new string('=', 60));
    Console.WriteLine("  MEDIATOR CONFIGURATION");
    Console.WriteLine(new string('=', 60));
    
    foreach (var handler in handlers.OrderBy(h => h.RegistrationType).ThenBy(h => h.ImplementationType.Name))
    {
        Console.WriteLine(handler); // Uses ToString() for formatted output
    }
    
    Console.WriteLine(new string('=', 60) + "\n");
});
#endif
```

> **💡 Performance Note**: `GetRegisteredHandlers()` uses reflection and should be called during startup/diagnostics only, not in request hot paths.

## Available Implementations

This abstractions package provides the foundation for the mediator pattern. For a ready-to-use concrete implementation, use:

- **pvNugsMediatorNc9**: Full mediator implementation with pipeline support and DI integration

## Benefits

✅ **Reduced Coupling**: Components communicate through the mediator, not directly  
✅ **Single Responsibility**: Handlers focus on one specific task  
✅ **Open/Closed Principle**: Add new handlers without modifying existing code  
✅ **Testability**: Easy to mock the mediator for unit testing  
✅ **Cross-Cutting Concerns**: Pipeline behaviors handle logging, validation, etc.  
✅ **Clean Architecture**: Supports domain-driven design and clean architecture principles  

## Best Practices

1. **Keep Handlers Focused**: Each handler should do one thing well
2. **Use Pipelines for Cross-Cutting Concerns**: Don't repeat logging/validation in every handler
3. **Prefer `Unit` over `bool` for Commands**: More explicit than returning success flags
4. **Name Requests Clearly**: Use suffixes like `Request`, `Command`, `Query`, or `Notification`
5. **Validate Early**: Use validation pipelines to fail fast
6. **Handle Exceptions Gracefully**: Use error handling pipelines for consistent error responses

## Thread Safety

- The mediator is designed to be thread-safe when used with proper DI container scoping
- Notification handlers typically execute concurrently - ensure handlers are thread-safe if needed
- Pipeline behaviors execute sequentially in the order they are registered

## Performance Considerations

- Request handlers are resolved from DI on each request
- Notification handlers are resolved and invoked concurrently for better performance
- Pipeline overhead is minimal (typically < 1ms per pipeline)

## Compatibility

- **.NET Version**: .NET 9.0+
- **DI Container**: Microsoft.Extensions.DependencyInjection
- **C# Version**: C# 12.0+

## License

MIT License - see LICENSE file for details

## Author

Pierre Van Wallendael - pvWay Ltd

## Repository

[GitHub Repository](https://github.com/licheez/pvWayNugs.git)

---

💡 **Tip**: Combine with `pvNugsLoggerNc9Abstractions` for comprehensive logging in your handlers and pipelines!


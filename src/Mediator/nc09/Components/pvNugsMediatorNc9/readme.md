# pvNugsMediatorNc9

🚀 **Concrete implementation** of the mediator pattern for .NET 9 with built-in logging, pipeline behaviors, and dependency injection support.

## Overview

This package provides a ready-to-use implementation of `IPvNugsMediator` from the `pvNugsMediatorNc9Abstractions` package. It uses reflection and dependency injection to dynamically route requests to handlers and publish notifications to subscribers, with comprehensive logging throughout the pipeline.

**🎯 MediatR Compatible**: This implementation extends the standard `IMediator` interface and uses the same method naming conventions (`Send`, `Publish`) as MediatR, making it a drop-in replacement for existing MediatR-based applications while adding PvNugs-specific features like handler introspection and flexible discovery modes.

## 🔧 Critical Fix in v9.0.3

**IMPORTANT UPDATE**: If upgrading from v9.0.2 or earlier:

- **Mediator is now correctly registered as `Scoped`** (was incorrectly `Singleton`)
- This **fixes critical issues** with scoped dependencies in WebAPI applications
- **Why**: Handlers can now properly resolve `DbContext`, `HttpContext`, and other scoped services
- **Impact**: ✅ **No code changes required** - existing applications continue to work, now with correct scoping

**Migration**: Zero code changes needed! Just update the package version. Your application will immediately benefit from proper scoping behavior.

**What This Fixes**:
- ✅ Handlers can now safely inject and use DbContext
- ✅ Each HTTP request gets proper service isolation  
- ✅ Scoped dependencies resolve correctly
- ✅ Both `IPvNugsMediator` and `IMediator` resolve to the same instance per scope
- ✅ No more "Cannot resolve scoped service from singleton" errors

**Strongly recommended upgrade for all WebAPI applications!**

## ✨ New in v9.0.4 - Task-Returning Handlers!

**EXCITING NEW FEATURE**: Cleaner void-like command handlers!

- **No more `return Unit.Value;`** - Single-parameter handlers return `Task` instead of `Task<Unit>`
- **More natural C# code** - Just write `async Task` for commands that don't return data
- **100% Backward Compatible** - Existing handlers continue to work without changes
- **Both styles supported** - Use the new interfaces (`IRequestHandler<TRequest>`) or keep the old ones

**Before (still works):**
```csharp
public class DeleteUserHandler : IPvNugsMediatorRequestHandler<DeleteUserRequest, Unit>
{
    public async Task<Unit> HandleAsync(DeleteUserRequest request, CancellationToken ct)
    {
        await _repository.DeleteAsync(request.UserId, ct);
        return Unit.Value; // ← Boilerplate!
    }
}
```

**After (cleaner!):**
```csharp
public class DeleteUserHandler : IPvNugsMediatorRequestHandler<DeleteUserRequest>
{
    public async Task HandleAsync(DeleteUserRequest request, CancellationToken ct)
    {
        await _repository.DeleteAsync(request.UserId, ct);
        // ← No return needed!
    }
}
```

**Requires**: `pvNugsMediatorNc9Abstractions` v9.0.7+

## Features

✨ **Task-Returning Handlers**: NEW! Void-like handlers return `Task` instead of `Task<Unit>` - no more `Unit.Value`  
⚡ **Production-Ready**: Complete mediator implementation ready for immediate use  
🔄 **MediatR Compatible**: Drop-in replacement for MediatR with same method names (`Send`, `Publish`)  
🔍 **Built-in Logging**: Automatic logging of all request/notification handling operations  
🎯 **Dynamic Resolution**: Automatic handler discovery and invocation via dependency injection  
🔧 **Flexible Discovery Modes**: Choose between Manual, Decorated, or FullScan handler discovery  
📊 **Handler Introspection**: Built-in diagnostics via `GetRegisteredHandlers()` for debugging  
🔄 **Pipeline Support**: Full support for pipeline behaviors with proper chain execution  
📈 **Error Tracking**: Detailed exception handling with wrapped error context  
🧩 **Easy Setup**: Single extension method for DI registration  
🔒 **Thread-Safe**: Singleton registration with safe concurrent operation  
⏱️ **Performance Optimized**: Efficient handler resolution and pipeline execution  

## Installation

Install via NuGet Package Manager:
```shell
Install-Package pvNugsMediatorNc9
```

Or via .NET CLI:
```shell
dotnet add package pvNugsMediatorNc9
```

## Dependencies

This package requires:
- **pvNugsMediatorNc9Abstractions** (9.0.0+) - Mediator pattern interfaces
- **pvNugsLoggerNc9Abstractions** (9.1.3+) - Logging abstractions

## Quick Start

### 1️⃣ Register Services

```csharp
using Microsoft.Extensions.DependencyInjection;
using pvNugsMediatorNc9;
using pvNugsMediatorNc9Abstractions;
using pvNugsLoggerNc9Seri; // Or any other logger implementation

var services = new ServiceCollection();

// 1. Register a logger (REQUIRED)
services.TryAddPvNugsLoggerSeriService(config);

// 2. Choose your discovery mode and register the mediator

// Option A: Manual mode (Recommended for Production)
// Handlers must be registered explicitly
services.TryAddPvNugsMediator(DiscoveryMode.Manual);
services.AddTransient<
    IPvNugsMediatorRequestHandler<GetUserByIdRequest, User>, 
    GetUserByIdHandler>();
services.AddTransient<
    IPvNugsMediatorRequestHandler<DeleteUserRequest>, 
    DeleteUserHandler>();

// Option B: FullScan mode (Great for Development)
// Automatically discovers all handlers via reflection
services.TryAddPvNugsMediator(DiscoveryMode.FullScan);

// Option C: Decorated mode (Balanced Approach)
// Discovers handlers marked with [MediatorHandler] attribute
services.TryAddPvNugsMediator(DiscoveryMode.Decorated);

// Option D: Configuration-based (Flexible)
// Read settings from appsettings.json
services.TryAddPvNugsMediator(configuration.GetSection("PvNugsMediatorConfig"));

// 3. Register notification handlers (can have multiple per notification)
services.AddTransient<
    IPvNugsMediatorNotificationHandler<UserCreatedNotification>, 
    SendWelcomeEmailHandler>();

services.AddTransient<
    IPvNugsMediatorNotificationHandler<UserCreatedNotification>, 
    LogUserCreationHandler>();

// 4. Register pipeline behaviors (optional, for cross-cutting concerns)
services.AddTransient<
    IPvNugsMediatorPipelineRequestHandler<GetUserByIdRequest, User>, 
    LoggingPipeline<GetUserByIdRequest, User>>();


var serviceProvider = services.BuildServiceProvider();
```

### 2️⃣ Use the Mediator

```csharp
public class UserService
{
    private readonly IPvNugsMediator _mediator;
    private readonly ILoggerService _logger;
    
    public UserService(IPvNugsMediator mediator, ILoggerService logger)
    {
        _mediator = mediator;
        _logger = logger;
    }
    
    public async Task<User> GetUserAsync(int userId)
    {
        try
        {
            // Send request - pipelines and handler execute automatically
            // MediatR-compatible: Use Send() or SendAsync()
            var user = await _mediator.Send(
                new GetUserByIdRequest { UserId = userId });
            
            // Alternative: Explicit async version
            // var user = await _mediator.SendAsync(
            //     new GetUserByIdRequest { UserId = userId });
            
            return user;
        }
        catch (PvNugsMediatorException ex)
        {
            await _logger.LogAsync(ex, SeverityEnu.Error);
            throw;
        }
    }
    
    public async Task CreateUserAsync(User user)
    {
        // ... create user logic ...
        
        // Publish notification - all registered handlers execute
        // MediatR-compatible: Use Publish() or PublishAsync()
        await _mediator.Publish(
            new UserCreatedNotification 
            { 
                UserId = user.Id, 
                Email = user.Email 
            });
    }
    
    public async Task DiagnosticsAsync()
    {
        // PvNugs-specific: Get handler registration info
        var handlers = _mediator.GetRegisteredHandlers();
        foreach (var handler in handlers)
        {
            await _logger.LogAsync(
                $"{handler.RegistrationType}: {handler.ImplementationType.Name}",
                SeverityEnu.Debug);
        }
    }
}
```

## Discovery Modes

The mediator supports three discovery modes for finding and registering handlers:

### 🎯 Manual Mode (Recommended for Production)

Handlers must be explicitly registered in the DI container. This offers the best performance and control.

```csharp
services.TryAddPvNugsMediator(DiscoveryMode.Manual);

// Explicitly register each handler
services.AddTransient<
    IPvNugsMediatorRequestHandler<GetUserRequest, User>, 
    GetUserHandler>();
services.AddTransient<
    IPvNugsMediatorRequestHandler<DeleteUserRequest>, 
    DeleteUserHandler>();
```

**Pros:**
- ✅ Best performance (no reflection overhead)
- ✅ Explicit control over what's registered
- ✅ Clear dependencies in code
- ✅ Recommended for production

**Cons:**
- ❌ More boilerplate code
- ❌ Manual maintenance required

### 🏷️ Decorated Mode (Balanced Approach)

Automatically discovers handlers marked with the `[MediatorHandler]` attribute.

```csharp
services.TryAddPvNugsMediator(DiscoveryMode.Decorated);

// Handlers are auto-discovered if decorated
[MediatorHandler(ServiceLifetime.Transient)]
public class GetUserHandler : IPvNugsMediatorRequestHandler<GetUserRequest, User>
{
    public async Task<User> HandleAsync(
        GetUserRequest request, 
        CancellationToken cancellationToken)
    {
        // ... handler logic ...
    }
}
```

**Pros:**
- ✅ Automatic registration
- ✅ Explicit marking with attributes
- ✅ Control over lifetime per handler
- ✅ Good balance for most projects

**Cons:**
- ❌ Requires attribute decoration
- ❌ Small reflection overhead at startup

### 🔍 FullScan Mode (Development & Rapid Prototyping)

Automatically discovers all handlers via reflection across all loaded assemblies.

```csharp
services.TryAddPvNugsMediator(DiscoveryMode.FullScan);

// No manual registration needed - all handlers are auto-discovered
public class GetUserHandler : IPvNugsMediatorRequestHandler<GetUserRequest, User>
{
    public async Task<User> HandleAsync(
        GetUserRequest request, 
        CancellationToken cancellationToken)
    {
        // ... handler logic ...
    }
}
```

**Pros:**
- ✅ Zero configuration required
- ✅ Great for rapid development
- ✅ Auto-discovers everything

**Cons:**
- ❌ Higher startup cost (reflection scan)
- ❌ Less control over what's registered
- ❌ Not recommended for production

### ⚙️ Configuration-Based Setup

Use `appsettings.json` for flexible configuration:

```json
{
  "PvNugsMediatorConfig": {
    "DiscoveryMode": "Decorated",
    "ServiceLifetime": "Scoped"
  }
}
```

```csharp
services.TryAddPvNugsMediator(
    configuration.GetSection("PvNugsMediatorConfig"));
```

## MediatR Compatibility

This implementation is **fully compatible with MediatR**. The mediator supports both MediatR base interfaces and PvNugs-branded interfaces:

### Interface Support

```
MediatR Base Interfaces (uses Handle method):
- IMediator
- IRequest<TResponse>
- IRequest (NEW v9.0.4 - void-like requests)
- IRequestHandler<TRequest, TResponse> (returns Task<TResponse>)
- IRequestHandler<TRequest> (NEW v9.0.4 - returns Task, not Task<Unit>)
- INotification
- INotificationHandler<TNotification>
- IPipelineBehavior<TRequest, TResponse>

PvNugs Interfaces (uses HandleAsync method):
- IPvNugsMediator (extends IMediator + adds GetRegisteredHandlers())
- IPvNugsMediatorRequest<TResponse> (extends IRequest<TResponse>)
- IPvNugsMediatorRequest (extends IRequest - void-like)
- IPvNugsMediatorRequestHandler<TRequest, TResponse> (returns Task<TResponse>)
- IPvNugsMediatorRequestHandler<TRequest> (NEW v9.0.4 - returns Task, not Task<Unit>)
- IPvNugsMediatorNotification (extends INotification)
- IPvNugsMediatorNotificationHandler<TNotification> (standalone - HandleAsync only)
- IPvNugsMediatorPipelineRequestHandler<TRequest, TResponse> (standalone - HandleAsync only)
```

### Method Resolution

The mediator **automatically detects** which interface type your handler implements and calls the correct method:

| Handler Type | Method Called | Example |
|--------------|---------------|---------|
| MediatR (`IRequestHandler`) | `Handle(request, ct)` | Base MediatR handlers |
| PvNugs (`IPvNugsMediatorRequestHandler`) | `HandleAsync(request, ct)` | PvNugs handlers |
| MediatR (`INotificationHandler`) | `Handle(notification, ct)` | Base MediatR notification handlers |
| PvNugs (`IPvNugsMediatorNotificationHandler`) | `HandleAsync(notification, ct)` | PvNugs notification handlers |

**Both work seamlessly through the same mediator instance!**

### Drop-in Replacement Example

```csharp
// Before (MediatR)
services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(assembly));
var result = await mediator.Send(new GetUserRequest());

// After (PvNugs) - Same code works!
services.TryAddPvNugsMediator(DiscoveryMode.FullScan);
var result = await mediator.Send(new GetUserRequest());

// Bonus: Use PvNugs-specific features
var handlers = mediator.GetRegisteredHandlers();
```

## How It Works

### 🎯 Request Handling Flow

```
1. Request arrives → SendAsync<TResponse>()
2. Mediator uses reflection to find handler type
3. Handler resolved from DI container
4. Pipeline behaviors resolved (if registered)
5. Pipeline chain built (last-to-first registration order)
6. Pipelines execute in reverse order (wrapping handler)
7. Handler executes
8. Response returns through pipeline chain
9. All operations logged automatically
```

**Example with Pipelines:**
```
Request 
  → ValidationPipeline (pre-process)
    → LoggingPipeline (pre-process)
      → Handler (execute)
    → LoggingPipeline (post-process)
  → ValidationPipeline (post-process)
Response
```

### 📢 Notification Publishing Flow

```
1. Notification arrives → PublishAsync()
2. Mediator uses reflection to find handler types
3. All handlers resolved from DI container
4. Handlers execute sequentially
5. All operations logged automatically
```

## Logging

The mediator automatically logs:

✅ **Trace Level**: Request/notification handling start  
✅ **Warning Level**: No handlers found for notification  
✅ **Error Level**: Missing handlers, missing methods, handler exceptions  

**Example Log Output:**
```
[16:18:59 VRB] Handling request of type MyApp.GetUserByIdRequest
[16:18:59 VRB] Handling notification of type MyApp.UserCreatedNotification
[16:19:00 WRN] No handlers registered for notification type MyApp.UnknownNotification
[16:19:01 ERR] No handler registered for request type MyApp.UnregisteredRequest
```

## Exception Handling

The mediator throws `PvNugsMediatorException` in these scenarios:

❌ No handler registered for a request type  
❌ Handler doesn't have a `HandleAsync` method  
❌ Pipeline doesn't have a `HandleAsync` method  
❌ Exception occurs during handler execution (wrapped)  

**Exception Structure:**
```csharp
try
{
    await _mediator.SendAsync(request);
}
catch (PvNugsMediatorException ex)
{
    // ex.Message contains descriptive error
    // ex.InnerException contains original exception (if wrapped)
    await _logger.LogAsync(ex, SeverityEnu.Error);
}
```

## Complete Example

### Define Request and Handler

#### Query Handler (returns data)

```csharp
// Request
public class GetUserByIdRequest : IPvNugsMediatorRequest<User>
{
    public int UserId { get; init; }
}

// Handler
public class GetUserByIdHandler : IPvNugsMediatorRequestHandler<GetUserByIdRequest, User>
{
    private readonly IUserRepository _repository;
    private readonly ILoggerService _logger;
    
    public GetUserByIdHandler(IUserRepository repository, ILoggerService logger)
    {
        _repository = repository;
        _logger = logger;
    }
    
    public async Task<User> HandleAsync(
        GetUserByIdRequest request, 
        CancellationToken cancellationToken)
    {
        await _logger.LogAsync(
            $"Retrieving user {request.UserId}", 
            SeverityEnu.Debug);
        
        var user = await _repository.GetByIdAsync(request.UserId, cancellationToken);
        
        if (user == null)
            throw new UserNotFoundException(request.UserId);
        
        return user;
    }
}
```

#### Command Handler (void-like - NEW in v9.0.4!)

```csharp
// Request
public class DeleteUserRequest : IPvNugsMediatorRequest
{
    public int UserId { get; init; }
}

// Handler - Returns Task instead of Task<Unit>
public class DeleteUserHandler : IPvNugsMediatorRequestHandler<DeleteUserRequest>
{
    private readonly IUserRepository _repository;
    private readonly ILoggerService _logger;
    
    public DeleteUserHandler(IUserRepository repository, ILoggerService logger)
    {
        _repository = repository;
        _logger = logger;
    }
    
    public async Task HandleAsync(
        DeleteUserRequest request, 
        CancellationToken cancellationToken)
    {
        await _logger.LogAsync(
            $"Deleting user {request.UserId}", 
            SeverityEnu.Debug);
        
        await _repository.DeleteAsync(request.UserId, cancellationToken);
        
        // No need to return Unit.Value! ✨
    }
}
```

### Define Pipeline Behavior

```csharp
public class LoggingPipeline<TRequest, TResponse> : IPvNugsMediatorPipelineRequestHandler<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ILoggerService _logger;
    
    public LoggingPipeline(ILoggerService logger)
    {
        _logger = logger;
    }
    
    public async Task<TResponse> HandleAsync(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        var stopwatch = Stopwatch.StartNew();
        
        await _logger.LogAsync(
            $"[START] {requestName}", 
            SeverityEnu.Trace);
        
        try
        {
            var response = await next();
            
            stopwatch.Stop();
            await _logger.LogAsync(
                $"[SUCCESS] {requestName} - {stopwatch.ElapsedMilliseconds}ms", 
                SeverityEnu.Trace);
            
            return response;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            await _logger.LogAsync(
                $"[FAILED] {requestName} - {stopwatch.ElapsedMilliseconds}ms: {ex.Message}", 
                SeverityEnu.Error);
            throw;
        }
    }
}
```

### Define Notification and Handlers

```csharp
// Notification
public class UserCreatedNotification : IPvNugsMediatorNotification
{
    public int UserId { get; init; }
    public string Email { get; init; }
}

// Handler 1: Send Email
public class SendWelcomeEmailHandler : IPvNugsMediatorNotificationHandler<UserCreatedNotification>
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
        await _emailService.SendWelcomeEmailAsync(
            notification.Email, 
            cancellationToken);
    }
}

// Handler 2: Log Event
public class LogUserCreationHandler : IPvNugsMediatorNotificationHandler<UserCreatedNotification>
{
    private readonly ILoggerService _logger;
    
    public LogUserCreationHandler(ILoggerService logger)
    {
        _logger = logger;
    }
    
    public async Task HandleAsync(
        UserCreatedNotification notification, 
        CancellationToken cancellationToken)
    {
        await _logger.LogAsync(
            $"User created: {notification.UserId} - {notification.Email}", 
            SeverityEnu.Info);
    }
}
```

### Complete DI Setup

```csharp
var services = new ServiceCollection();

// Logger
services.TryAddPvNugsLoggerSeriService(config);

// Repositories
services.AddScoped<IUserRepository, UserRepository>();

// Email Service
services.AddTransient<IEmailService, EmailService>();

// Mediator with discovery mode
services.TryAddPvNugsMediator(DiscoveryMode.Manual);

// Request Handlers (Manual mode requires explicit registration)
services.AddTransient<
    IPvNugsMediatorRequestHandler<GetUserByIdRequest, User>, 
    GetUserByIdHandler>();

// Notification Handlers
services.AddTransient<
    IPvNugsMediatorNotificationHandler<UserCreatedNotification>, 
    SendWelcomeEmailHandler>();
services.AddTransient<
    IPvNugsMediatorNotificationHandler<UserCreatedNotification>, 
    LogUserCreationHandler>();

// Pipelines
services.AddTransient(
    typeof(IPvNugsMediatorPipelineRequestHandler<,>), 
    typeof(LoggingPipeline<,>));


var sp = services.BuildServiceProvider();
```

### WebAPI Integration with Scoped Services

The mediator is registered as **scoped**, making it perfect for ASP.NET Core applications with scoped dependencies:

```csharp
// Program.cs (WebAPI)
var builder = WebApplication.CreateBuilder(args);

// Register DbContext (scoped)
builder.Services.AddDbContext<MyDbContext>(options =>
    options.UseSqlServer(connectionString));

// Register logger
builder.Services.TryAddPvNugsLoggerSeriService(builder.Configuration);

// Register mediator (scoped)
builder.Services.TryAddPvNugsMediator(DiscoveryMode.Decorated);

var app = builder.Build();
```

**Handler with Scoped Dependencies:**

```csharp
[MediatorHandler(pvNugsMediatorNc9Abstractions.ServiceLifetime.Scoped)]
public class CreateOrderHandler : IPvNugsMediatorRequestHandler<CreateOrderRequest, OrderResult>
{
    private readonly MyDbContext _db;           // Scoped - same instance per request
    private readonly ICurrentUser _userContext;  // Scoped - current authenticated user
    private readonly ILoggerService _logger;     // Can be singleton or scoped
    
    public CreateOrderHandler(MyDbContext db, ICurrentUser userContext, ILoggerService logger)
    {
        _db = db;
        _userContext = userContext;
        _logger = logger;
    }
    
    public async Task<OrderResult> HandleAsync(
        CreateOrderRequest request, 
        CancellationToken cancellationToken)
    {
        // All dependencies are properly resolved for the current HTTP request
        var order = new Order
        {
            UserId = _userContext.UserId,  // ✅ Current request's user
            Products = request.Products,
            // ...
        };
        
        await _db.Orders.AddAsync(order, cancellationToken);
        await _db.SaveChangesAsync(cancellationToken);  // ✅ Uses correct DbContext
        
        return new OrderResult { OrderId = order.Id };
    }
}
```

**Controller Usage:**

```csharp
[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly MyDbContext _db;
    
    public OrdersController(IMediator mediator, MyDbContext db)
    {
        _mediator = mediator;
        _db = db;  // Same instance as handler will receive
    }
    
    [HttpPost]
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequest request)
    {
        // Handler gets the SAME DbContext instance
        var result = await _mediator.Send(request);
        
        // Changes made in handler are visible here
        var order = await _db.Orders.FindAsync(result.OrderId);
        
        return CreatedAtAction(nameof(GetOrder), new { id = result.OrderId }, order);
    }
}
```

**Key Benefits:**
- ✅ Handler and controller share the same `DbContext` instance
- ✅ Changes made in handler are visible in controller
- ✅ Each HTTP request gets its own isolated scope
- ✅ Proper disposal of scoped services at request end
- ✅ No cross-contamination between concurrent requests

## Advanced Scenarios

### Generic Pipeline Registration

Register a pipeline for ALL request types:

```csharp
// This pipeline applies to every request
services.AddTransient(
    typeof(IPvNugsMediatorPipelineRequestHandler<,>), 
    typeof(LoggingPipeline<,>));
```

### Request-Specific Pipeline

Register a pipeline for a specific request:

```csharp
// This pipeline only applies to GetUserByIdRequest
services.AddTransient<
    IPvNugsMediatorPipelineRequestHandler<GetUserByIdRequest, User>, 
    PerformancePipeline>();
```

### Pipeline Execution Order

Pipelines execute in **reverse registration order**:

```csharp
services.AddTransient<IPvNugsMediatorPipelineRequestHandler<MyRequest, MyResponse>, Pipeline1>();
services.AddTransient<IPvNugsMediatorPipelineRequestHandler<MyRequest, MyResponse>, Pipeline2>();
services.AddTransient<IPvNugsMediatorPipelineRequestHandler<MyRequest, MyResponse>, Pipeline3>();

// Execution order: Pipeline3 → Pipeline2 → Pipeline1 → Handler
```

### Runtime Type Notifications

Use the non-generic `Publish` for polymorphic notifications:

```csharp
object notification = GetNotificationFromSomewhere();
await _mediator.Publish(notification); // Runtime type resolution
```

### Using Decorated Mode with Attributes

Mark handlers with the `[MediatorHandler]` attribute to control registration:

```csharp
// This handler will be auto-discovered in Decorated mode
[MediatorHandler(ServiceLifetime.Scoped)]
public class MyHandler : IPvNugsMediatorRequestHandler<MyRequest, MyResponse>
{
    public async Task<MyResponse> HandleAsync(MyRequest request, CancellationToken cancellationToken)
    {
        // Handler logic
    }
}

// This handler won't be auto-discovered (no attribute)
public class ManualHandler : IPvNugsMediatorRequestHandler<OtherRequest, OtherResponse>
{
    public async Task<OtherResponse> HandleAsync(OtherRequest request, CancellationToken cancellationToken)
    {
        // Handler logic
    }
}
```

## Performance Tips

✅ **Scoped Mediator**: Registered as scoped for proper WebAPI integration with scoped dependencies  
✅ **Cache Handler Types**: Reflection overhead is minimized  
✅ **Sequential Pipelines**: Pipelines execute in order (not parallel overhead)  
✅ **Notification Handlers**: Execute sequentially (predictable performance)  

## Best Practices

1. ✅ **Always Register Logger First**: The mediator requires `ILoggerService`
2. ✅ **Choose Appropriate Discovery Mode**: Manual for production, FullScan for development, Decorated for balance
3. ✅ **One Handler Per Request**: Follow mediator pattern - exactly one handler per request type
4. ✅ **Multiple Handlers for Notifications**: Use notifications for fan-out scenarios
5. ✅ **Use Pipelines for Cross-Cutting Concerns**: Don't duplicate logging/validation in handlers
6. ✅ **Handle PvNugsMediatorException**: Catch and log mediator exceptions appropriately
7. ✅ **Register Handlers as Transient/Scoped**: Avoid singleton handlers with state
8. ✅ **Use `GetRegisteredHandlers()` for Diagnostics**: Validate handler registration during development

## Troubleshooting

### "No handler registered for request type"

**Cause**: Handler not registered in DI container  
**Solution**: Add handler registration:
```csharp
services.AddTransient<IPvNugsMediatorRequestHandler<YourRequest, YourResponse>, YourHandler>();
```

### "Handler does not have a 'Handle' method"

**Cause**: Handler doesn't implement the interface correctly  
**Solution**: Ensure handler implements `IPvNugsMediatorRequestHandler<TRequest, TResponse>` with `Handle` method (which internally calls `HandleAsync`)

### No logs appearing

**Cause**: Logger not registered or log level too high  
**Solution**: 
```csharp
// Ensure logger is registered
services.TryAddPvNugsLoggerSeriService(config);

// Set appropriate log level in config
{ "PvNugsLoggerConfig:MinLogLevel", "trace" }
```

## Compatibility

- **.NET Version**: .NET 9.0+
- **Logger**: Any implementation of `pvNugsLoggerNc9Abstractions`
- **DI Container**: Microsoft.Extensions.DependencyInjection

## Related Packages

📦 **pvNugsMediatorNc9Abstractions** - Interface definitions (required)  
📦 **pvNugsLoggerNc9Abstractions** - Logger abstractions (required)  
📦 **pvNugsLoggerNc9Seri** - Serilog logger implementation (recommended)  
📦 **pvNugsLoggerNc9MsSql** - SQL Server logger implementation  
📦 **pvNugsLoggerNc9Hybrid** - Multi-output logger implementation  

## License

MIT License - see LICENSE file for details

## Author

Pierre Van Wallendael - pvWay Ltd

## Repository

[GitHub Repository](https://github.com/licheez/pvWayNugs.git)

---

💡 **Pro Tip**: Combine with pipeline behaviors for powerful cross-cutting concerns like validation, caching, and retry logic without cluttering your handlers!

🔗 **Need the abstractions?** Install `pvNugsMediatorNc9Abstractions` to define your own mediator implementation.


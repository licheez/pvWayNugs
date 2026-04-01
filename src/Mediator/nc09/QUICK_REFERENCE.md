# Task-Returning Handlers - Quick Reference

## ✨ What's New in v9.0.7 / v9.0.4?

**Cleaner void-like handlers!** No more `return Unit.Value;` required.

---

## 📋 Interface Quick Reference

### For Queries (with response)
```csharp
// Returns data - use two-parameter interface
public class GetUserHandler : IPvNugsMediatorRequestHandler<GetUserRequest, User>
{
    public async Task<User> HandleAsync(GetUserRequest req, CancellationToken ct)
    {
        return await _repo.GetByIdAsync(req.UserId, ct);
    }
}
```

### For Commands (void-like) - NEW!
```csharp
// No return data - use single-parameter interface
public class DeleteUserHandler : IPvNugsMediatorRequestHandler<DeleteUserRequest>
{
    public async Task HandleAsync(DeleteUserRequest req, CancellationToken ct)
    {
        await _repo.DeleteAsync(req.UserId, ct);
        // No return needed! ✨
    }
}
```

---

## 🔄 Migration Examples

### PvNugs Style

**Before:**
```csharp
public class MyHandler : IPvNugsMediatorRequestHandler<MyRequest, Unit>
{
    public async Task<Unit> HandleAsync(MyRequest request, CancellationToken ct)
    {
        await DoWork();
        return Unit.Value; // ← Boilerplate
    }
}
```

**After:**
```csharp
public class MyHandler : IPvNugsMediatorRequestHandler<MyRequest>
{
    public async Task HandleAsync(MyRequest request, CancellationToken ct)
    {
        await DoWork(); // ← Clean!
    }
}
```

### MediatR Style

**Before:**
```csharp
public class MyHandler : IRequestHandler<MyRequest, Unit>
{
    public async Task<Unit> Handle(MyRequest request, CancellationToken ct)
    {
        await DoWork();
        return Unit.Value; // ← Boilerplate
    }
}
```

**After:**
```csharp
public class MyHandler : IRequestHandler<MyRequest>
{
    public async Task Handle(MyRequest request, CancellationToken ct)
    {
        await DoWork(); // ← Clean!
    }
}
```

---

## 🎯 When to Use Each Interface

| Scenario | Use This | Returns |
|----------|----------|---------|
| Get user by ID | `IPvNugsMediatorRequestHandler<GetUserRequest, User>` | `Task<User>` |
| Delete user | `IPvNugsMediatorRequestHandler<DeleteUserRequest>` | `Task` ✨ |
| Update user | `IPvNugsMediatorRequestHandler<UpdateUserRequest>` | `Task` ✨ |
| Send email | `IRequestHandler<SendEmailRequest>` | `Task` ✨ |
| Calculate sum | `IRequestHandler<CalculateRequest, int>` | `Task<int>` |

---

## 📦 Package Versions

| Package | Version | Status |
|---------|---------|--------|
| pvNugsMediatorNc9Abstractions | 9.0.7 | ✅ Published |
| pvNugsMediatorNc9 | 9.0.4 | ✅ Ready |

---

## ⚡ Quick Start

### 1. Install/Update Packages
```bash
dotnet add package pvNugsMediatorNc9Abstractions --version 9.0.7
dotnet add package pvNugsMediatorNc9 --version 9.0.4
```

### 2. Create a Command Request
```csharp
public class DeleteUserRequest : IPvNugsMediatorRequest
{
    public int UserId { get; init; }
}
```

### 3. Create a Handler (NEW WAY!)
```csharp
public class DeleteUserHandler : IPvNugsMediatorRequestHandler<DeleteUserRequest>
{
    private readonly IUserRepository _repo;
    
    public DeleteUserHandler(IUserRepository repo) => _repo = repo;
    
    public async Task HandleAsync(DeleteUserRequest request, CancellationToken ct)
    {
        await _repo.DeleteAsync(request.UserId, ct);
        // That's it! No return needed!
    }
}
```

### 4. Register and Use
```csharp
// Registration
services.TryAddPvNugsMediator(DiscoveryMode.FullScan);
// Or manual:
services.AddTransient<IPvNugsMediatorRequestHandler<DeleteUserRequest>, DeleteUserHandler>();

// Usage
await mediator.SendAsync(new DeleteUserRequest { UserId = 123 });
```

---

## ✅ Benefits

✨ **Cleaner Code** - No more `return Unit.Value;`  
✨ **Natural C#** - Just return `Task` for void operations  
✨ **MediatR Compatible** - Same interface names  
✨ **Backward Compatible** - Old code still works  
✨ **Type Safe** - Compile-time safety  
✨ **Well Tested** - 4 integration tests included  

---

## 🔍 Discovery Modes

All discovery modes support the new interfaces:

```csharp
// Manual - register yourself
services.TryAddPvNugsMediator(DiscoveryMode.Manual);
services.AddTransient<IPvNugsMediatorRequestHandler<MyRequest>, MyHandler>();

// Decorated - use [MediatorHandler]
[MediatorHandler(Lifetime = ServiceLifetime.Transient)]
public class MyHandler : IPvNugsMediatorRequestHandler<MyRequest> { }

// FullScan - automatic
services.TryAddPvNugsMediator(DiscoveryMode.FullScan);
```

---

## 📖 Full Documentation

See these files for more details:
- `PHASE1_CHANGES_SUMMARY.md` - Abstractions changes
- `PHASE2_IMPLEMENTATION_SUMMARY.md` - Implementation details
- `TASK_RETURNING_HANDLERS_COMPLETE.md` - Complete overview

---

## 💡 Pro Tips

1. **Use single-parameter interfaces for commands** (no return data)
2. **Use two-parameter interfaces for queries** (return data)
3. **Both work together** - mix and match in your codebase
4. **Migrate gradually** - no rush to update existing handlers
5. **Let FullScan discover handlers** - less manual registration

---

**Quick Reference Card** | pvNugs Mediator v9.0.4+ | Updated: April 1, 2026


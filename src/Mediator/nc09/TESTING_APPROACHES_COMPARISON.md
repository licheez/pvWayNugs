# Testing Mediator Scoping: WebAPI vs Console

## 🎯 What Are You Testing?

You want to verify:
1. ✅ Scoped handlers resolve correctly
2. ✅ Each "request" gets its own scoped services
3. ✅ Transient/Scoped/Singleton lifetimes work as expected
4. ✅ No cross-contamination between requests

---

## Approach 1: Console App ✅ **RECOMMENDED for Scoping Tests**

### Pros:
- ✅ **Simple** - No HTTP stack complexity
- ✅ **Fast** - No web server startup
- ✅ **Identical scoping behavior** - `CreateScope()` mimics HTTP request scope
- ✅ **Easy to debug** - Step through code easily
- ✅ **Sufficient** - Tests the DI scoping, which is what matters

### Cons:
- ❌ Doesn't test HTTP pipeline
- ❌ Doesn't test middleware integration
- ❌ Doesn't test controller integration

### Example:

```csharp
// Program.cs - Console App
var services = new ServiceCollection();

// Register logger
services.AddSingleton<ILoggerService, ConsoleLogger>();

// Register DbContext (scoped)
services.AddScoped<TestDbContext>();

// Register Mediator
services.TryAddPvNugsMediator(DiscoveryMode.FullScan);

// Register test handler (scoped)
services.AddScoped<IRequestHandler<TestRequest, TestResponse>, TestHandler>();

var rootProvider = services.BuildServiceProvider();

// Simulate Request 1
Console.WriteLine("=== REQUEST 1 ===");
using (var scope1 = rootProvider.CreateScope())
{
    var mediator1 = scope1.ServiceProvider.GetRequiredService<IMediator>();
    var db1 = scope1.ServiceProvider.GetRequiredService<TestDbContext>();
    
    Console.WriteLine($"DbContext1 ID: {db1.InstanceId}");
    
    var result = await mediator1.Send(new TestRequest { Value = "Request1" });
    
    Console.WriteLine($"Handler saw DbContext ID: {result.DbContextId}");
    Console.WriteLine($"Match: {db1.InstanceId == result.DbContextId}"); // Should be TRUE
}

// Simulate Request 2
Console.WriteLine("\n=== REQUEST 2 ===");
using (var scope2 = rootProvider.CreateScope())
{
    var mediator2 = scope2.ServiceProvider.GetRequiredService<IMediator>();
    var db2 = scope2.ServiceProvider.GetRequiredService<TestDbContext>();
    
    Console.WriteLine($"DbContext2 ID: {db2.InstanceId}");
    
    var result = await mediator2.Send(new TestRequest { Value = "Request2" });
    
    Console.WriteLine($"Handler saw DbContext ID: {result.DbContextId}");
    Console.WriteLine($"Match: {db2.InstanceId == result.DbContextId}"); // Should be TRUE
    Console.WriteLine($"Different from Request1: {result.DbContextId != /* previous ID */}"); // Should be TRUE
}
```

**Output:**
```
=== REQUEST 1 ===
DbContext1 ID: abc123
Handler saw DbContext ID: abc123
Match: True ✅

=== REQUEST 2 ===
DbContext2 ID: def456
Handler saw DbContext ID: def456
Match: True ✅
Different from Request1: True ✅
```

---

## Approach 2: WebAPI Integration Tests ⚡ **For Full Integration**

### Pros:
- ✅ **Real HTTP pipeline** - Tests entire stack
- ✅ **HTTP Context** - Tests HttpContext-dependent features
- ✅ **Middleware** - Tests middleware interaction
- ✅ **Realistic** - Closest to production

### Cons:
- ❌ **More complex** - Requires WebApplicationFactory
- ❌ **Slower** - Spins up test server
- ❌ **Heavier** - More dependencies (Microsoft.AspNetCore.Mvc.Testing)

### Example:

```csharp
// WebApiTests.cs
public class MediatorScopingTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    
    public MediatorScopingTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }
    
    [Fact]
    public async Task MultipleRequests_Should_GetDifferentScopedInstances()
    {
        // Arrange
        var client = _factory.CreateClient();
        
        // Act - Request 1
        var response1 = await client.PostAsync("/api/test", 
            JsonContent.Create(new { Value = "Test1" }));
        var result1 = await response1.Content.ReadFromJsonAsync<TestResponse>();
        
        // Act - Request 2
        var response2 = await client.PostAsync("/api/test", 
            JsonContent.Create(new { Value = "Test2" }));
        var result2 = await response2.Content.ReadFromJsonAsync<TestResponse>();
        
        // Assert
        Assert.NotEqual(result1.DbContextId, result2.DbContextId); // Different scopes
    }
}
```

---

## 📊 Comparison Matrix

| Aspect | Console App | WebAPI Integration Test |
|--------|-------------|------------------------|
| **Tests DI Scoping** | ✅ Yes | ✅ Yes |
| **Tests HTTP Pipeline** | ❌ No | ✅ Yes |
| **Setup Complexity** | ⭐ Low | ⭐⭐⭐ High |
| **Execution Speed** | ⚡⚡⚡ Fast | ⚡ Slower |
| **Debugging Ease** | ✅ Easy | ⚠️ Moderate |
| **Dependencies** | Minimal | Heavy (ASP.NET) |
| **For Scoping Validation** | ✅ **Sufficient** | ⚠️ **Overkill** |
| **For Full Integration** | ❌ Insufficient | ✅ **Appropriate** |

---

## 🎯 My Recommendation

### For YOUR specific need (testing scoped behavior):
**Use a Console App** ⭐ **BEST CHOICE**

**Why:**
1. The scoping mechanism is **identical** - `CreateScope()` is what ASP.NET uses internally
2. You're testing the **DI container behavior**, not HTTP features
3. Much **faster** to write and run
4. **Easier to debug** - Direct code execution
5. **Proves your architecture** without HTTP overhead

### When to use WebAPI Integration Tests:
- Testing **controllers** with mediator
- Testing **middleware** that interacts with mediator
- Testing **authentication/authorization** with mediator
- End-to-end **feature testing**
- When you need **HttpContext** in handlers

---

## 🚀 Recommendation: **Create BOTH!**

### 1. Console App (for scoping proof)
- Quick to create (~15 minutes)
- Proves DI architecture works
- Can run frequently during development

### 2. WebAPI Integration Tests (for real-world scenarios)
- Full integration confidence
- Catches HTTP-specific issues
- Better for CI/CD pipelines

**Start with console app, add WebAPI tests later!**


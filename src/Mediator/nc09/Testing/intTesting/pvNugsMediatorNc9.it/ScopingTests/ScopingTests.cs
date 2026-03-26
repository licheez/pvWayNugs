using Microsoft.Extensions.DependencyInjection;
using pvNugsLoggerNc9Abstractions;
using pvNugsMediatorNc9Abstractions;
using pvNugsMediatorNc9Abstractions.Mediator;
using pvNugsMediatorNc9Abstractions.pvNugs;

namespace pvNugsMediatorNc9.it.ScopingTests;

/// <summary>
/// Comprehensive tests for validating scoped behavior of the mediator and handlers.
/// Simulates HTTP request scopes using IServiceScope to prove the architecture works correctly.
/// </summary>
public class ScopingTests(ILoggerService logger)
{
    public async Task RunAllTestsAsync()
    {
        Console.WriteLine("\n" + new string('=', 80));
        Console.WriteLine("MEDIATOR SCOPING TESTS - Simulating HTTP Request Scopes");
        Console.WriteLine(new string('=', 80) + "\n");
        
        await Test1_ScopedHandler_SameDbContextWithinScope();
        await Test2_ScopedHandler_DifferentDbContextAcrossScopes();
        await Test3_TransientHandler_SameDbContextWithinScope();
        await Test4_MultipleCallsInSameScope();
        await Test5_DisposalVerification();
        
        Console.WriteLine("\n" + new string('=', 80));
        Console.WriteLine("✅ ALL SCOPING TESTS PASSED!");
        Console.WriteLine(new string('=', 80) + "\n");
    }
    
    /// <summary>
    /// Test 1: Within a single scope, handler should get the SAME DbContext as the caller.
    /// </summary>
    private async Task Test1_ScopedHandler_SameDbContextWithinScope()
    {
        Console.WriteLine("TEST 1: Scoped Handler - Same DbContext Within Scope");
        Console.WriteLine(new string('-', 80));
        
        var services = new ServiceCollection();
        ConfigureServices(services);
        
        var rootProvider = services.BuildServiceProvider();
        
        using var scope = rootProvider.CreateScope();
        var sp = scope.ServiceProvider;
        
        var mediator = sp.GetRequiredService<IMediator>();
        var dbContext = sp.GetRequiredService<TestDbContext>();
        
        Console.WriteLine($"  Controller's DbContext ID: {dbContext.InstanceId:N}");
        
        var result = await mediator.Send(new ScopedTestRequest("Request1"));
        
        Console.WriteLine($"  Handler's DbContext ID:    {result.DbContextId:N}");
        
        var isSame = dbContext.InstanceId == result.DbContextId;
        Console.WriteLine($"  Are they the same? {(isSame ? "✅ YES" : "❌ NO")}");
        
        if (!isSame)
            throw new Exception("❌ TEST FAILED: DbContext instances should be the same within a scope!");
        
        Console.WriteLine("  ✅ PASSED: Handler correctly received the same DbContext instance\n");
    }
    
    /// <summary>
    /// Test 2: Different scopes should get DIFFERENT DbContext instances.
    /// </summary>
    private async Task Test2_ScopedHandler_DifferentDbContextAcrossScopes()
    {
        Console.WriteLine("TEST 2: Scoped Handler - Different DbContext Across Scopes");
        Console.WriteLine(new string('-', 80));
        
        var services = new ServiceCollection();
        ConfigureServices(services);
        
        var rootProvider = services.BuildServiceProvider();
        
        Guid dbId1, dbId2;
        
        // Scope 1 (simulates HTTP Request 1)
        Console.WriteLine("  Scope 1 (Request 1):");
        using (var scope1 = rootProvider.CreateScope())
        {
            var mediator1 = scope1.ServiceProvider.GetRequiredService<IMediator>();
            var result1 = await mediator1.Send(new ScopedTestRequest("Request1"));
            dbId1 = result1.DbContextId;
            Console.WriteLine($"    DbContext ID: {dbId1:N}");
        }
        
        // Scope 2 (simulates HTTP Request 2)
        Console.WriteLine("  Scope 2 (Request 2):");
        using (var scope2 = rootProvider.CreateScope())
        {
            var mediator2 = scope2.ServiceProvider.GetRequiredService<IMediator>();
            var result2 = await mediator2.Send(new ScopedTestRequest("Request2"));
            dbId2 = result2.DbContextId;
            Console.WriteLine($"    DbContext ID: {dbId2:N}");
        }
        
        var isDifferent = dbId1 != dbId2;
        Console.WriteLine($"  Are they different? {(isDifferent ? "✅ YES" : "❌ NO")}");
        
        if (!isDifferent)
            throw new Exception("❌ TEST FAILED: Different scopes should get different DbContext instances!");
        
        Console.WriteLine("  ✅ PASSED: Each scope correctly received a different DbContext instance\n");
    }
    
    /// <summary>
    /// Test 3: Transient handler should still get the SAME scoped DbContext as the caller.
    /// </summary>
    private async Task Test3_TransientHandler_SameDbContextWithinScope()
    {
        Console.WriteLine("TEST 3: Transient Handler - Same DbContext Within Scope");
        Console.WriteLine(new string('-', 80));
        
        var services = new ServiceCollection();
        ConfigureServicesWithTransientHandler(services);
        
        var rootProvider = services.BuildServiceProvider();
        
        using var scope = rootProvider.CreateScope();
        var sp = scope.ServiceProvider;
        
        var mediator = sp.GetRequiredService<IMediator>();
        var dbContext = sp.GetRequiredService<TestDbContext>();
        
        Console.WriteLine($"  Controller's DbContext ID: {dbContext.InstanceId:N}");
        
        var result = await mediator.Send(new ScopedTestRequest("TransientTest"));
        
        Console.WriteLine($"  Transient Handler's DbContext ID: {result.DbContextId:N}");
        
        var isSame = dbContext.InstanceId == result.DbContextId;
        Console.WriteLine($"  Are they the same? {(isSame ? "✅ YES" : "❌ NO")}");
        
        if (!isSame)
            throw new Exception("❌ TEST FAILED: Transient handler should still get the same scoped DbContext!");
        
        Console.WriteLine("  ✅ PASSED: Transient handler correctly received the scoped DbContext\n");
    }
    
    /// <summary>
    /// Test 4: Multiple mediator calls within the same scope should share the same DbContext.
    /// </summary>
    private async Task Test4_MultipleCallsInSameScope()
    {
        Console.WriteLine("TEST 4: Multiple Calls In Same Scope");
        Console.WriteLine(new string('-', 80));
        
        var services = new ServiceCollection();
        ConfigureServices(services);
        
        var rootProvider = services.BuildServiceProvider();
        
        using var scope = rootProvider.CreateScope();
        var sp = scope.ServiceProvider;
        
        var mediator = sp.GetRequiredService<IMediator>();
        var dbContext = sp.GetRequiredService<TestDbContext>();
        
        Console.WriteLine($"  Scope's DbContext ID: {dbContext.InstanceId:N}");
        Console.WriteLine($"  Initial data count: {dbContext.Data.Count}");
        
        // Call 1
        var result1 = await mediator.Send(new ScopedTestRequest("Call1"));
        Console.WriteLine($"  After Call1 - DbContext ID: {result1.DbContextId:N}, Data count: {result1.DataCount}");
        
        // Call 2 - should see data from Call1 because same DbContext
        var result2 = await mediator.Send(new ScopedTestRequest("Call2"));
        Console.WriteLine($"  After Call2 - DbContext ID: {result2.DbContextId:N}, Data count: {result2.DataCount}");
        
        // Call 3
        var result3 = await mediator.Send(new ScopedTestRequest("Call3"));
        Console.WriteLine($"  After Call3 - DbContext ID: {result3.DbContextId:N}, Data count: {result3.DataCount}");
        
        var allSame = result1.DbContextId == result2.DbContextId && 
                      result2.DbContextId == result3.DbContextId;
        var dataAccumulates = result1.DataCount == 1 && 
                               result2.DataCount == 2 && 
                               result3.DataCount == 3;
        
        Console.WriteLine($"  All DbContexts the same? {(allSame ? "✅ YES" : "❌ NO")}");
        Console.WriteLine($"  Data accumulates? {(dataAccumulates ? "✅ YES" : "❌ NO")}");
        
        if (!allSame)
            throw new Exception("❌ TEST FAILED: All calls in same scope should use same DbContext!");
        
        if (!dataAccumulates)
            throw new Exception("❌ TEST FAILED: Data should accumulate in the same DbContext!");
        
        Console.WriteLine("  ✅ PASSED: All calls correctly shared the same DbContext\n");
    }
    
    /// <summary>
    /// Test 5: Verify that scoped services are disposed when scope ends.
    /// </summary>
    private async Task Test5_DisposalVerification()
    {
        Console.WriteLine("TEST 5: Disposal Verification");
        Console.WriteLine(new string('-', 80));
        
        var services = new ServiceCollection();
        ConfigureServices(services);
        
        var rootProvider = services.BuildServiceProvider();
        
        TestDbContext? dbContext;
        
        Console.WriteLine("  Creating scope...");
        using (var scope = rootProvider.CreateScope())
        {
            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
            dbContext = scope.ServiceProvider.GetRequiredService<TestDbContext>();
            
            Console.WriteLine($"  DbContext {dbContext.InstanceId:N} created");
            await mediator.Send(new ScopedTestRequest("DisposalTest"));
            
            Console.WriteLine($"  Is DbContext disposed? {(dbContext.IsDisposed ? "YES" : "NO")}");
        }
        Console.WriteLine("  Scope ended (disposed)");
        
        await Task.Delay(10); // Give disposal a moment
        
        Console.WriteLine($"  Is DbContext disposed now? {(dbContext.IsDisposed ? "✅ YES" : "❌ NO")}");
        
        if (!dbContext.IsDisposed)
            throw new Exception("❌ TEST FAILED: DbContext should be disposed when scope ends!");
        
        Console.WriteLine("  ✅ PASSED: DbContext correctly disposed when scope ended\n");
    }
    
    /// <summary>
    /// Configures services with scoped handler for testing.
    /// </summary>
    private void ConfigureServices(IServiceCollection services)
    {
        // Logger (singleton)
        services.AddSingleton(logger);
        
        // DbContext (scoped - simulates real database context)
        services.AddScoped<TestDbContext>();
        
        // Mediator (scoped)
        services.TryAddPvNugsMediator(DiscoveryMode.Decorated);
        
        // Handler will be auto-discovered via [MediatorHandler] attribute
    }
    
    /// <summary>
    /// Configures services with transient handler for testing.
    /// </summary>
    private void ConfigureServicesWithTransientHandler(IServiceCollection services)
    {
        services.AddSingleton(logger);
        services.AddScoped<TestDbContext>();
        services.TryAddPvNugsMediator(DiscoveryMode.Manual);
        
        // Manually register the transient handler
        services.AddTransient<IPvNugsMediatorRequestHandler<ScopedTestRequest, ScopedTestResponse>, 
            TransientTestHandler>();
    }
}


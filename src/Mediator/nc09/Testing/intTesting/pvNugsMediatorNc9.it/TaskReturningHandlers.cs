using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using pvNugsLoggerNc9Abstractions;
using pvNugsLoggerNc9Seri;
using pvNugsMediatorNc9Abstractions;
using pvNugsMediatorNc9Abstractions.pvNugs;
using pvNugsMediatorNc9.it.Requests.TaskReturning;
using pvNugsMediatorNc9.it.Handlers.TaskReturning;

namespace pvNugsMediatorNc9.it;

/// <summary>
/// Integration tests for Task-returning handlers (v9.0.7 feature)
/// Tests both IPvNugsMediatorRequestHandler&lt;TRequest&gt; and IRequestHandler&lt;TRequest&gt;
/// that return Task instead of Task&lt;Unit&gt;
/// </summary>
public static class TaskReturningHandlers
{
    public static async Task RunTests()
    {
        Console.WriteLine("═══════════════════════════════════════════════════════════════");
        Console.WriteLine("  TASK-RETURNING HANDLERS TESTS (v9.0.7)");
        Console.WriteLine("  Testing handlers that return Task instead of Task<Unit>");
        Console.WriteLine("═══════════════════════════════════════════════════════════════\n");

        await TestPvNugsTaskReturningHandler();
        await TestMediatRStyleTaskReturningHandler();
        await TestHandlerIntrospection();
        await TestFullScanDiscovery();
        
        Console.WriteLine("\n✅ All Task-Returning Handler Tests Passed!\n");
    }

    private static IConfiguration GetConfig()
    {
        var inMemSettings = new Dictionary<string, string>
        {
            { "PvNugsLoggerConfig:MinLogLevel", "trace" }
        };

        return new ConfigurationBuilder()
            .AddInMemoryCollection(inMemSettings!)
            .Build();
    }

    /// <summary>
    /// Test PvNugs-style handler: IPvNugsMediatorRequestHandler&lt;TRequest&gt;
    /// </summary>
    private static async Task TestPvNugsTaskReturningHandler()
    {
        Console.WriteLine("─────────────────────────────────────────────────────────────");
        Console.WriteLine("TEST 1: PvNugs Task-Returning Handler");
        Console.WriteLine("─────────────────────────────────────────────────────────────");

        var config = GetConfig();
        var services = new ServiceCollection();
        services.TryAddPvNugsLoggerSeriService(config);
        services.TryAddPvNugsMediator(DiscoveryMode.Manual);
        
        // Register handler using new single-parameter interface
        services.AddTransient<IPvNugsMediatorRequestHandler<DeleteUserRequest>, DeleteUserHandler>();
        
        var sp = services.BuildServiceProvider();
        var mediator = sp.GetRequiredService<IPvNugsMediator>();

        DeleteUserHandler.Reset();
        
        // Send request
        var request = new DeleteUserRequest { UserId = 123 };
        await mediator.SendAsync(request);
        
        // Verify handler was called
        if (DeleteUserHandler.CallCount != 1)
            throw new Exception($"Expected CallCount=1, got {DeleteUserHandler.CallCount}");
        
        if (DeleteUserHandler.LastDeletedUserId != 123)
            throw new Exception($"Expected UserId=123, got {DeleteUserHandler.LastDeletedUserId}");
        
        Console.WriteLine("✓ Handler executed successfully");
        Console.WriteLine("✓ No Unit.Value return required");
        Console.WriteLine("✓ Test Passed\n");
    }

    /// <summary>
    /// Test MediatR-style handler: IRequestHandler&lt;TRequest&gt;
    /// </summary>
    private static async Task TestMediatRStyleTaskReturningHandler()
    {
        Console.WriteLine("─────────────────────────────────────────────────────────────");
        Console.WriteLine("TEST 2: MediatR-Style Task-Returning Handler");
        Console.WriteLine("─────────────────────────────────────────────────────────────");

        var config = GetConfig();
        var services = new ServiceCollection();
        services.TryAddPvNugsLoggerSeriService(config);
        services.TryAddPvNugsMediator(DiscoveryMode.Manual);
        
        // Register handler using MediatR-style single-parameter interface
        services.AddTransient<pvNugsMediatorNc9Abstractions.Mediator.IRequestHandler<SendEmailRequest>, SendEmailHandler>();
        
        var sp = services.BuildServiceProvider();
        var mediator = sp.GetRequiredService<IPvNugsMediator>();

        SendEmailHandler.Reset();
        
        // Send request
        var request = new SendEmailRequest 
        { 
            To = "test@example.com", 
            Subject = "Test",
            Body = "Hello!"
        };
        await mediator.SendAsync(request);
        
        // Verify handler was called
        if (SendEmailHandler.CallCount != 1)
            throw new Exception($"Expected CallCount=1, got {SendEmailHandler.CallCount}");
        
        if (SendEmailHandler.LastRequest?.To != "test@example.com")
            throw new Exception($"Expected To=test@example.com, got {SendEmailHandler.LastRequest?.To}");
        
        Console.WriteLine("✓ MediatR-style handler executed successfully");
        Console.WriteLine("✓ Uses Handle method (not HandleAsync)");
        Console.WriteLine("✓ No Unit.Value return required");
        Console.WriteLine("✓ Test Passed\n");
    }

    /// <summary>
    /// Test handler introspection includes new Task-returning handlers
    /// </summary>
    private static async Task TestHandlerIntrospection()
    {
        Console.WriteLine("─────────────────────────────────────────────────────────────");
        Console.WriteLine("TEST 3: Handler Introspection");
        Console.WriteLine("─────────────────────────────────────────────────────────────");

        var config = GetConfig();
        var services = new ServiceCollection();
        services.TryAddPvNugsLoggerSeriService(config);
        services.TryAddPvNugsMediator(DiscoveryMode.Manual);
        
        // Register both types of Task-returning handlers
        services.AddTransient<IPvNugsMediatorRequestHandler<DeleteUserRequest>, DeleteUserHandler>();
        services.AddTransient<pvNugsMediatorNc9Abstractions.Mediator.IRequestHandler<SendEmailRequest>, SendEmailHandler>();
        
        var sp = services.BuildServiceProvider();
        var mediator = sp.GetRequiredService<IPvNugsMediator>();

        var registrations = mediator.GetRegisteredHandlers().ToList();
        
        var deleteUserReg = registrations.FirstOrDefault(r => 
            r.MessageType?.Name == nameof(DeleteUserRequest));
        var sendEmailReg = registrations.FirstOrDefault(r => 
            r.MessageType?.Name == nameof(SendEmailRequest));
        
        if (deleteUserReg == null)
            throw new Exception("DeleteUserHandler not found in registrations");
        
        if (sendEmailReg == null)
            throw new Exception("SendEmailHandler not found in registrations");
        
        Console.WriteLine($"✓ Found DeleteUserHandler: {deleteUserReg.RegistrationType}");
        Console.WriteLine($"✓ Found SendEmailHandler: {sendEmailReg.RegistrationType}");
        Console.WriteLine("✓ Both Task-returning handlers registered correctly");
        Console.WriteLine("✓ Test Passed\n");
        
        await Task.CompletedTask;
    }

    /// <summary>
    /// Test FullScan discovery mode with Task-returning handlers
    /// </summary>
    private static async Task TestFullScanDiscovery()
    {
        Console.WriteLine("─────────────────────────────────────────────────────────────");
        Console.WriteLine("TEST 4: FullScan Discovery Mode");
        Console.WriteLine("─────────────────────────────────────────────────────────────");

        var config = GetConfig();
        var services = new ServiceCollection();
        services.TryAddPvNugsLoggerSeriService(config);
        
        // Use FullScan - should auto-discover Task-returning handlers
        services.TryAddPvNugsMediator(DiscoveryMode.FullScan);
        
        var sp = services.BuildServiceProvider();
        var mediator = sp.GetRequiredService<IPvNugsMediator>();

        DeleteUserHandler.Reset();
        SendEmailHandler.Reset();
        
        // Test both handlers
        await mediator.SendAsync(new DeleteUserRequest { UserId = 456 });
        await mediator.SendAsync(new SendEmailRequest 
        { 
            To = "auto@example.com", 
            Subject = "Auto-discovered",
            Body = "FullScan works!"
        });
        
        if (DeleteUserHandler.CallCount != 1 || DeleteUserHandler.LastDeletedUserId != 456)
            throw new Exception("DeleteUserHandler not auto-discovered correctly");
        
        if (SendEmailHandler.CallCount != 1 || SendEmailHandler.LastRequest?.To != "auto@example.com")
            throw new Exception("SendEmailHandler not auto-discovered correctly");
        
        Console.WriteLine("✓ FullScan auto-discovered PvNugs Task-returning handler");
        Console.WriteLine("✓ FullScan auto-discovered MediatR Task-returning handler");
        Console.WriteLine("✓ Both handlers executed successfully");
        Console.WriteLine("✓ Test Passed\n");
    }
}

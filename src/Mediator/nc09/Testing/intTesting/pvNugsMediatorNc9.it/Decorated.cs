using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using pvNugsLoggerNc9Abstractions;
using pvNugsLoggerNc9Seri;
using pvNugsMediatorNc9.it.Mediator;
using pvNugsMediatorNc9.it.PvNugs;
using pvNugsMediatorNc9Abstractions.Mediator;
using pvNugsMediatorNc9Abstractions.pvNugs;

namespace pvNugsMediatorNc9.it;

public class Decorated(ILoggerService logger)
{
    public async Task RunTestAsync()
    {
        var inMemSettings = new Dictionary<string, string>
        {
            // SERILOG
            { "PvNugsLoggerConfig:MinLogLevel", "trace" },

            // MEDIATOR
            { "PvNugsMediatorConfig:DiscoveryMode", "Decorated" }
        };

        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemSettings!)
            .Build();

        var services = new ServiceCollection();

        services.TryAddPvNugsLoggerSeriService(config);

        services.TryAddPvNugsMediator(config);

        var sp = services.BuildServiceProvider();
        var mediator = sp.GetRequiredService<IPvNugsMediator>();

        await logger.LogAsync("pvNugsMediatorNc9 Integration Testing setup complete",
            SeverityEnu.Trace);

        const string testSeparator = "========================================";

        var handlers = mediator.GetRegisteredHandlers()
            .ToList();

        await logger.LogAsync(
            $"Registered handlers: {handlers.Count}", SeverityEnu.Trace);
        foreach (var handler in handlers)
        {
            await logger.LogAsync(
                $"Handler: {handler}", SeverityEnu.Trace);
        }

        // ========================================
        // Test 1: NON-PIPELINE Request Handling
        // ========================================
        await logger.LogAsync(testSeparator, SeverityEnu.Trace);
        await logger.LogAsync(
            "TEST 1: NON-PIPELINE Request (Direct Handler)",
            SeverityEnu.Trace);
        await logger.LogAsync(testSeparator, SeverityEnu.Trace);

        var productQuery = new MdProductQueryRequest(42);
        var productInfo = await mediator.SendAsync(productQuery);

        await logger.LogAsync(
            $"Product query result: {productInfo}",
            SeverityEnu.Trace);

        // ========================================
        // Test 2: PIPELINE Request Handling
        // ========================================
        await logger.LogAsync(testSeparator, SeverityEnu.Trace);
        await logger.LogAsync(
            "TEST 2: PIPELINE Request (With Logging & Validation)",
            SeverityEnu.Trace);
        await logger.LogAsync(testSeparator, SeverityEnu.Trace);

        var mdUserCreationRequest = new MdUserCreationRequest(
            "mdTestUser", "test@gmail.com");

        await logger.LogAsync("calling mediator.SendAsync()", SeverityEnu.Trace);

        var mdUserId = await mediator.SendAsync(mdUserCreationRequest);
        await logger.LogAsync(
            $"User created with Id = {mdUserId}",
            SeverityEnu.Trace);

        if (mdUserId == Guid.Empty)
        {
            await logger.LogAsync("ERROR: Invalid userId returned", SeverityEnu.Error);
        }

        var pvUserCreationRequest = new PvUserCreationRequest(
            "pvTestUser", "test@gmail.com");
        var pvUserId = await mediator.SendAsync(pvUserCreationRequest);
        if (pvUserId == Guid.Empty)
        {
            await logger.LogAsync("ERROR: Invalid userId returned", SeverityEnu.Error);
        }
        
        // ========================================
        // Test 3: Notification Publishing
        // ========================================
        await logger.LogAsync(testSeparator, SeverityEnu.Trace);
        await logger.LogAsync(
            "TEST 3: Notification Publishing",
            SeverityEnu.Trace);
        await logger.LogAsync(testSeparator, SeverityEnu.Trace);

        var notification = new MdNotification("Some notification");

        await logger.LogAsync("Testing generic PublishAsync<T>", SeverityEnu.Trace);
        // ReSharper disable once RedundantTypeArgumentsOfMethod
        await mediator.PublishAsync<MdNotification>(notification);

        await logger.LogAsync("Testing non-generic PublishAsync", SeverityEnu.Trace);
        await mediator.PublishAsync(notification);

        await logger.LogAsync("All tests completed successfully", SeverityEnu.Trace);
    }

}
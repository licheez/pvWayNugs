using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using pvNugsLoggerNc9Abstractions;
using pvNugsLoggerNc9Seri;
using pvNugsMediatorNc9;
using pvNugsMediatorNc9.it;
using pvNugsMediatorNc9Abstractions.Mediator;
using pvNugsMediatorNc9Abstractions.pvNugs;

Console.WriteLine("Integration Testing for pvNugsMediatorNc9");

var inMemSettings = new Dictionary<string, string>
{
    // SERILOG
    { "PvNugsLoggerConfig:MinLogLevel", "trace" },
};

var config = new ConfigurationBuilder()
    .AddInMemoryCollection(inMemSettings!)
    .Build();

var services = new ServiceCollection();

services.TryAddPvNugsLoggerSeriService(config);

// Register handler with pipelines (for pipeline testing)
services.AddTransient<
    IRequestHandler<UserCreationRequest, Guid>, 
    UserCreationHandler>();
services.AddTransient<
    IPipelineBehavior<UserCreationRequest, Guid>, 
    LoggingPipeline>();
services.AddTransient<
    IPipelineBehavior<UserCreationRequest, Guid>, 
    ValidationPipeline>();

// Register handler WITHOUT pipelines (for non-pipeline testing)
services.AddTransient<
    IRequestHandler<ProductQueryRequest, string>, 
    ProductQueryHandler>();

services.AddTransient<
    INotificationHandler<Notification>, 
    MainNotificationHandler>();

services.AddTransient<
    INotificationHandler<Notification>, 
    AlternateNotificationHandler>();

services.TryAddPvNugsMediator();

var sp = services.BuildServiceProvider();
var logger = sp.GetRequiredService<IConsoleLoggerService>();
var mediator = sp.GetRequiredService<IPvNugsMediator>();

await logger.LogAsync("pvNugsMediatorNc9 Integration Testing setup complete", 
    SeverityEnu.Trace);

const string testSeparator = "========================================";

// ========================================
// Test 1: NON-PIPELINE Request Handling
// ========================================
await logger.LogAsync(testSeparator, SeverityEnu.Trace);
await logger.LogAsync(
    "TEST 1: NON-PIPELINE Request (Direct Handler)", 
    SeverityEnu.Trace);
await logger.LogAsync(testSeparator, SeverityEnu.Trace);

var productQuery = new ProductQueryRequest(42);
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

var userCreationRequest = new UserCreationRequest(
    "testUser", "test@gmail.com");


await logger.LogAsync("calling mediator.SendAsync()", SeverityEnu.Trace);

var userId = await mediator.SendAsync(userCreationRequest);

await logger.LogAsync(
    $"User created with Id = {userId}", 
    SeverityEnu.Trace);

if (userId == Guid.Empty)
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

var notification = new Notification("Some notification");

await logger.LogAsync("Testing generic PublishAsync<T>", SeverityEnu.Trace);
// ReSharper disable once RedundantTypeArgumentsOfMethod
await mediator.PublishAsync<Notification>(notification);

await logger.LogAsync("Testing non-generic PublishAsync", SeverityEnu.Trace);
await mediator.PublishAsync(notification);

await logger.LogAsync("All tests completed successfully", SeverityEnu.Trace);


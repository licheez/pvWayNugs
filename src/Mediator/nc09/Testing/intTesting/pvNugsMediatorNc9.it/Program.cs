using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using pvNugsLoggerNc9Abstractions;
using pvNugsLoggerNc9Seri;
using pvNugsMediatorNc9;
using pvNugsMediatorNc9.it;
using pvNugsMediatorNc9Abstractions;

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

services.AddTransient<
    IPvNugsMediatorRequestHandler<UserCreationRequest, Guid>, 
    UserCreationHandler>();
services.AddTransient<
    IPvNugsPipelineMediator<UserCreationRequest, Guid>, 
    LoggingPipeline>();
services.AddTransient<
    IPvNugsPipelineMediator<UserCreationRequest, Guid>, 
    ValidationPipeline>();

services.AddTransient<
    IPvNugsMediatorNotificationHandler<Notification>, 
    MainNotificationHandler>();

services.AddTransient<
    IPvNugsMediatorNotificationHandler<Notification>, 
    AlternateNotificationHandler>();

services.TryAddPvNugsMediator();

var sp = services.BuildServiceProvider();
var logger = sp.GetRequiredService<IConsoleLoggerService>();

await logger.LogAsync("pvNugsMediatorNc9 Integration Testing setup complete", 
    SeverityEnu.Trace);

var userCreationRequest = new UserCreationRequest(
    "testUser", "test@gmail.com");

var mediator = sp.GetRequiredService<IPvNugsMediator>();

await logger.LogAsync("calling mediator.SendAsync()", SeverityEnu.Trace);

var userId = await mediator.SendAsync(userCreationRequest);

await logger.LogAsync(
    $"User created with Id = {userId}", 
    SeverityEnu.Trace);

if (userId == Guid.Empty)
{
    await logger.LogAsync("ERROR: Invalid userId returned", SeverityEnu.Error);
}

var notification = new Notification("Some notification");

await logger.LogAsync("Testing generic PublishAsync<T>", SeverityEnu.Trace);
await mediator.PublishAsync<Notification>(notification);

await logger.LogAsync("Testing non-generic PublishAsync", SeverityEnu.Trace);
await mediator.PublishAsync(notification);

await logger.LogAsync("All tests completed successfully", SeverityEnu.Trace);


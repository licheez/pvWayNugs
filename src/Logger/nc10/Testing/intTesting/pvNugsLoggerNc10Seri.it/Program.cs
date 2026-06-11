using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using pvNugsLoggerNc6Abstractions;
using pvNugsLoggerNc6Seri;

Console.WriteLine("Integration console for pvNugsLoggerNc9Hybrid");

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

var sp = services.BuildServiceProvider();
var cLogger = sp.GetRequiredService<IConsoleLoggerService>();

await cLogger.LogAsync("Logging to the console with Serilog ", SeverityEnu.Trace);
await cLogger.LogAsync("Logging to the console with Serilog " /*, SeverityEnu.Debug*/);
await cLogger.LogAsync("Logging to the console with Serilog ", SeverityEnu.Info);
await cLogger.LogAsync("Logging to the console with Serilog ", SeverityEnu.Warning);
await cLogger.LogAsync("Logging to the console with Serilog ", SeverityEnu.Error);
await cLogger.LogAsync("Logging to the console with Serilog ", SeverityEnu.Fatal);

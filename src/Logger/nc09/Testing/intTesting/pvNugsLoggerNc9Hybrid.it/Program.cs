using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using pvNugsCsProviderNc9MsSql;
using pvNugsLoggerNc9Abstractions;
using pvNugsLoggerNc9Hybrid;
using pvNugsLoggerNc9MsSql;
using pvNugsLoggerNc9Seri;

Console.WriteLine("Integration console for pvNugsLoggerNc9Hybrid");

var inMemSettings = new Dictionary<string, string>
{
    // SERILOG
    { "PvNugsLoggerConfig:MinLogLevel", "trace" },
    
    // CS PROVIDER in Config mode
    // Here we mount a Docker container running postgres on port 5433
    { "PvNugsCsProviderMsSqlConfig:Rows:0:Name", "LoggingDb" },
    { "PvNugsCsProviderMsSqlConfig:Rows:0:Mode", "Config" },
    { "PvNugsCsProviderMsSqlConfig:Rows:0:Server", "Localhost" },
    { "PvNugsCsProviderMsSqlConfig:Rows:0:Schema", "dbo" },
    { "PvNugsCsProviderMsSqlConfig:Rows:0:Database", "IntTestingDb" },
    { "PvNugsCsProviderMsSqlConfig:Rows:0:Port", "1433" },
    { "PvNugsCsProviderMsSqlConfig:Rows:0:TimeoutInSeconds", "300" },
    { "PvNugsCsProviderMsSqlConfig:Rows:0:UseIntegratedSecurity", "true" },
    
    // MS SQL LOG WRITER CONFIG
    { "PvNugsMsSqlLogWriterConfig:ConnectionStringName", "LoggingDb" },
    { "PvNugsMsSqlLogWriterConfig:DefaultRetentionPeriodForTrace", "00:00:01" },
};

var config = new ConfigurationBuilder()
    .AddInMemoryCollection(inMemSettings!)
    .Build();

var services = new ServiceCollection();

services.TryAddPvNugsLoggerSeriService(config);
services.TryAddPvNugsCsProviderMsSql(config);
services.TryAddPvNugsMsSqlLogger(config);
services.TryAddPvNugsHybridLogger(config);

var sp = services.BuildServiceProvider();
var cLogger = sp.GetRequiredService<IConsoleLoggerService>();
var hLogger = sp.GetRequiredService<ILoggerService>();
var sLogger = sp.GetRequiredService<IMsSqlLoggerService>();

await cLogger.LogAsync("Logging to both the console and the db ", SeverityEnu.Trace);
await hLogger.LogAsync("Hello World", SeverityEnu.Trace);
await cLogger.LogAsync("Done", SeverityEnu.Trace);

await cLogger.LogAsync("Sleeping 1 second", SeverityEnu.Trace);
await Task.Delay(1000);

await cLogger.LogAsync("Purging", SeverityEnu.Trace);
var nbRowsPurged = await sLogger.PurgeLogsAsync();
await cLogger.LogAsync($"{nbRowsPurged} rows() purged", SeverityEnu.Trace);
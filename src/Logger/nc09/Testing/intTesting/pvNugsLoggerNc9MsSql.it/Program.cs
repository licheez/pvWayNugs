using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using pvNugsCsProviderNc9MsSql;
using pvNugsLoggerNc9Abstractions;
using pvNugsLoggerNc9MsSql;
using pvNugsLoggerNc9Seri;

Console.WriteLine("Integration testing console for pvNugsLoggerNc9MsSql");

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

var sp = services.BuildServiceProvider();
var logger = sp.GetRequiredService<IConsoleLoggerService>();

var svc = sp.GetRequiredService<IMsSqlLoggerService>();

await logger.LogAsync("Logging into the Db", SeverityEnu.Trace);
await svc.LogAsync("Hello World", SeverityEnu.Trace);
await logger.LogAsync("Done", SeverityEnu.Trace);

await logger.LogAsync("Sleeping 1 second", SeverityEnu.Trace);
await Task.Delay(1000);

await logger.LogAsync("Purging", SeverityEnu.Trace);
var nbRowsPurged = await svc.PurgeLogsAsync();
await logger.LogAsync($"{nbRowsPurged} rows() purged", SeverityEnu.Trace);

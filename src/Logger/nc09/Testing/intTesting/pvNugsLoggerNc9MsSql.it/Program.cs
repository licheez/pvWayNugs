using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using pvNugsCsProviderNc9Abstractions;
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
    { "PvNugsCsProviderMsSqlConfig:Mode", "Config" },
    { "PvNugsCsProviderMsSqlConfig:Server", "Localhost" },
    { "PvNugsCsProviderMsSqlConfig:Schema", "dbo" },
    { "PvNugsCsProviderMsSqlConfig:Database", "IntTestingDb" },
    { "PvNugsCsProviderMsSqlConfig:Port", "1433" },
    { "PvNugsCsProviderMsSqlConfig:TimeoutInSeconds", "300" },
    { "PvNugsCsProviderMsSqlConfig:UseIntegratedSecurity", "true" },
    
    // MS SQL LOGGER CONFIG
    // use all default values
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

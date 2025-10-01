using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using pvNugsCsProviderNc9MsSql;
using pvNugsLoggerNc9Abstractions;
using pvNugsLoggerNc9Seri;
using pvNugsSemaphoreNc9Abstractions;
using pvNugsSemaphoreNc9MsSql;

Console.WriteLine("Integration testing console for pvNugsSemaphoreNc9MsSql");

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
    
    // MS SQL SEMAPHORE CONFIG
    { "PvNugsMsSqlSemaphoreConfig:ConnectionStringName", "LoggingDb" },
    { "PvNugsMsSqlSemaphoreConfig:TableName", "MySemaphore" },
    { "PvNugsMsSqlSemaphoreConfig:CreateTableAtFirstUse", "true" }
};

var config = new ConfigurationBuilder()
    .AddInMemoryCollection(inMemSettings!)
    .Build();

var services = new ServiceCollection();

services.TryAddPvNugsLoggerSeriService(config);
services.TryAddPvNugsCsProviderMsSql(config);
services.TryAddPvNugsMsSqlSemaphore(config);

var sp = services.BuildServiceProvider();
var logger = sp.GetRequiredService<IConsoleLoggerService>();
var svc = sp.GetRequiredService<IPvNugsSemaphoreService>();

const string theMutex = "MyUniqueMutex";

var si = await svc.AcquireSemaphoreAsync(
    theMutex, Environment.MachineName, TimeSpan.FromSeconds(10));
await logger.LogAsync(si.ToString()!);

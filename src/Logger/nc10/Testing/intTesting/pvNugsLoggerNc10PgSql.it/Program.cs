using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using pvNugsCsProviderNc10Abstractions;
using pvNugsLoggerNc10Abstractions;
using pvNugsLoggerNc10PgSql;
using pvNugsLoggerNc10PgSql.it;
using pvNugsLoggerNc10Seri;

Console.WriteLine("Integration console for pvNugsLoggerNc10PgSql");

var inMemSettings = new Dictionary<string, string>
{
    // SERILOG
    { "PvNugsLoggerConfig:MinLogLevel", "trace" },
    
    // PG_SQL_LOGGER
    //{ "PvNugsPgSqlLogWriterConfig:ConnectionStringName", "TestBench" },
    { "PvNugsPgSqlLogWriterConfig:Schema", "int-testing" },
    { "PvNugsPgSqlLogWriterConfig:TableName", "logs" }
};

var config = new ConfigurationBuilder()
    .AddInMemoryCollection(inMemSettings!)
    .Build();

var services = new ServiceCollection();

services.AddTransient<IPvNugsPgSqlCsProvider, TestBenchCsProvider>();

services.TryAddPvNugsLoggerSeriService(config)
    .TryAddPvNugsPgSqlLoggerNc10PgSql(config);

var sp = services.BuildServiceProvider();
var cLogger = sp.GetRequiredService<IConsoleLoggerService>();
await cLogger.LogAsync("Logging to the console with Serilog ", SeverityEnu.Trace);

var pgLogger = sp.GetRequiredService<IPgSqlLoggerService>();

await pgLogger.LogAsync("Logging to the console with Serilog ", SeverityEnu.Trace);


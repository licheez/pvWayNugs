using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using pvNugsCsProviderNc10Abstractions;
using pvNugsCsProviderNc10PgSql;
using pvNugsCsProviderNc10PgSql.it;
using pvNugsLoggerNc10Abstractions;
using pvNugsLoggerNc10Seri;
using pvNugsSecretManagerNc10Abstractions;

Console.WriteLine("Integration Testing Console for CsProvider using testSecretManager");

var inMemSettings = new Dictionary<string, string>
{
    // SERILOG
    { "PvNugsLoggerConfig:MinLogLevel", "trace" },
    
    // PG SQL CS PROVIDER
    { "PvNugsCsProviderPgSqlConfig:Mode", "DynamicSecret" },
    { "PvNugsCsProviderPgSqlConfig:Server", "Localhost" },
    { "PvNugsCsProviderPgSqlConfig:Schema", "int_testing_db" },
    { "PvNugsCsProviderPgSqlConfig:Database", "postgres" },
    { "PvNugsCsProviderPgSqlConfig:Port", "5432" },
    { "PvNugsCsProviderPgSqlConfig:Timezone", "UTC" },
    { "PvNugsCsProviderPgSqlConfig:TimeoutInSeconds", "300" },
    { "PvNugsCsProviderPgSqlConfig:OwnerSecretParams:mountPoint", "database/postgres/pg5432" },
    { "PvNugsCsProviderPgSqlConfig:OwnerSecretParams:role", "owner" },
};

var config = new ConfigurationBuilder()
    .AddInMemoryCollection(inMemSettings!)
    .Build();

var services = new ServiceCollection();

services
    .TryAddPvNugsLoggerSeriService(config)
    .TryAddPvNugsCsProviderPgSql(config)
    .TryAddTransient<IPvNugsSecretManager, TestSecretManager>();
    
var sp = services.BuildServiceProvider();

var logger = sp.GetRequiredService<IConsoleLoggerService>();
var csp = sp.GetRequiredService<IPvNugsPgSqlCsProvider>();

await logger.LogAsync(
    "Starting integration tests for CsProvider using test secretManager", 
    SeverityEnu.Trace);
    
var cs = await csp.GetConnectionStringAsync(
    CsProviderSqlRoleEnu.Owner);
await logger.LogAsync(
    $"Retrieved connection string: {cs}", 
    SeverityEnu.Trace);
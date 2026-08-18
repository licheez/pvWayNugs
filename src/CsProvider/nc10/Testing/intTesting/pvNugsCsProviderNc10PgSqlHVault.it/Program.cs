using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using pvNugsCacheNc10Memory;
using pvNugsCsProviderNc10Abstractions;
using pvNugsCsProviderNc10PgSql;
using pvNugsLoggerNc10Abstractions;
using pvNugsLoggerNc10Seri;
using pvNugsSecretManagerNc10;
using pvNugsSecretManagerNc10ProviderHVault;

Console.WriteLine("Integration Testing Console for CsProvider using HVault infra .NET 10");

var inMemSettings = new Dictionary<string, string>
{
    // SERILOG
    { "PvNugsLoggerConfig:MinLogLevel", "trace" },
    
    // CACHE
    { "PvNugsCacheConfig:TimeToLive", "00:00:10" },
    
    // SECRET MANAGER
    { "PvNugsSecretManagerConfig:CacheKeyPrefix", "MyCache"},
    { "PvNugsSecretManagerConfig:CacheTimeToLive", "00:00:05"},
    
    // H_VAULT CONFIG
    { "PvNugsHVaultSecretProviderConfig:AuthMethod", "TokenAuth"},
    { "PvNugsHVaultSecretProviderConfig:Token", "dev-only-token"},
    { "PvNugsHVaultSecretProviderConfig:ServerUrl", "http://localhost:8200"},
    { "PvNugsHVaultSecretProviderConfig:ExpirationErrorToleranceInMinutes", "1"},
    
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

services.TryAddPvNugsLoggerSeriService(config)
    .TryAddPvNugsCacheMemory(config)
    .TryAddPvNugsHVaultSecretProvider(config)
    .TryAddPvNugsSecretManager(config)
    .TryAddPvNugsCsProviderPgSql(config);

var sp = services.BuildServiceProvider();

var logger = sp.GetRequiredService<ILoggerService>();

var svc = sp.GetRequiredService<IPvNugsCsProvider>();

await logger.LogAsync("getting connection string for Owner role", SeverityEnu.Trace);

var connStr = await svc.GetConnectionStringAsync(CsProviderSqlRoleEnu.Owner);

await logger.LogAsync($"connection string for Owner role: {connStr}", SeverityEnu.Trace);

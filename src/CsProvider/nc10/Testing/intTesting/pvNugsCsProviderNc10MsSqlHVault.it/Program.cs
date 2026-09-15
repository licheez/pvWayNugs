using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using pvNugsCacheNc10Memory;
using pvNugsCsProviderNc10Abstractions;
using pvNugsCsProviderNc10MsSql;
using pvNugsLoggerNc10Abstractions;
using pvNugsLoggerNc10Seri;
using pvNugsSecretManagerNc10;
using pvNugsSecretManagerNc10ProviderHVault;

Console.WriteLine("Integration Testing Console for Ms SQL CsProvider using HVault infra .NET 10");

// use the HVaultLab integration console for
// mounting the HVault server and creating the database/mssql/ms1433 role

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
    
    // MS SQL CS PROVIDER
    { "PvNugsCsProviderMsSqlConfig:Mode", "DynamicSecret" },
    { "PvNugsCsProviderMsSqlConfig:Server", "Localhost" },
    { "PvNugsCsProviderMsSqlConfig:Database", "MyDatabase" },
    { "PvNugsCsProviderMsSqlConfig:Port", "1433" },
    { "PvNugsCsProviderMsSqlConfig:TimeoutInSeconds", "300" },
    { "PvNugsCsProviderMsSqlConfig:OwnerSecretParams:mountPoint", "database/mssql/ms1433" },
    { "PvNugsCsProviderMsSqlConfig:OwnerSecretParams:role", "owner" },
};

var config = new ConfigurationBuilder()
    .AddInMemoryCollection(inMemSettings!)
    .Build();

var services = new ServiceCollection();

services.TryAddPvNugsLoggerSeriService(config)
    .TryAddPvNugsCacheMemory(config)
    .TryAddPvNugsHVaultSecretProvider(config)
    .TryAddPvNugsSecretManager(config)
    .TryAddPvNugsCsProviderMsSql(config);   

var sp = services.BuildServiceProvider();

var logger = sp.GetRequiredService<ILoggerService>();

var svc = sp.GetRequiredService<IPvNugsCsProvider>();

await logger.LogAsync("getting connection string for Owner role", SeverityEnu.Trace);

var connStr = await svc.GetConnectionStringAsync(CsProviderSqlRoleEnu.Owner);

await logger.LogAsync($"connection string for Owner role: {connStr}", SeverityEnu.Trace);

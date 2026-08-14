using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using pvNugsCacheNc10Memory;
using pvNugsLoggerNc10Abstractions;
using pvNugsLoggerNc10Seri;
using pvNugsSecretManagerNc10;
using pvNugsSecretManagerNc10Abstractions;
using pvNugsSecretManagerNc10ProviderHVault;

Console.WriteLine("Integration Testing Console for HashiCorp Vault SecretProvider .NET 10");

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
    { "PvNugsHVaultSecretProviderConfig:TokenFilePath", "token.txt"},
    { "PvNugsHVaultSecretProviderConfig:ServerUrl", "http://localhost:8200"}
    
};

var config = new ConfigurationBuilder()
    .AddInMemoryCollection(inMemSettings!)
    .Build();

var services = new ServiceCollection();

services.TryAddPvNugsLoggerSeriService(config)
    .TryAddPvNugsCacheMemory(config)
    .TryAddPvNugsSecretManager(config)
    .TryAddPvNugsHVaultSecretProvider(config);

var sp = services.BuildServiceProvider();

var logger = sp.GetRequiredService<ILoggerService>();
var svc = sp.GetRequiredService<IPvNugsSecretManager>();

var statParameters = PvNugsHVaultSecretProviderParameters.CreateStaticParameters(
    mountPoint: "secret", 
    path: "secret-manager-test",
    secretName: "api-key");
Console.WriteLine("Retrieving Static Secrets from PvNugsSecretManager");
var statSecret = await svc.GetStaticSecretAsync(statParameters);
if (statSecret == null) return;

await logger.LogAsync($"Static Secret Retrieved: {statSecret}", SeverityEnu.Trace);

// Retrieve the dynamic secret from HashiCorp Vault
var dynParameters = PvNugsHVaultSecretProviderParameters.CreateDynamicParameters(
    mountPoint: "database/postgres/pg5432",
    roleName: "owner");

Console.WriteLine($"Retrieving Dynamic Secret from HashiCorp Vault with parameters: " +
                  $"{string.Join(", ", 
                      dynParameters.Select(kv => $"{kv.Key}='{kv.Value}'"))}");

var dynSecret = await svc.GetDynamicSecretAsync(dynParameters);
if (dynSecret == null) return;

await logger.LogAsync(
    $"Dynamic Secret Retrieved: {dynSecret.Username} " +
    $"/ {dynSecret.Password} " +
    $"/ {dynSecret.ExpirationDateUtc}", 
    SeverityEnu.Trace);

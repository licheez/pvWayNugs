using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using pvNugsCacheNc10Memory;
using pvNugsLoggerNc10Abstractions;
using pvNugsLoggerNc10Seri;
using pvNugsSecretManagerNc10;
using pvNugsSecretManagerNc10Abstractions;
using pvNugsSecretManagerNc10ProviderAzure;

Console.WriteLine("Integration Testing Console for AzureSecretProvider .NET 10");

var inMemSettings = new Dictionary<string, string>
{
    // SERILOG
    { "PvNugsLoggerConfig:MinLogLevel", "trace" },
    
    // CACHE
    { "PvNugsCacheConfig:TimeToLive", "00:00:10" },
    
    // SECRET MANAGER
    { "PvNugsSecretManagerConfig:CacheKeyPrefix", "MyCache"},
    { "PvNugsSecretManagerConfig:CacheTimeToLive", "00:00:05"}
    
    // AZURE CONFIG
    // see environment variables for Azure Key Vault configuration
};

var config = new ConfigurationBuilder()
    .AddInMemoryCollection(inMemSettings!)
    // Expecting the following environment variable:
    //  PvNugsAzureSecretProviderConfig__KeyVaultUrl
    //  PvNugsAzureSecretProviderConfig__Credential__TenantId
    //  PvNugsAzureSecretProviderConfig__Credential__ClientId
    //  PvNugsAzureSecretProviderConfig__Credential__ClientSecret
    .AddEnvironmentVariables()
    .Build();

var services = new ServiceCollection();

services.TryAddPvNugsLoggerSeriService(config)
    .TryAddPvNugsCacheMemory(config)
    .TryAddPvNugsSecretManager(config)
    .TryAddPvNugsAzureSecretProvider(config);

var sp = services.BuildServiceProvider();

var logger = sp.GetRequiredService<ILoggerService>();
var svc = sp.GetRequiredService<IPvNugsSecretManager>();

var parameters = PvNugsAzureSecretProviderParameters
    .CreateParameters("MyFirstSecretName");

var secret = await svc.GetStaticSecretAsync(parameters);
await logger.LogAsync($"Secret: {secret}");
await logger.LogAsync("Second call");
secret = await svc.GetStaticSecretAsync(parameters);
await logger.LogAsync($"Secret: {secret}");

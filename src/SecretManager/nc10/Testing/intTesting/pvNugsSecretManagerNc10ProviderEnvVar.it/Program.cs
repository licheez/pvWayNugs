using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using pvNugsCacheNc10Memory;
using pvNugsLoggerNc10Abstractions;
using pvNugsLoggerNc10Seri;
using pvNugsSecretManagerNc10;
using pvNugsSecretManagerNc10Abstractions;
using pvNugsSecretManagerNc10ProviderEnvironment;

Console.WriteLine("Integration Testing Console for EnvVarSecretProvider .NET 10");

var inMemSettings = new Dictionary<string, string>
{
    // SERILOG
    { "PvNugsLoggerConfig:MinLogLevel", "trace" },
    
    // CACHE
    { "PvNugsCacheConfig:TimeToLive", "00:00:10" },
    
    // SECRET MANAGER
    { "PvNugsSecretManagerConfig:CacheKeyPrefix", "MyCache"},
    { "PvNugsSecretManagerConfig:CacheTimeToLive", "00:00:05"},
    
    // ENV_VAR_PROVIDER CONFIG
    { "PvNugsEnvVarSecretProviderConfig:Prefix", "EnvVarSecretProvider"},
    
    // ENV VAR SIMULATION
    {"EnvVarSecretProvider:MyNonSecretName","MyNonSecretValue"}
};

var config = new ConfigurationBuilder()
    .AddInMemoryCollection(inMemSettings!)
    .Build();

var services = new ServiceCollection();

services.TryAddPvNugsLoggerSeriService(config)
    .TryAddPvNugsCacheMemory(config)
    .TryAddPvNugsSecretManager(config)
    .TryAddPvNugsEnvVarSecretProvider(config);

services.TryAddSingleton<IConfiguration>(_ => config);

var sp = services.BuildServiceProvider();

var logger = sp.GetRequiredService<ILoggerService>();
var svc = sp.GetRequiredService<IPvNugsSecretManager>();

var parameters = PvNugsEnvVarSecretProviderParameters
    .CreateParameters("MyNonSecretName");

var secret = await svc.GetStaticSecretAsync(parameters);
await logger.LogAsync($"Secret: {secret}");
await logger.LogAsync("Second call");
secret = await svc.GetStaticSecretAsync(parameters);
await logger.LogAsync($"Secret: {secret}");
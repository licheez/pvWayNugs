using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using pvNugsCacheNc9Local;
using pvNugsLoggerNc9Abstractions;
using pvNugsLoggerNc9Seri;
using pvNugsSecretManagerNc9Abstractions;
using pvNugsSecretManagerNc9Azure;

Console.WriteLine("Integration Testing Console for pvNugsSecretManagerNc9Azure");

var inMemSettings = new Dictionary<string, string>
{
    // SERILOG
    { "PvNugsLoggerConfig:MinLogLevel", "trace" },
};

var config = new ConfigurationBuilder()
    .AddInMemoryCollection(inMemSettings!)
    // Expecting the following environment variable:
    //  PvNugsAzureSecretManagerConfig__KeyVaultUrl
    //  PvNugsAzureSecretManagerConfig__Credential__TenantId
    //  PvNugsAzureSecretManagerConfig__Credential__ClientId
    //  PvNugsAzureSecretManagerConfig__Credential__ClientSecret
    .AddEnvironmentVariables()
    .Build();

var services = new ServiceCollection();

services.TryAddPvNugsLoggerSeriService(config)
    .TryAddPvNugsCacheNc9Local(config)
    .TryAddPvNugsAzureSecretManager(config);

var sp = services.BuildServiceProvider();

var logger = sp.GetRequiredService<ILoggerService>();
var svc = sp.GetRequiredService<IPvNugsStaticSecretManager>();

await logger.LogAsync("Retrieving secret...");
var secret = await svc.GetStaticSecretAsync("MyFirstSecretName");
await logger.LogAsync($"Secret: {secret}");
await logger.LogAsync("Second call");
secret = await svc.GetStaticSecretAsync("MyFirstSecretName");
await logger.LogAsync($"Secret: {secret}");

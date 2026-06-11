using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using pvNugsCacheNc10Abstractions;
using pvNugsCacheNc10Memory;
using pvNugsLoggerNc10Abstractions;
using pvNugsLoggerNc10Seri;

Console.WriteLine("Integration console for pvNugsCacheNc10Memory");

var inMemSettings = new Dictionary<string, string>
{
    // SERILOG
    { "PvNugsLoggerConfig:MinLogLevel", "trace" },
    
    // CACHE
    { "PvNugsCacheConfig:DefaultTimeToLive", "00:00:03" }
};

var config = new ConfigurationBuilder()
    .AddInMemoryCollection(inMemSettings!)
    .Build();

var services = new ServiceCollection();

services.TryAddPvNugsLoggerSeriService(config);
services.TryAddPvNugsCacheMemory(config);

var sp = services.BuildServiceProvider();
var cLogger = sp.GetRequiredService<IConsoleLoggerService>();

var memCache = sp.GetRequiredService<IPvNugsCache>();

const string key = "MyKey";
const string value = "MyValue";

await cLogger.LogAsync("Creating a new cache entry with a time to live of 2 seconds");
await memCache.SetAsync(key, value, TimeSpan.FromSeconds(1));

await cLogger.LogAsync("Retrieving the cached value");
var rVal = await memCache.GetAsync<string>(key);
await cLogger.LogAsync($"retrieved value is : '{rVal}'");

Thread.Sleep(2000);
rVal = await memCache.GetAsync<string>(key);
await cLogger.LogAsync($"retrieved value is : '{rVal}'");

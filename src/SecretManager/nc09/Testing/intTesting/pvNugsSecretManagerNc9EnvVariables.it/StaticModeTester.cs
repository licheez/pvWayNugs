using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using pvNugsLoggerNc9Abstractions;
using pvNugsLoggerNc9Seri;
using pvNugsSecretManagerNc9Abstractions;

namespace pvNugsSecretManagerNc9EnvVariables.it;

public static class StaticModeTester
{
    public static  async Task RunAsync()
    {
        var inMemSettings = new Dictionary<string, string>
        {
            // SERILOG
            { "PvNugsLoggerConfig:MinLogLevel", "trace" },
            
            // ENV VARIABLES SECRET MANAGER
            { "PvNugsSecretManagerEnvVariablesConfig:Prefix", "intTesting" },
        };

        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemSettings!)
            // expecting the following environment variables:
            //   intTesting__MyPgSqlStaticPassword-Reader
            .AddEnvironmentVariables()
            .Build();

        var services = new ServiceCollection();

        services.TryAddPvNugsLoggerSeriService(config);
        services.TryAddPvNugsEnvVariablesStaticSecretManager(config);

        var sp = services.BuildServiceProvider();
        var logger = sp.GetRequiredService<IConsoleLoggerService>();
        
        var svc = sp.GetRequiredService<IPvNugsStaticSecretManager>();

        var secret = await svc.GetStaticSecretAsync("MyPgSqlStaticPassword-Reader");

        await logger.LogAsync($"secret:{secret}", SeverityEnu.Trace);
    }    
}
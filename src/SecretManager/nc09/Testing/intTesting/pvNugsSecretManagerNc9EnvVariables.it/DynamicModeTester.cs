using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using pvNugsLoggerNc9Abstractions;
using pvNugsLoggerNc9Seri;
using pvNugsSecretManagerNc9Abstractions;

namespace pvNugsSecretManagerNc9EnvVariables.it;

public static class DynamicModeTester
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
            // Expecting the following environment variables:
            //   intTesting__MyPgSqlDynamicCredential-Reader__Username
            //   intTesting__MyPgSqlDynamicCredential-Reader__Password
            //   intTesting__MyPgSqlDynamicCredential-Reader__ExpirationDateUtc
            .AddEnvironmentVariables()
            .Build();

        var services = new ServiceCollection();

        services.TryAddPvNugsLoggerSeriService(config);
        services.TryAddPvNugsSecretManagerEnvVariables(config);

        var sp = services.BuildServiceProvider();
        var logger = sp.GetRequiredService<IConsoleLoggerService>();
        
        var svc = sp.GetRequiredService<IPvNugsDynamicSecretManager>();

        var secret =
            await svc.GetDynamicSecretAsync("MyPgSqlDynamicCredential-Reader");

        await logger.LogAsync(
            $"{secret?.Username} " +
            $"{secret?.Password} " +
            $"{secret?.ExpirationDateUtc}", SeverityEnu.Trace);
    }    
}
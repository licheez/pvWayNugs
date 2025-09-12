using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using pvNugsCsProviderNc9Abstractions;
using pvNugsLoggerNc9Abstractions;
using pvNugsLoggerNc9Seri;
using pvNugsSecretManagerNc9EnvVariables;

namespace pvNugsCsProviderNc9PgSql.it;

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
    
            // CS PROVIDER in Config mode
            // Here we mount a Docker container running postgres on port 5433
            { "PvNugsCsProviderPgSqlConfig:Mode", "StaticSecret" },
            { "PvNugsCsProviderPgSqlConfig:Server", "Localhost" },
            { "PvNugsCsProviderPgSqlConfig:Schema", "int_testing_db" },
            { "PvNugsCsProviderPgSqlConfig:Database", "postgres" },
            { "PvNugsCsProviderPgSqlConfig:Port", "5433" },
            { "PvNugsCsProviderPgSqlConfig:Timezone", "UTC" },
            { "PvNugsCsProviderPgSqlConfig:TimeoutInSeconds", "300" },
            { "PvNugsCsProviderPgSqlConfig:Username", "postgres" },
            { "PvNugsCsProviderPgSqlConfig:SecretName", "MyPgSqlStaticPassword" },
        };

        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemSettings!)
            // expecting the following environment variables:
            //   intTesting__MyPgSqlStaticPassword-Reader
            .AddEnvironmentVariables()
            .Build();

        var services = new ServiceCollection();

        services.TryAddPvNugsLoggerSeriService(config);
        services.TryAddPvNugsSecretManagerEnvVariables(config);
        services.TryAddPvNugsCsProviderPgSql(config);

        var sp = services.BuildServiceProvider();
        var logger = sp.GetRequiredService<IConsoleLoggerService>();
        
        var svc = sp.GetRequiredService<IPvNugsCsProvider>();

        var cs = await svc.GetConnectionStringAsync();

        await logger.LogAsync(cs, SeverityEnu.Trace);
    }    
}
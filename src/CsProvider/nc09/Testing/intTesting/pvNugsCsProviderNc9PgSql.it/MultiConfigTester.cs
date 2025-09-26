using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using pvNugsCsProviderNc9Abstractions;
using pvNugsLoggerNc9Abstractions;
using pvNugsLoggerNc9Seri;

namespace pvNugsCsProviderNc9PgSql.it;

public static class MultiConfigTester
{
    public static  async Task RunAsync()
    {
        const string postgres = "Postgres";
        var inMemSettings = new Dictionary<string, string>
        {
            // SERILOG
            { "PvNugsLoggerConfig:MinLogLevel", "trace" },
    
            // CS PROVIDER in Config mode
            { "PvNugsCsProviderPgSqlConfig:Rows:0:Name", "MainDb" },
            { "PvNugsCsProviderPgSqlConfig:Rows:0:Mode", "Config" },
            { "PvNugsCsProviderPgSqlConfig:Rows:0:Server", "Localhost" },
            { "PvNugsCsProviderPgSqlConfig:Rows:0:Schema", "main_db" },
            { "PvNugsCsProviderPgSqlConfig:Rows:0:Database", postgres },
            { "PvNugsCsProviderPgSqlConfig:Rows:0:Port", "5433" },
            { "PvNugsCsProviderPgSqlConfig:Rows:0:Timezone", "UTC" },
            { "PvNugsCsProviderPgSqlConfig:Rows:0:TimeoutInSeconds", "300" },
            { "PvNugsCsProviderPgSqlConfig:Rows:0:Username", postgres },
            
            { "PvNugsCsProviderPgSqlConfig:Rows:1:Name", "AltDb" },
            { "PvNugsCsProviderPgSqlConfig:Rows:1:Mode", "Config" },
            { "PvNugsCsProviderPgSqlConfig:Rows:1:Server", "Localhost" },
            { "PvNugsCsProviderPgSqlConfig:Rows:1:Schema", "alt_db" },
            { "PvNugsCsProviderPgSqlConfig:Rows:1:Database", postgres },
            { "PvNugsCsProviderPgSqlConfig:Rows:1:Port", "5433" },
            { "PvNugsCsProviderPgSqlConfig:Rows:1:Timezone", "UTC" },
            { "PvNugsCsProviderPgSqlConfig:Rows:1:TimeoutInSeconds", "100" },
            { "PvNugsCsProviderPgSqlConfig:Rows:1:Username", postgres },
        };

        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemSettings!)
            .Build();

        var services = new ServiceCollection();

        services.TryAddPvNugsLoggerSeriService(config);
        services.TryAddPvNugsCsProviderPgSql(config);

        var sp = services.BuildServiceProvider();
        var logger = sp.GetRequiredService<IConsoleLoggerService>();
        
        var svc = sp.GetRequiredService<IPvNugsCsProvider>();

        var mainCs = await svc.GetConnectionStringAsync("MainDb");
        await logger.LogAsync(mainCs, SeverityEnu.Trace);
        var altCs = await svc.GetConnectionStringAsync("AltDb");
        await logger.LogAsync(altCs, SeverityEnu.Trace);
        
    }    
}
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using pvNugsCsProviderNc9Abstractions;
using pvNugsLoggerNc9Abstractions;
using pvNugsLoggerNc9Seri;

namespace pvNugsCsProviderNc9MsSql.it;

public static class MultiConfigTester
{
    public static  async Task RunAsync()
    {
        var inMemSettings = new Dictionary<string, string>
        {
            // SERILOG
            { "PvNugsLoggerConfig:MinLogLevel", "trace" },
    
            // CS PROVIDER in Config mode
            { "PvNugsCsProviderMsSqlConfig:Rows:0:Name", "MainDb" },
            { "PvNugsCsProviderMsSqlConfig:Rows:0:Mode", "Config" },
            { "PvNugsCsProviderMsSqlConfig:Rows:0:Server", "Localhost" },
            { "PvNugsCsProviderMsSqlConfig:Rows:0:Schema", "dbo" },
            { "PvNugsCsProviderMsSqlConfig:Rows:0:Database", "MainDb" },
            { "PvNugsCsProviderMsSqlConfig:Rows:0:Port", "1433" },
            { "PvNugsCsProviderMsSqlConfig:Rows:0:TimeoutInSeconds", "300" },
            { "PvNugsCsProviderMsSqlConfig:Rows:0:UseIntegratedSecurity", "true" },
            { "PvNugsCsProviderMsSqlConfig:Rows:1:Name", "AltDb" },
            { "PvNugsCsProviderMsSqlConfig:Rows:1:Mode", "Config" },
            { "PvNugsCsProviderMsSqlConfig:Rows:1:Server", "Localhost" },
            { "PvNugsCsProviderMsSqlConfig:Rows:1:Schema", "dbo" },
            { "PvNugsCsProviderMsSqlConfig:Rows:1:Database", "AltDb" },
            { "PvNugsCsProviderMsSqlConfig:Rows:1:Port", "1433" },
            { "PvNugsCsProviderMsSqlConfig:Rows:1:TimeoutInSeconds", "300" },
            { "PvNugsCsProviderMsSqlConfig:Rows:1:UserName", "TecA1546Sq" },
            { "PvNugsCsProviderMsSqlConfig:Rows:1:Password", "SomeSecret" },
        };

        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemSettings!)
            .Build();

        var services = new ServiceCollection();

        services.TryAddPvNugsLoggerSeriService(config);
        services.TryAddPvNugsCsProviderMsSql(config);

        var sp = services.BuildServiceProvider();
        var logger = sp.GetRequiredService<IConsoleLoggerService>();
        
        var svc = sp.GetRequiredService<IPvNugsCsProvider>();

        var mainCs = await svc.GetConnectionStringAsync("MainDb");
        await logger.LogAsync(mainCs, SeverityEnu.Trace);
        var altCs = await svc.GetConnectionStringAsync("AltDb");
        await logger.LogAsync(altCs, SeverityEnu.Trace);


    }    
}
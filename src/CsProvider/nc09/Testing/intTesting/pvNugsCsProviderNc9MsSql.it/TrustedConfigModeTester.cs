using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using pvNugsCsProviderNc9Abstractions;
using pvNugsLoggerNc9Abstractions;
using pvNugsLoggerNc9Seri;

namespace pvNugsCsProviderNc9MsSql.it;

public static class TrustedConfigModeTester
{
    public static  async Task RunAsync()
    {
        var inMemSettings = new Dictionary<string, string>
        {
            // SERILOG
            { "PvNugsLoggerConfig:MinLogLevel", "trace" },
    
            // CS PROVIDER in Config mode
            { "PvNugsCsProviderMsSqlConfig:Mode", "Config" },
            { "PvNugsCsProviderMsSqlConfig:Server", "Localhost" },
            { "PvNugsCsProviderMsSqlConfig:Schema", "dbo" },
            { "PvNugsCsProviderMsSqlConfig:Database", "IntTestingDb" },
            { "PvNugsCsProviderMsSqlConfig:Port", "1433" },
            { "PvNugsCsProviderMsSqlConfig:TimeoutInSeconds", "300" },
            { "PvNugsCsProviderMsSqlConfig:UseIntegratedSecurity", "true" },
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

        var cs = await svc.GetConnectionStringAsync();

        await logger.LogAsync(cs, SeverityEnu.Trace);

        await using var cn = new SqlConnection(cs);
        
        await logger.LogAsync("opening the connection", SeverityEnu.Trace);
        await cn.OpenAsync();
        
        Thread.Sleep(1000);
        
        await logger.LogAsync("closing the connection", SeverityEnu.Trace);
        await cn.CloseAsync();
    }    
}
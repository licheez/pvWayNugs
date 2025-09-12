using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using pvNugsCsProviderNc9Abstractions;
using pvNugsLoggerNc9Abstractions;
using pvNugsLoggerNc9Seri;

namespace pvNugsCsProviderNc9PgSql.it;

public static class ConfigModeTester
{
    public static  async Task RunAsync()
    {
        var inMemSettings = new Dictionary<string, string>
        {
            // SERILOG
            { "PvNugsLoggerConfig:MinLogLevel", "trace" },
    
            // CS PROVIDER in Config mode
            // Here we mount a Docker container running postgres on port 5433
            { "PvNugsCsProviderPgSqlConfig:Mode", "Config" },
            { "PvNugsCsProviderPgSqlConfig:Server", "Localhost" },
            { "PvNugsCsProviderPgSqlConfig:Schema", "int_testing_db" },
            { "PvNugsCsProviderPgSqlConfig:Database", "postgres" },
            { "PvNugsCsProviderPgSqlConfig:Port", "5433" },
            { "PvNugsCsProviderPgSqlConfig:Timezone", "UTC" },
            { "PvNugsCsProviderPgSqlConfig:TimeoutInSeconds", "300" },
            { "PvNugsCsProviderPgSqlConfig:Username", "postgres" },
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

        var cs = await svc.GetConnectionStringAsync();

        await logger.LogAsync(cs, SeverityEnu.Trace);

        await using var cn = new NpgsqlConnection(cs);

        await logger.LogAsync("opening the connection", SeverityEnu.Trace);
        await cn.OpenAsync();

        Thread.Sleep(1000);

        await logger.LogAsync("closing the connection", SeverityEnu.Trace);
        await cn.CloseAsync();
    }    
}
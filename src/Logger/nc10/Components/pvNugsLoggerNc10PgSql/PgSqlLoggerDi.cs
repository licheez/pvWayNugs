using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using pvNugsLoggerNc10Abstractions;

namespace pvNugsLoggerNc10PgSql;

public static class PgSqlLoggerDi
{
    public static IServiceCollection TryAddPgSqlLoggerNc10PgSql(
        this IServiceCollection services,
        IConfiguration config)
    {
        services.Configure<PvNugsPgSqlLogWriterConfig>(
            config.GetSection(PvNugsPgSqlLogWriterConfig.Section));

        services.TryAddSingleton<
            IPgSqlLogWriter, PgSqlLogWriter>();
        services.TryAddSingleton<ISqlLogWriter>(sp =>
            sp.GetRequiredService<IPgSqlLogWriter>());

        services.TryAddSingleton<IPgSqlLoggerService, PgSqlLoggerService>();
        return services;
    }
}
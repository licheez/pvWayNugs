using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using pvNugsLoggerNc10Abstractions;

namespace pvNugsLoggerNc10PgSql;

/// <summary>
/// Provides dependency injection extension methods for registering PostgreSQL-backed logging services.
/// </summary>
/// <remarks>
/// <para>
/// This static class centralizes the registration of the PostgreSQL log writer, the logger factory,
/// and the concrete logger service abstractions used throughout the application.
/// </para>
/// <para>
/// The method uses the <c>TryAdd...</c> pattern so repeated calls do not register duplicate singleton
/// implementations, which makes the registration safe to invoke from multiple bootstrap locations.
/// </para>
/// </remarks>
public static class PgSqlLoggerDi
{
    /// <summary>
    /// Registers PostgreSQL logging infrastructure with the dependency injection container.
    /// </summary>
    /// <param name="services">
    /// The <see cref="IServiceCollection"/> to which the PostgreSQL logger services will be added.
    /// </param>
    /// <param name="config">
    /// The application configuration instance containing the required sections for the writer and logger settings.
    /// Expected sections include <see cref="PvNugsPgSqlLogWriterConfig.Section"/> and
    /// <see cref="PvNugsLoggerConfig.Section"/>.
    /// </param>
    /// <returns>
    /// The same <see cref="IServiceCollection"/> instance so additional registrations can be chained.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="services"/> or <paramref name="config"/> is <see langword="null"/>.
    /// </exception>
    /// <remarks>
    /// <para>
    /// This method registers the following services as singletons:
    /// </para>
    /// <list type="bullet">
    /// <item><see cref="IPgSqlLogWriter"/> → <see cref="PgSqlLogWriter"/></item>
    /// <item><see cref="ISqlLogWriter"/> → <see cref="IPgSqlLogWriter"/></item>
    /// <item><see cref="ILogWriter"/> → <see cref="IPgSqlLogWriter"/></item>
    /// <item><see cref="ILoggerFactory"/> → <see cref="PgSqlLoggerFactory"/></item>
    /// <item><see cref="IPgSqlLoggerService"/> → <see cref="PgSqlLoggerService"/></item>
    /// <item><see cref="ISqlLoggerService"/> → <see cref="IPgSqlLoggerService"/></item>
    /// <item><see cref="ILoggerService"/> → <see cref="IPgSqlLoggerService"/></item>
    /// </list>
    /// <para>
    /// The configured values are read from the section names defined in
    /// <see cref="PvNugsPgSqlLogWriterConfig.Section"/> and <see cref="PvNugsLoggerConfig.Section"/>.
    /// </para>
    /// </remarks>
    public static IServiceCollection TryAddPvNugsPgSqlLoggerNc10PgSql(
        this IServiceCollection services,
        IConfiguration config)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(config);

        services.Configure<PvNugsPgSqlLogWriterConfig>(
            config.GetSection(PvNugsPgSqlLogWriterConfig.Section));

        services.TryAddSingleton<IPgSqlLogWriter, PgSqlLogWriter>();
        services.TryAddSingleton<ISqlLogWriter>(sp =>
            sp.GetRequiredService<IPgSqlLogWriter>());
        services.TryAddSingleton<ILogWriter>(sp =>
            sp.GetRequiredService<IPgSqlLogWriter>());

        services.Configure<PvNugsLoggerConfig>(
            config.GetSection(PvNugsLoggerConfig.Section));
        services.TryAddSingleton<ILoggerFactory, PgSqlLoggerFactory>();

        services.TryAddSingleton<IPgSqlLoggerService>(sp =>
            new PgSqlLoggerService(
                sp.GetRequiredService<IOptions<PvNugsLoggerConfig>>(),
                sp.GetRequiredService<IPgSqlLogWriter>()));
        services.TryAddSingleton<ISqlLoggerService>(sp =>
            sp.GetRequiredService<IPgSqlLoggerService>());
        services.TryAddSingleton<ILoggerService>(sp =>
            sp.GetRequiredService<IPgSqlLoggerService>());

        return services;
    }
}
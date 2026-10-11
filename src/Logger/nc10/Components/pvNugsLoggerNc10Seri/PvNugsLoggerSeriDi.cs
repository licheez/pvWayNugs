using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using pvNugsLoggerNc10Abstractions;

namespace pvNugsLoggerNc10Seri;

/// <summary>
/// Provides dependency injection extension methods for registering Serilog-based console logging services.
/// </summary>
/// <remarks>
/// <para>
/// This static class encapsulates the bootstrapping required to integrate the console logging pipeline with
/// the Microsoft dependency injection container. It registers the concrete writer, binds logging settings from
/// configuration, and exposes the logger through the abstractions used by the rest of the application.
/// </para>
/// <para>
/// The methods use the <c>TryAdd...</c> pattern to preserve existing registrations and avoid duplicate service
/// definitions when the logger is configured from multiple initialization paths.
/// </para>
/// </remarks>
public static class PvNugsLoggerSeriDi
{
    /// <summary>
    /// Registers the Serilog console writer and preserves any existing writer registrations.
    /// </summary>
    /// <param name="services">
    /// The <see cref="IServiceCollection"/> to which the console writer services will be added.
    /// </param>
    /// <returns>
    /// The same <see cref="IServiceCollection"/> instance so additional registrations can be chained.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="services"/> is <see langword="null"/>.
    /// </exception>
    /// <remarks>
    /// <para>
    /// This method registers the writer under both the generic <see cref="ILogWriter"/> abstraction and the
    /// more specific <see cref="IConsoleLogWriter"/> contract. The first registration wins because of the
    /// behavior.
    /// </para>
    /// </remarks>
    // ReSharper disable once MemberCanBePrivate.Global
    public static IServiceCollection TryAddPvNugsLoggerSeriWriter(
        this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.TryAddSingleton<IConsoleLogWriter, SerilogConsoleWriter>();
        services.TryAddSingleton<ILogWriter>(sp =>
            sp.GetRequiredService<IConsoleLogWriter>());
        
        return services;
    }

    /// <summary>
    /// Configures and registers the complete Serilog console logging pipeline with the dependency injection container.
    /// </summary>
    /// <param name="services">
    /// The <see cref="IServiceCollection"/> to which the logging components are added.
    /// </param>
    /// <param name="config">
    /// The application configuration instance containing the logger settings. The expected section is
    /// <see cref="PvNugsLoggerConfig.Section"/>.
    /// </param>
    /// <returns>
    /// The same <see cref="IServiceCollection"/> instance for fluent registration patterns.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="services"/> or <paramref name="config"/> is <see langword="null"/>.
    /// </exception>
    /// <remarks>
    /// <para>
    /// This method performs the following setup:
    /// </para>
    /// <list type="number">
    /// <item>Registers the console log writer in the container.</item>
    /// <item>Binds <see cref="PvNugsLoggerConfig"/> from the configuration section.</item>
    /// <item>Registers a <see cref="ILoggerFactory"/> implementation for the Serilog console pipeline.</item>
    /// <item>Registers the concrete concrete service as <see cref="ISeriConsoleLoggerService"/>,
    /// <see cref="IConsoleLoggerService"/>, and <see cref="ILoggerService"/>.</item>
    /// </list>
    /// <para>
    /// As a result, callers can resolve <see cref="ILoggerService"/> or the specialized console-based interfaces
    /// without wiring the writer and logger service manually.
    /// </para>
    /// </remarks>
    public static IServiceCollection TryAddPvNugsLoggerSeriService(
        this IServiceCollection services,
        IConfiguration config)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(config);

        services.TryAddPvNugsLoggerSeriWriter();

        services.Configure<PvNugsLoggerConfig>(
            config.GetSection(PvNugsLoggerConfig.Section));

        services.TryAddPvNugsLoggerSeriWriter();

        services.Configure<PvNugsLoggerConfig>(
            config.GetSection(PvNugsLoggerConfig.Section));
        services.TryAddSingleton<ILoggerFactory, SeriLogLoggerFactory>();

        services.TryAddSingleton<ISeriConsoleLoggerService, SerilogConsoleService>();
        services.TryAddSingleton<IConsoleLoggerService>(sp =>
            sp.GetRequiredService<ISeriConsoleLoggerService>());
        services.TryAddSingleton<ILoggerService>(sp =>
            sp.GetRequiredService<ISeriConsoleLoggerService>()
        );

        return services;
    }
}
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using pvNugsLoggerNc9Abstractions;

namespace pvNugsLoggerNc9Seri;

/// <summary>
/// Provides extension methods for registering Serilog-based logging services in the dependency injection container.
/// </summary>
public static class PvNugsLoggerSeriDi
{
    /// <summary>
    /// Attempts to register the Serilog console writer implementations in the service collection.
    /// If implementations are already registered, the existing registrations are preserved.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add the services to.</param>
    /// <returns>The same service collection for method chaining.</returns>
    /// <remarks>
    /// This method registers:
    /// - <see cref="SerilogConsoleWriter"/> as singleton for <see cref="ILogWriter"/>
    /// - <see cref="SerilogConsoleWriter"/> as singleton for <see cref="IConsoleLogWriter"/>
    /// </remarks>
    // ReSharper disable once MemberCanBePrivate.Global
    public static IServiceCollection TryAddPvNugsLoggerSeriWriter(this IServiceCollection services)
    {
        services.AddSingleton<ILogWriter, SerilogConsoleWriter>();
        services.TryAddSingleton<IConsoleLogWriter, SerilogConsoleWriter>();
        return services;
    }

    /// <summary>
    /// Configures and registers the complete Serilog logging service with its dependencies.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add the services to.</param>
    /// <param name="config">The configuration containing logger settings.</param>
    /// <returns>The same service collection for method chaining.</returns>
    /// <remarks>
    /// This method:
    /// 1. Registers the Serilog console writer
    /// 2. Configures logger settings from configuration
    /// 3. Registers the Serilog console service as singleton for multiple interfaces:
    ///    - <see cref="ILoggerService"/>
    ///    - <see cref="IConsoleLoggerService"/>
    ///    - <see cref="ISeriConsoleLoggerService"/>
    /// </remarks>
    public static IServiceCollection TryAddPvNugsLoggerSeriService(
        this IServiceCollection services,
        IConfiguration config)
    {
        services.TryAddPvNugsLoggerSeriWriter();
        
        services.Configure<PvNugsLoggerConfig>(
            config.GetSection(PvNugsLoggerConfig.Section));
        
        services.TryAddSingleton<ILoggerService, SerilogConsoleService>();
        services.TryAddSingleton<IConsoleLoggerService, SerilogConsoleService>();
        services.TryAddSingleton<ISeriConsoleLoggerService, SerilogConsoleService>();
        
        return services;
    }
}
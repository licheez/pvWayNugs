using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using pvNugsLoggerNc6Abstractions;

namespace pvNugsLoggerNc6Hybrid;

/// <summary>
/// Provides extension methods for registering the hybrid logger and related services in the dependency injection container.
/// </summary>
public static class PvNugsLoggerHybridDi
{
    /// <summary>
    /// Registers the hybrid logger and its dependencies in the service collection.
    /// <para>
    /// Configures <see cref="PvNugsLoggerConfig"/> from the specified configuration section,
    /// registers <see cref="IHybridLoggerService"/> as a singleton, and ensures that <see cref="ILoggerService"/>
    /// is mapped to the hybrid logger implementation.
    /// </para>
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add the services to.</param>
    /// <param name="config">The application configuration used to bind logger settings.</param>
    /// <returns>The same <see cref="IServiceCollection"/> instance so that additional calls can be chained.</returns>
    public static IServiceCollection TryAddPvNugsHybridLogger(
        this IServiceCollection services, IConfiguration config)
    {
        // 1. Bind logger configuration from the specified section
        services.Configure<PvNugsLoggerConfig>(
            config.GetSection(PvNugsLoggerConfig.Section));
        
        // 2. Register the hybrid logger as a singleton, aggregating all log writers
        services.TryAddSingleton<IHybridLoggerService>(sp =>
        {
            // Optional: get a console logger for diagnostic output during DI setup
            var cls = sp.GetService<IConsoleLoggerService>();
            var options = sp.GetRequiredService<IOptions<PvNugsLoggerConfig>>();
            cls?.Log("hls: building the HybridLoggerService from provisioned logWriters");
            // Gather all registered log writers (generic, console, SQL)
            var gLogWriters = sp.GetServices<ILogWriter>();
            var cLogWriters = sp.GetServices<IConsoleLogWriter>()
                .Cast<ILogWriter>();
            var sLogWriters = sp.GetServices<ISqlLogWriter>()
                .Cast<ILogWriter>();
            // Unify and deduplicate log writers by type name
            var logWriters = gLogWriters
                .Union(cLogWriters.Union(sLogWriters))
                .DistinctBy(x => x.GetType().Name)
                .ToArray();
            // Log the addition of each log writer for traceability
            foreach (var logWriter in logWriters)
                cls?.Log($"hls: adding logWriter: {logWriter.GetType().Name} " +
                         $"to the hybrid logger");
            // Instantiate the hybrid logger with all discovered log writers
            return new HybridLoggerService(options, logWriters);
        });

        // 3. Ensure ILoggerService resolves to the hybrid logger
        var sd = ServiceDescriptor.Singleton<ILoggerService>(sp =>
        {
            var cls = sp.GetService<IConsoleLoggerService>();
            cls?.Log("hls: building ILoggerService for using IHybridLoggerService");
            return sp.GetRequiredService<IHybridLoggerService>();
        });
        
        // Replace or add ILoggerService registration as needed
        var hasILoggerService = services.Any(
            x => x.ServiceType == typeof(ILoggerService));
        if (hasILoggerService)
        {
            services.Replace(sd);
        }
        else
        {
            services.Add(sd);
        }
        
        // 4. Return the service collection for chaining
        return services;
    }
}
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using pvNugsLoggerNc6Abstractions;

namespace pvNugsLoggerNc6UTest;

/// <summary>
/// Dependency injection helper class for configuring and creating unit test logger services
/// </summary>
/// <remarks>
/// This class provides factory methods and dependency injection extensions for setting up
/// in-memory logging in unit tests. It supports both direct instantiation and Microsoft.Extensions.DependencyInjection integration.
/// </remarks>
public static class PvNugsLoggerUTestDi
{
    /// <summary>
    /// Creates a new instance of IUTestLogWriter for capturing log entries in memory
    /// </summary>
    /// <returns>An instance of IUTestLogWriter</returns>
    // ReSharper disable once MemberCanBePrivate.Global
    public static IUTestLogWriter CreateUTestLogWriter()
    {
        return new UTestLogWriter();
    }
    
    /// <summary>
    /// Creates a new instance of IUTestLoggerService with the provided log writer
    /// </summary>
    /// <param name="utLw">The unit test log writer to use for capturing log entries</param>
    /// <returns>An instance of IUTestLoggerService</returns>
    public static IUTestLoggerService CreateService(
        IUTestLogWriter utLw)
    {
        return new UTestLoggerService(utLw);
    }
    
    /// <summary>
    /// Creates a new instance of IUTestLoggerService and returns the log writer via an out parameter
    /// </summary>
    /// <param name="utLw">The created unit test log writer instance</param>
    /// <returns>An instance of IUTestLoggerService</returns>
    public static IUTestLoggerService CreateService(
        out IUTestLogWriter utLw)
    {
        utLw = CreateUTestLogWriter();
        return new UTestLoggerService(utLw);
    }

    /// <summary>
    /// Registers the unit test logger service and its dependencies in the service collection as transient services
    /// </summary>
    /// <param name="services">The service collection to register services into</param>
    /// <returns>The IUTestLogWriter instance that was registered, allowing direct access to captured log entries</returns>
    /// <remarks>
    /// This method registers the following services:
    /// - IUTestLogWriter (singleton instance returned)
    /// - ILoggerService (as UTestLoggerService)
    /// - IUTestLoggerService (as UTestLoggerService)
    /// All services are registered as transient, but share the same IUTestLogWriter instance for log capture.
    /// </remarks>
    public static IUTestLogWriter AddPvWayUTestLoggerService(
        this IServiceCollection services)
    {
        var logWriter = new UTestLogWriter();
        
        services.TryAddTransient<IUTestLogWriter>(_ => logWriter);
        services.TryAddTransient<ILoggerService, UTestLoggerService>();
        services.TryAddTransient<IUTestLoggerService, UTestLoggerService>();
        
        return logWriter;
    }
}
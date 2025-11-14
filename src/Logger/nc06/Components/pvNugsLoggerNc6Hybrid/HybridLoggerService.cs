using Microsoft.Extensions.Options;
using pvNugsLoggerNc6Abstractions;

namespace pvNugsLoggerNc6Hybrid;

/// <summary>
/// Provides a hybrid logger service that delegates log messages to multiple log writers.
/// </summary>
/// <remarks>
/// This service is internal and is typically registered and resolved via dependency injection.
/// </remarks>
internal class HybridLoggerService :
    BaseLoggerService, IHybridLoggerService
{
    /// <summary>
    /// Provides a hybrid logger service that delegates log messages to multiple log writers.
    /// </summary>
    /// <remarks>
    /// This service is internal and is typically registered and resolved via dependency injection.
    /// </remarks>
    public HybridLoggerService(IOptions<PvNugsLoggerConfig> options,
        ILogWriter[] logWriters) : base(options.Value.MinLevel, logWriters)
    {
    }
}

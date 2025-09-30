using Microsoft.Extensions.Options;
using pvNugsLoggerNc9Abstractions;

namespace pvNugsLoggerNc9Hybrid;

/// <summary>
/// Provides a hybrid logger service that delegates log messages to multiple log writers.
/// </summary>
/// <remarks>
/// This service is internal and is typically registered and resolved via dependency injection.
/// </remarks>
internal class HybridLoggerService(
    IOptions<PvNugsLoggerConfig> options,
    ILogWriter[] logWriters) :
    BaseLoggerService(options.Value.MinLevel, logWriters), IHybridLoggerService;

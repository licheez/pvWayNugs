using Microsoft.Extensions.Options;
using pvNugsLoggerNc9Abstractions;

namespace pvNugsLoggerNc9Seri;

/// <summary>
/// Implements a Serilog-based console logging service that provides structured logging capabilities.
/// Extends <see cref="BaseLoggerService"/> to leverage core logging functionality while
/// adding Serilog-specific console output formatting.
/// </summary>
internal sealed class SerilogConsoleService(
    SeverityEnu minLogLevel,
    IConsoleLogWriter logWriter) : BaseLoggerService(minLogLevel, logWriter), 
    ISeriConsoleLoggerService
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SerilogConsoleService"/> class
    /// using configuration settings.
    /// </summary>
    /// <param name="config">The configuration options containing logging settings.</param>
    /// <param name="logWriter">The console log writer implementation.</param>
    public SerilogConsoleService(
        IOptions<PvNugsLoggerConfig> config,
        IConsoleLogWriter logWriter): this(config.Value.MinLevel, logWriter)
    {
    }
}
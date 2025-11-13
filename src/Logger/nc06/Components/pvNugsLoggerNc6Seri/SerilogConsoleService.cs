using Microsoft.Extensions.Options;
using pvNugsLoggerNc6Abstractions;

namespace pvNugsLoggerNc6Seri;

/// <summary>
/// Implements a Serilog-based console logging service that provides structured logging capabilities.
/// Extends <see cref="BaseLoggerService"/> to leverage core logging functionality while
/// adding Serilog-specific console output formatting.
/// </summary>
internal sealed class SerilogConsoleService : BaseLoggerService, 
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

    /// <summary>
    /// Implements a Serilog-based console logging service that provides structured logging capabilities.
    /// Extends <see cref="BaseLoggerService"/> to leverage core logging functionality while
    /// adding Serilog-specific console output formatting.
    /// </summary>
    public SerilogConsoleService(SeverityEnu minLogLevel,
        IConsoleLogWriter logWriter) : base(minLogLevel, logWriter)
    {
    }
}
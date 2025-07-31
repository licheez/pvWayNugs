using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using pvNugsLoggerNc9;
using pvNugsLoggerNc9Abstractions;

namespace pvNugsLoggerNc9Seri;

/// <summary>
/// Provides a factory for creating Serilog-based console loggers.
/// Implements <see cref="ILoggerProvider"/> to integrate with the Microsoft.Extensions.Logging framework.
/// </summary>
public sealed class SerilogConsoleLoggerProvider(SeverityEnu minLogLevel) : ILoggerProvider
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SerilogConsoleLoggerProvider"/> class
    /// with default Debug severity level.
    /// </summary>
    public SerilogConsoleLoggerProvider(): this(SeverityEnu.Debug)
    {
    }
    
    /// <summary>
    /// Initializes a new instance of the <see cref="SerilogConsoleLoggerProvider"/> class
    /// using configuration settings.
    /// </summary>
    /// <param name="config">The configuration options containing logging settings.</param>
    public SerilogConsoleLoggerProvider(
        IOptions<PvNugsLoggerConfig> config) : this(config.Value.MinLevel)
    {
    }
    
    /// <inheritdoc/>
    public void Dispose()
    {
        // nop
    }

    /// <inheritdoc/>
    public ILogger CreateLogger(string categoryName)
    {
        var lw = new SerilogConsoleWriter();
        return new SerilogConsoleService(minLogLevel, lw);
    }
}
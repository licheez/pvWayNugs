using System.Globalization;
using pvNugsLoggerNc6Abstractions;
using Serilog;
using Serilog.Events;

namespace pvNugsLoggerNc6Seri;

/// <summary>
/// Implements a console log writer using Serilog for formatted console output.
/// Provides structured logging capabilities with enhanced formatting and level-based coloring.
/// </summary>
/// <remarks>
/// This implementation:
/// - Uses Serilog's console sink for output
/// - Supports all severity levels
/// - Includes contextual information (user, company, topic)
/// - Formats timestamps using invariant culture
/// - Provides both synchronous and asynchronous logging methods
/// </remarks>
internal sealed class SerilogConsoleWriter: IConsoleLogWriter
{
    /// <summary>
    /// The Serilog logger instance configured for console output.
    /// Initialized with verbose minimum level and log context enrichment.
    /// </summary>
    private readonly Serilog.Core.Logger _logger = new LoggerConfiguration()
        .WriteTo.Console()
        // filtering message is done by the WriteLog method
        .MinimumLevel.Is(LogEventLevel.Verbose)
        .Enrich.FromLogContext()
        .CreateLogger();
    
    /// <inheritdoc/>
    public void Dispose()
    {
        // nop
    }

    /// <inheritdoc/>
    public ValueTask DisposeAsync()
    {
        return new ValueTask();
    }

    /// <inheritdoc/>
    public Task WriteLogAsync(
        string? userId, string? companyId, string? topic, 
        SeverityEnu severity, string machineName,
        string memberName, string filePath, int lineNumber, 
        string message, DateTime dateUtc)
    {
        WriteLog(userId, companyId, topic,
            severity, machineName,
            memberName, filePath, lineNumber,
            message, dateUtc);
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public void WriteLog(string? userId, string? companyId, string? topic, 
        SeverityEnu severity, string machineName,
        string memberName, string filePath, int lineNumber, 
        string message, DateTime dateUtc)
    {
        if (severity == SeverityEnu.Ok) return;
        var sLevel = GetLogEventLevel(severity);
        var userIdStr = string.IsNullOrEmpty(userId)
            ? string.Empty
            : $" userId: '{userId}'";
        var companyIdStr = string.IsNullOrEmpty(companyId)
            ? string.Empty
            : $" companyId: '{companyId}'";
        var topicStr = string.IsNullOrEmpty(topic)
            ? string.Empty
            : $" topic: '{topic}'";
        var dateUtcStr = dateUtc.ToString(CultureInfo.InvariantCulture);
        _logger.Write(sLevel,
            "{Message} from {MachineName} in {MemberName} " +
            "({FilePath}) line {LineNumber} at {DateUtc}" +
            "{UserId}{CompanyId}{Topic}",
            message, machineName,
            memberName, filePath, 
            lineNumber, dateUtcStr,
            userIdStr, companyIdStr, topicStr);
    }

    /// <summary>
    /// Converts the application's severity enum to Serilog's LogEventLevel.
    /// </summary>
    /// <param name="severity">The severity level to convert.</param>
    /// <returns>The corresponding Serilog LogEventLevel.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when the severity value is not recognized.
    /// </exception>
    private static LogEventLevel GetLogEventLevel(SeverityEnu severity)
    {
        return severity switch
        {
            SeverityEnu.Ok => LogEventLevel.Verbose,
            SeverityEnu.Trace => LogEventLevel.Verbose,
            SeverityEnu.Debug => LogEventLevel.Debug,
            SeverityEnu.Info => LogEventLevel.Information,
            SeverityEnu.Warning => LogEventLevel.Warning,
            SeverityEnu.Error => LogEventLevel.Error,
            SeverityEnu.Fatal => LogEventLevel.Fatal,
            _ => throw new ArgumentOutOfRangeException(nameof(severity), severity, null)
        };
    }
    
}

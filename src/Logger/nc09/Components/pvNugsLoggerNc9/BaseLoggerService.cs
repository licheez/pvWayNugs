
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.Extensions.Logging;
using pvNugsLoggerNc9Abstractions;

// ReSharper disable ExplicitCallerInfoArgument
namespace pvNugsLoggerNc9;

/// <summary>
/// Base implementation of the logging service that provides core logging functionality.
/// Implements <see cref="ILoggerService"/> interface and handles log message formatting,
/// severity level filtering, and distribution to multiple log writers.
/// </summary>
internal abstract class BaseLoggerService(
    SeverityEnu minLevel,
    params ILogWriter[] logWriters) : ILoggerService
{
    private string? _userId;
    private string? _companyId;
    private string? _topic;

    /// <inheritdoc />
    public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        var logMessage = formatter(state, exception);

        if (exception != null)
        {
            var stackTraceLines = Environment.StackTrace
                .Split(Environment.NewLine)
                .Where(x => !(
                    x.Contains("Microsoft.Extensions.Logging.LoggerExtensions")
                    || x.Contains("pvNugsLogger")
                    || x.Contains("System.Environment")
                ));
            var sb = new StringBuilder();
            foreach (var line in stackTraceLines)
            {
                if (sb.Length > 0) sb.Append(Environment.NewLine);
                sb.Append(line);
            }

            logMessage += Environment.NewLine + sb;
        }

        var severity = GetSeverity(logLevel);

        var stackTrace = new StackTrace(true);
        var frame = stackTrace.GetFrames()
            .Skip(3).FirstOrDefault(x => x.GetFileLineNumber() > 0);
        var memberName = frame?.GetMethod()?.ToString() ?? string.Empty;
        var filePath = frame?.GetFileName() ?? string.Empty;
        var lineNumber = frame?.GetFileLineNumber() ?? -1;

        var eventMessage = string.Empty;
        if (eventId.Id != 0 || !string.IsNullOrEmpty(eventId.Name))
        {
            eventMessage = $"[{eventId.Id}:{eventId.Name}] ";
        }
        var message = $"{eventMessage}{logMessage}";
        Log(message, severity, memberName, filePath, lineNumber);
    }

    /// <inheritdoc />
    public bool IsEnabled(LogLevel logLevel)
    {
        var severity = GetSeverity(logLevel);
        return severity >= minLevel;
    }

    /// <inheritdoc />
    public IDisposable BeginScope<TState>(TState state) where TState : notnull
    {
        return this;
    }

    /// <inheritdoc />
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Releases the unmanaged resources used by this class and optionally releases the managed resources.
    /// </summary>
    /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
    protected virtual void Dispose(bool disposing)
    {
        foreach (var logWriter in logWriters)
        {
            logWriter.Dispose();
        }
    }

    /// <summary>
    /// Finalizes an instance of the <see cref="BaseLoggerService"/> class.
    /// </summary>
    ~BaseLoggerService()
    {
        Dispose(false);
    }

    /// <inheritdoc />
    public ValueTask DisposeAsync()
    {
        foreach (var logWriter in logWriters)
        {
            logWriter.DisposeAsync();
        }
        GC.SuppressFinalize(this);
        return new ValueTask();
    }

    /// <inheritdoc />
    public void SetUser(string? userId, string? companyId = null)
    {
        _userId = userId;
        _companyId = companyId;
    }

    /// <inheritdoc />
    public void SetTopic(string? topic)
    {
        _topic = topic;
    }

    /// <inheritdoc />
    public void Log(
        string message,
        SeverityEnu severity = SeverityEnu.Debug,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = -1)
    {
        WriteLog(message, _topic, severity,
            memberName, filePath, lineNumber);
    }

    /// <inheritdoc />
    public void Log(
        IEnumerable<string> messages,
        SeverityEnu severity,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = -1)
    {
        WriteLog(GetMessage(messages), _topic, severity,
            memberName, filePath, lineNumber);
    }

    /// <inheritdoc />
    public void Log(
        Exception e,
        SeverityEnu severity = SeverityEnu.Fatal,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = -1)
    {
        WriteLog(e.GetDeepMessage(), _topic, severity,
            memberName, filePath, lineNumber);
    }

    /// <inheritdoc />
    public void Log(
        IMethodResult result,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = -1)
    {
        WriteLog(result.ErrorMessage, _topic, result.Severity,
            memberName, filePath, lineNumber);
    }

    /// <inheritdoc />
    public void Log(
        string message,
        string? topic,
        SeverityEnu severity = SeverityEnu.Debug,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = -1)
    {
        WriteLog(message, topic, severity,
            memberName, filePath, lineNumber);
    }

    /// <inheritdoc />
    public void Log(
        IMethodResult result,
        string? topic,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = -1)
    {
        WriteLog(result.ErrorMessage, topic, result.Severity,
            memberName, filePath, lineNumber);
    }

    /// <inheritdoc />
    public void Log(
        IEnumerable<string> messages,
        string? topic,
        SeverityEnu severity,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = -1)
    {
        WriteLog(GetMessage(messages), topic, severity,
            memberName, filePath, lineNumber);
    }

    /// <inheritdoc />
    public void Log(
        Exception e,
        string? topic,
        SeverityEnu severity = SeverityEnu.Fatal,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = -1)
    {
        WriteLog(e.GetDeepMessage(), topic, severity,
            memberName, filePath, lineNumber);
    }

    /// <inheritdoc />
    public Task LogAsync(
        string message,
        SeverityEnu severity = SeverityEnu.Debug,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = -1)
    {
        return WriteLogAsync(message, _topic, severity,
            memberName, filePath, lineNumber);
    }

    /// <inheritdoc />
    public Task LogAsync(
        IEnumerable<string> messages,
        SeverityEnu severity,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = -1)
    {
        return WriteLogAsync(GetMessage(messages), _topic, severity,
            memberName, filePath, lineNumber);
    }

    /// <inheritdoc />
    public Task LogAsync(
        Exception e,
        SeverityEnu severity = SeverityEnu.Fatal,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = -1)
    {
        return WriteLogAsync(e.GetDeepMessage(), _topic, severity,
            memberName, filePath, lineNumber);
    }

    /// <inheritdoc />
    public Task LogAsync(
        IMethodResult result,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = -1)
    {
        return WriteLogAsync(result.ErrorMessage, _topic, result.Severity,
            memberName, filePath, lineNumber);
    }

    /// <inheritdoc />
    public Task LogAsync(
        string message,
        string? topic,
        SeverityEnu severity = SeverityEnu.Debug,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = -1)
    {
        return WriteLogAsync(message, topic, severity,
            memberName, filePath, lineNumber);
    }

    /// <inheritdoc />
    public Task LogAsync(
        IEnumerable<string> messages,
        string? topic,
        SeverityEnu severity,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = -1)
    {
        return WriteLogAsync(GetMessage(messages), topic, severity,
            memberName, filePath, lineNumber);
    }

    /// <inheritdoc />
    public Task LogAsync(
        Exception e,
        string? topic,
        SeverityEnu severity = SeverityEnu.Fatal,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = -1)
    {
        return WriteLogAsync(e.GetDeepMessage(), topic, severity,
            memberName, filePath, lineNumber);
    }

    /// <inheritdoc />
    public Task LogAsync(
        IMethodResult result,
        string? topic,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = -1)
    {
        return WriteLogAsync(result.ErrorMessage, topic, result.Severity,
            memberName, filePath, lineNumber);
    }

    /// <summary>
    /// Combines multiple messages into a single string, separated by newlines.
    /// </summary>
    /// <param name="messages">Collection of messages to combine.</param>
    /// <returns>A single string containing all messages separated by newlines.</returns>
    private static string GetMessage(IEnumerable<string> messages)
    {
        var sb = new StringBuilder();
        foreach (var msg in messages)
        {
            if (sb.Length > 0)
                sb.Append(Environment.NewLine);
            sb.Append(msg);
        }

        return sb.ToString();
    }

    /// <summary>
    /// Writes a log entry to all configured log writers if the severity level meets the minimum threshold.
    /// </summary>
    /// <param name="message">The message to log.</param>
    /// <param name="topic">The topic associated with the message.</param>
    /// <param name="severity">The severity level of the message.</param>
    /// <param name="memberName">The name of the calling member.</param>
    /// <param name="filePath">The path of the source file.</param>
    /// <param name="lineNumber">The line number in the source file.</param>
    private void WriteLog(
        string message,
        string? topic,
        SeverityEnu severity = SeverityEnu.Debug,
        string memberName = "",
        string filePath = "",
        int lineNumber = -1)
    {
        if (severity < minLevel)
            return;

        foreach (var logWriter in logWriters)
        {
            logWriter.WriteLog(
                _userId, _companyId, topic,
                severity,
                Environment.MachineName,
                memberName, filePath, lineNumber,
                message, DateTime.UtcNow);
        }
    }

    /// <summary>
    /// Asynchronously writes a log entry to all configured log writers if the severity level meets the minimum threshold.
    /// </summary>
    /// <param name="message">The message to log.</param>
    /// <param name="topic">The topic associated with the message.</param>
    /// <param name="severity">The severity level of the message.</param>
    /// <param name="memberName">The name of the calling member.</param>
    /// <param name="filePath">The path of the source file.</param>
    /// <param name="lineNumber">The line number in the source file.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    private async Task WriteLogAsync(
        string message,
        string? topic,
        SeverityEnu severity = SeverityEnu.Debug,
        string memberName = "",
        string filePath = "",
        int lineNumber = -1)
    {
        if (severity < minLevel)
            return;

        foreach (var logWriter in logWriters)
        {
            await logWriter.WriteLogAsync(
                _userId, _companyId, topic,
                severity,
                Environment.MachineName,
                memberName, filePath, lineNumber,
                message, DateTime.UtcNow);
        }
    }

    /// <summary>
    /// Converts a Microsoft.Extensions.Logging LogLevel to the corresponding SeverityEnu value.
    /// </summary>
    /// <param name="logLevel">The LogLevel to convert.</param>
    /// <returns>The corresponding SeverityEnu value.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the logLevel is not recognized.</exception>
    private static SeverityEnu GetSeverity(LogLevel logLevel)
    {
        switch (logLevel)
        {
            case LogLevel.Trace:
                return SeverityEnu.Trace;
            case LogLevel.Debug:
                return SeverityEnu.Debug;
            case LogLevel.Information:
                return SeverityEnu.Info;
            case LogLevel.Warning:
                return SeverityEnu.Warning;
            case LogLevel.Error:
                return SeverityEnu.Error;
            case LogLevel.Critical:
                return SeverityEnu.Fatal;
            case LogLevel.None:
                return SeverityEnu.Ok;
            default:
                throw new ArgumentOutOfRangeException(nameof(logLevel), logLevel, null);
        }
    }
}
/// <summary>
/// Generic version of the base logger service that provides type-specific logging context.
/// </summary>
/// <typeparam name="T">The type that provides context for the logger.</typeparam>
[SuppressMessage("CodeQuality", "S2326:Unused type parameters should be removed",
    Justification = "Type parameter T is used for type safety in the logger hierarchy")]
internal abstract class BaseLoggerService<T>(
    SeverityEnu minLevel,
    params ILogWriter[] logWriters)
    : BaseLoggerService(minLevel, logWriters) where T : class;
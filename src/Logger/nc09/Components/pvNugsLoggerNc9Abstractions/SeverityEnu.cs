using Microsoft.Extensions.Logging;

namespace pvNugsLoggerNc9Abstractions;

/// <summary>
/// Represents logging severity levels for the application.
/// </summary>
public enum SeverityEnu
{
    /// <summary>
    /// Indicates a normal, successful operation. Maps to LogLevel.None.
    /// </summary>
    Ok,
    
    /// <summary>
    /// Contains the most detailed messages. Maps to LogLevel.Trace.
    /// </summary>
    Trace,
    
    /// <summary>
    /// Contains information useful for debugging. Maps to LogLevel.Debug.
    /// </summary>
    Debug,
    
    /// <summary>
    /// Represents normal application flow messages. Maps to LogLevel.Information.
    /// </summary>
    Info,
    
    /// <summary>
    /// Highlights an abnormal or unexpected event. Maps to LogLevel.Warning.
    /// </summary>
    Warning,
    
    /// <summary>
    /// Indicates a failure in the application. Maps to LogLevel.Error.
    /// </summary>
    Error,
    
    /// <summary>
    /// Represents a critical error causing complete failure. Maps to LogLevel.Critical.
    /// </summary>
    Fatal,
}

/// <summary>
/// Provides utility methods for working with severity levels and their representations.
/// </summary>
public static class EnumSeverity
{
    private const string Ok = "O";
    private const string Trace = "T";
    private const string Debug = "D";
    private const string Info = "I";
    private const string Warning = "W";
    private const string Error = "E";
    private const string Fatal = "F";

    /// <summary>
    /// Converts a severity enum value to its single-character code representation.
    /// </summary>
    /// <param name="value">The severity enum value to convert.</param>
    /// <returns>A single-character string code representing the severity level.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the severity value is not defined in the enum.</exception>
    public static string GetCode(SeverityEnu value)
    {
        return value switch
        {
            SeverityEnu.Ok => Ok,
            SeverityEnu.Trace => Trace,
            SeverityEnu.Debug => Debug,
            SeverityEnu.Info => Info,
            SeverityEnu.Warning => Warning,
            SeverityEnu.Error => Error,
            SeverityEnu.Fatal => Fatal,
            _ => throw new ArgumentOutOfRangeException(nameof(value), value, null)
        };
    }

    /// <summary>
    /// Converts a single-character severity code to its corresponding severity enum value.
    /// </summary>
    /// <param name="code">The single-character severity code.</param>
    /// <returns>The corresponding severity enum value.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the code doesn't match any known severity level.</exception>
    public static SeverityEnu GetValue(string code)
    {
        return code switch
        {
            null => SeverityEnu.Ok,
            Ok => SeverityEnu.Ok,
            Trace => SeverityEnu.Trace,
            Debug => SeverityEnu.Debug,
            Info => SeverityEnu.Info,
            Warning => SeverityEnu.Warning,
            Error => SeverityEnu.Error,
            Fatal => SeverityEnu.Fatal,
            _ => throw new ArgumentOutOfRangeException(nameof(code), code, null)
        };
    }

    /// <summary>
    /// Converts a Microsoft LogLevel to its corresponding abbreviated string representation.
    /// </summary>
    /// <param name="lgLevel">The Microsoft LogLevel to convert.</param>
    /// <returns>The abbreviated string representation of the log level.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the log level is not defined.</exception>
    public static string GetMsLogLevelAbbrev(LogLevel lgLevel)
    {
        return lgLevel switch
        {
            LogLevel.Trace => "trac:",
            LogLevel.Debug => "debg:",
            LogLevel.Information => "info:",
            LogLevel.Warning => "warn:",
            LogLevel.Error => "fail:",
            LogLevel.Critical => "crit:",
            LogLevel.None => "",
            _ => throw new ArgumentOutOfRangeException(nameof(lgLevel), lgLevel, null)
        };
    }

    /// <summary>
    /// Converts a severity enum value to its corresponding Microsoft LogLevel.
    /// </summary>
    /// <param name="severity">The severity enum value to convert.</param>
    /// <returns>The corresponding Microsoft LogLevel.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the severity value is not defined in the enum.</exception>
    public static LogLevel GetMsLogLevel(SeverityEnu severity)
    {
        return severity switch
        {
            SeverityEnu.Ok => LogLevel.None,
            SeverityEnu.Trace => LogLevel.Trace,
            SeverityEnu.Debug => LogLevel.Debug,
            SeverityEnu.Info => LogLevel.Information,
            SeverityEnu.Warning => LogLevel.Warning,
            SeverityEnu.Error => LogLevel.Error,
            SeverityEnu.Fatal => LogLevel.Critical,
            _ => throw new ArgumentOutOfRangeException(nameof(severity), severity, null)
        };
    }

    /// <summary>
    /// Converts a Microsoft LogLevel to its corresponding severity enum value.
    /// </summary>
    /// <param name="level">The Microsoft LogLevel to convert.</param>
    /// <returns>The corresponding severity enum value.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the log level is not defined.</exception>
    public static SeverityEnu GetSeverity(LogLevel level)
    {
        return level switch
        {
            LogLevel.Trace => SeverityEnu.Trace,
            LogLevel.Debug => SeverityEnu.Debug,
            LogLevel.Information => SeverityEnu.Info,
            LogLevel.Warning => SeverityEnu.Warning,
            LogLevel.Error => SeverityEnu.Error,
            LogLevel.Critical => SeverityEnu.Fatal,
            LogLevel.None => SeverityEnu.Ok,
            _ => throw new ArgumentOutOfRangeException(nameof(level), level, null)
        };
    }
}
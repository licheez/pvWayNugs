namespace pvNugsLoggerNc9Abstractions;

/// <summary>
/// Defines the contract for writing log entries to a storage medium.
/// Provides both synchronous and asynchronous methods for log persistence
/// with comprehensive contextual information.
/// </summary>
public interface ILogWriter : IDisposable, IAsyncDisposable
{
    /// <summary>
    /// Asynchronously writes a log entry with full contextual information
    /// to the underlying storage medium.
    /// </summary>
    /// <param name="userId">The identifier of the user associated with the log entry.</param>
    /// <param name="companyId">The identifier of the company associated with the user.</param>
    /// <param name="topic">The topic or category of the log entry for logical grouping.</param>
    /// <param name="severity">The severity level of the log entry.</param>
    /// <param name="machineName">The name of the machine where the log was generated.</param>
    /// <param name="memberName">The name of the method that generated the log.</param>
    /// <param name="filePath">The source file path where the log was generated.</param>
    /// <param name="lineNumber">The line number in the source file where the log was generated.</param>
    /// <param name="message">The actual log message content.</param>
    /// <param name="dateUtc">The UTC timestamp when the log entry was created.</param>
    /// <returns>A task representing the asynchronous write operation.</returns>
    Task WriteLogAsync(
        string? userId, string? companyId, string? topic,
        SeverityEnu severity,
        string machineName, string memberName,
        string filePath, int lineNumber,
        string message, DateTime dateUtc);

    /// <summary>
    /// Synchronously writes a log entry with full contextual information
    /// to the underlying storage medium.
    /// </summary>
    /// <param name="userId">The identifier of the user associated with the log entry.</param>
    /// <param name="companyId">The identifier of the company associated with the user.</param>
    /// <param name="topic">The topic or category of the log entry for logical grouping.</param>
    /// <param name="severity">The severity level of the log entry.</param>
    /// <param name="machineName">The name of the machine where the log was generated.</param>
    /// <param name="memberName">The name of the method that generated the log.</param>
    /// <param name="filePath">The source file path where the log was generated.</param>
    /// <param name="lineNumber">The line number in the source file where the log was generated.</param>
    /// <param name="message">The actual log message content.</param>
    /// <param name="dateUtc">The UTC timestamp when the log entry was created.</param>
    void WriteLog(
        string? userId, string? companyId, string? topic,
        SeverityEnu severity,
        string machineName, string memberName,
        string filePath, int lineNumber,
        string message, DateTime dateUtc);
}
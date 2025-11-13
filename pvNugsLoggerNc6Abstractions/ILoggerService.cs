using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;

namespace pvNugsLoggerNc6Abstractions;

/// <summary>
/// Provides comprehensive logging functionality with support for
/// user context, topics, and various logging methods.
/// Extends standard logging (<see cref="ILogger"/>) with additional features
/// for structured and contextual logging.
/// </summary>
public interface ILoggerService : ILogger, IDisposable, IAsyncDisposable
{
    /// <summary>
    /// Sets the user context for further log entries.
    /// All log calls after this will include
    /// the specified user and company identifiers
    /// until changed or reset.
    /// </summary>
    /// <param name="userId">
    /// The identifier of the user performing the logged actions.
    /// Can be null to reset the user context.
    /// </param>
    /// <param name="companyId">
    /// Optional company identifier associated with the user.
    /// Can be null if not applicable.
    /// </param>
    void SetUser(string? userId, string? companyId = null);

    /// <summary>
    /// Sets a topic for further log entries.
    /// All log calls after this will include
    /// the specified topic until changed or reset.
    /// Topics enable logical grouping of related log entries.
    /// </summary>
    /// <param name="topic">
    /// The topic identifier to associate with further logs.
    /// Can be null to reset the topic.
    /// </param>
    void SetTopic(string? topic);

    /// <summary>
    /// Logs a single message with specified severity
    /// and automatic caller information.
    /// </summary>
    /// <param name="message">The message to log.</param>
    /// <param name="severity">
    /// The severity level of the log entry.
    /// Defaults to Debug if not specified.
    /// </param>
    /// <param name="memberName">
    /// Name of the calling method (autopopulated).
    /// </param>
    /// <param name="filePath">
    /// Path of the source file (autopopulated).
    /// </param>
    /// <param name="lineNumber">
    /// Line number in the source file (autopopulated).
    /// </param>
    void Log(
        string message,
        SeverityEnu severity = SeverityEnu.Debug,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = -1);

    /// <summary>
    /// Logs multiple messages as a single entry
    /// with specified severity.
    /// </summary>
    /// <param name="messages">Collection of messages to log.</param>
    /// <param name="severity">Severity level for the log entry.</param>
    /// <param name="memberName">Calling method name (autopopulated).</param>
    /// <param name="filePath">Source file path (autopopulated).</param>
    /// <param name="lineNumber">Source line number (autopopulated).</param>
    void Log(
        IEnumerable<string> messages,
        SeverityEnu severity,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = -1);
    
    /// <summary>
    /// Logs an exception with optional severity override.
    /// Includes exception details and stack trace.
    /// </summary>
    /// <param name="e">The exception to log.</param>
    /// <param name="severity">
    /// Severity level, defaults to Fatal for exceptions.
    /// </param>
    /// <param name="memberName">Calling method name (autopopulated).</param>
    /// <param name="filePath">Source file path (autopopulated).</param>
    /// <param name="lineNumber">Source line number (autopopulated).</param>
    void Log(
        Exception e,
        SeverityEnu severity = SeverityEnu.Fatal,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = -1);

    /// <summary>
    /// Logs a method result including its notifications
    /// and status information.
    /// </summary>
    /// <param name="result">The method result to log.</param>
    /// <param name="memberName">Calling method name (autopopulated).</param>
    /// <param name="filePath">Source file path (autopopulated).</param>
    /// <param name="lineNumber">Source line number (autopopulated).</param>
    void Log(
        IMethodResult result,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = -1);

    // Topic-based variants of logging methods

    /// <summary>
    /// Logs multiple messages with a specific topic
    /// and severity level.
    /// </summary>
    /// <param name="messages">Collection of messages to log.</param>
    /// <param name="topic">
    /// Topic to associate with these messages.
    /// Overrides any topic set via SetTopic.
    /// </param>
    /// <param name="severity">Severity level for the log entry.</param>
    /// <param name="memberName">Calling method name (autopopulated).</param>
    /// <param name="filePath">Source file path (autopopulated).</param>
    /// <param name="lineNumber">Source line number (autopopulated).</param>
    void Log(
        IEnumerable<string> messages,
        string? topic,
        SeverityEnu severity,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = -1);

    /// <summary>
    /// Logs a single message with a specific topic
    /// and optional severity.
    /// </summary>
    /// <param name="message">The message to log.</param>
    /// <param name="topic">
    /// Topic to associate with this message.
    /// Overrides any topic set via SetTopic.
    /// </param>
    /// <param name="severity">
    /// Severity level, defaults to Debug.
    /// </param>
    /// <param name="memberName">Calling method name (autopopulated).</param>
    /// <param name="filePath">Source file path (autopopulated).</param>
    /// <param name="lineNumber">Source line number (autopopulated).</param>
    void Log(
        string message,
        string? topic,
        SeverityEnu severity = SeverityEnu.Debug,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = -1);

    /// <summary>
    /// Logs a method result with a specific topic.
    /// </summary>
    /// <param name="result">The method result to log.</param>
    /// <param name="topic">
    /// Topic to associate with this result.
    /// Overrides any topic set via SetTopic.
    /// </param>
    /// <param name="memberName">Calling method name (autopopulated).</param>
    /// <param name="filePath">Source file path (autopopulated).</param>
    /// <param name="lineNumber">Source line number (autopopulated).</param>
    void Log(
        IMethodResult result,
        string? topic,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = -1);

    /// <summary>
    /// Logs an exception with a specific topic
    /// and optional severity override.
    /// </summary>
    /// <param name="e">The exception to log.</param>
    /// <param name="topic">
    /// Topic to associate with this exception.
    /// Overrides any topic set via SetTopic.
    /// </param>
    /// <param name="severity">
    /// Severity level, defaults to Fatal.
    /// </param>
    /// <param name="memberName">Calling method name (autopopulated).</param>
    /// <param name="filePath">Source file path (autopopulated).</param>
    /// <param name="lineNumber">Source line number (autopopulated).</param>
    void Log(
        Exception e,
        string? topic,
        SeverityEnu severity = SeverityEnu.Fatal,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = -1);

    // Async variants follow the same pattern as their synchronous counterparts

    /// <summary>
    /// Asynchronously logs a single message.
    /// Follows the same pattern as the synchronous version
    /// but returns a Task for await operations.
    /// </summary>
    Task LogAsync(
        string message,
        SeverityEnu severity = SeverityEnu.Debug,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = -1);

    /// Asynchronously logs a list of messages.
    /// Follows the same pattern as the synchronous version
    /// but returns a Task for await operations.
    Task LogAsync(
        IEnumerable<string> messages,
        SeverityEnu severity,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = -1);

   /// <summary>
    /// Asynchronously logs an exception with optional severity override.
    /// Follows the same pattern as the synchronous version but returns a Task.
    /// </summary>
    /// <param name="e">The exception to log.</param>
    /// <param name="severity">Severity level, defaults to Fatal for exceptions.</param>
    /// <param name="memberName">Calling method name (autopopulated).</param>
    /// <param name="filePath">Source file path (autopopulated).</param>
    /// <param name="lineNumber">Source line number (autopopulated).</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task LogAsync(
        Exception e,
        SeverityEnu severity = SeverityEnu.Fatal,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = -1);

    /// <summary>
    /// Asynchronously logs a method result including its notifications and status information.
    /// Follows the same pattern as the synchronous version but returns a Task.
    /// </summary>
    /// <param name="result">The method result to log.</param>
    /// <param name="memberName">Calling method name (autopopulated).</param>
    /// <param name="filePath">Source file path (autopopulated).</param>
    /// <param name="lineNumber">Source line number (autopopulated).</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task LogAsync(
        IMethodResult result,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = -1);

    /// <summary>
    /// Asynchronously logs a single message with a specific topic.
    /// Follows the same pattern as the synchronous version but returns a Task.
    /// </summary>
    /// <param name="message">The message to log.</param>
    /// <param name="topic">Topic to associate with this message. Overrides any topic set via SetTopic.</param>
    /// <param name="severity">Severity level, defaults to Debug.</param>
    /// <param name="memberName">Calling method name (autopopulated).</param>
    /// <param name="filePath">Source file path (autopopulated).</param>
    /// <param name="lineNumber">Source line number (autopopulated).</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task LogAsync(
        string message,
        string? topic,
        SeverityEnu severity = SeverityEnu.Debug,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = -1);

    /// <summary>
    /// Asynchronously logs multiple messages with a specific topic and severity level.
    /// Follows the same pattern as the synchronous version but returns a Task.
    /// </summary>
    /// <param name="messages">Collection of messages to log.</param>
    /// <param name="topic">Topic to associate with these messages. Overrides any topic set via SetTopic.</param>
    /// <param name="severity">Severity level for the log entry.</param>
    /// <param name="memberName">Calling method name (autopopulated).</param>
    /// <param name="filePath">Source file path (autopopulated).</param>
    /// <param name="lineNumber">Source line number (autopopulated).</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task LogAsync(
        IEnumerable<string> messages,
        string? topic,
        SeverityEnu severity,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = -1);

    /// <summary>
    /// Asynchronously logs an exception with a specific topic and optional severity override.
    /// Follows the same pattern as the synchronous version but returns a Task.
    /// </summary>
    /// <param name="e">The exception to log.</param>
    /// <param name="topic">Topic to associate with this exception. Overrides any topic set via SetTopic.</param>
    /// <param name="severity">Severity level, defaults to Fatal.</param>
    /// <param name="memberName">Calling method name (autopopulated).</param>
    /// <param name="filePath">Source file path (autopopulated).</param>
    /// <param name="lineNumber">Source line number (autopopulated).</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task LogAsync(
        Exception e,
        string? topic,
        SeverityEnu severity = SeverityEnu.Fatal,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = -1);

    /// <summary>
    /// Asynchronously logs a method result with a specific topic.
    /// Follows the same pattern as the synchronous version but returns a Task.
    /// </summary>
    /// <param name="result">The method result to log.</param>
    /// <param name="topic">Topic to associate with this result. Overrides any topic set via SetTopic.</param>
    /// <param name="memberName">Calling method name (autopopulated).</param>
    /// <param name="filePath">Source file path (autopopulated).</param>
    /// <param name="lineNumber">Source line number (autopopulated).</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task LogAsync(
        IMethodResult result,
        string? topic,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = -1);
}

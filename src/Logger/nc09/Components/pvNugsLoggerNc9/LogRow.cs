using pvNugsLoggerNc9Abstractions;

namespace pvNugsLoggerNc9;

/// <summary>
/// Represents a concrete implementation of a log entry row that contains all logging metadata and message content.
/// Implements the <see cref="ILoggerServiceRow"/> interface to provide structured access to log entry data.
/// </summary>
internal class LogRow(
    int id,
    string? userId,
    string? companyId,
    string? topic,
    SeverityEnu severity,
    string machineName,
    string memberName,
    string filePath,
    int lineNumber,
    string message,
    DateTime creationDateUtc)
    : ILoggerServiceRow
{
    /// <summary>
    /// Gets the unique identifier for this log entry.
    /// </summary>
    public int Id { get; } = id;

    /// <summary>
    /// Gets the identifier of the user associated with this log entry.
    /// May be null if the log was not generated in a user context.
    /// </summary>
    public string? UserId { get; } = userId;

    /// <summary>
    /// Gets the identifier of the company associated with this log entry.
    /// May be null if the log was not generated in a company context.
    /// </summary>
    public string? CompanyId { get; } = companyId;

    /// <summary>
    /// Gets the topic or category associated with this log entry.
    /// May be null if no specific topic was set.
    /// </summary>
    public string? Topic { get; } = topic;

    /// <summary>
    /// Gets the severity level of this log entry.
    /// </summary>
    public SeverityEnu Severity { get; } = severity;

    /// <summary>
    /// Gets the name of the machine where this log entry was generated.
    /// </summary>
    public string MachineName { get; } = machineName;

    /// <summary>
    /// Gets the name of the method that generated this log entry.
    /// </summary>
    public string MemberName { get; } = memberName;

    /// <summary>
    /// Gets the path to the source file where this log entry was generated.
    /// </summary>
    public string FilePath { get; } = filePath;

    /// <summary>
    /// Gets the line number in the source file where this log entry was generated.
    /// </summary>
    public int LineNumber { get; } = lineNumber;

    /// <summary>
    /// Gets the actual content of the log message.
    /// </summary>
    public string Message { get; } = message;

    /// <summary>
    /// Gets the UTC timestamp when this log entry was created.
    /// </summary>
    public DateTime CreationDateUtc { get; } = creationDateUtc;
}
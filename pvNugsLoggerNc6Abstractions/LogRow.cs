namespace pvNugsLoggerNc6Abstractions;

/// <summary>
/// Represents a concrete implementation of a log entry row that contains all logging metadata and message content.
/// Implements the <see cref="ILoggerServiceRow"/> interface to provide structured access to log entry data.
/// </summary>
internal class LogRow : ILoggerServiceRow
{
    /// <summary>
    /// Represents a concrete implementation of a log entry row that contains all logging metadata and message content.
    /// Implements the <see cref="ILoggerServiceRow"/> interface to provide structured access to log entry data.
    /// </summary>
    public LogRow(int id,
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
    {
        Id = id;
        UserId = userId;
        CompanyId = companyId;
        Topic = topic;
        Severity = severity;
        MachineName = machineName;
        MemberName = memberName;
        FilePath = filePath;
        LineNumber = lineNumber;
        Message = message;
        CreationDateUtc = creationDateUtc;
    }

    /// <summary>
    /// Gets the unique identifier for this log entry.
    /// </summary>
    public int Id { get; }

    /// <summary>
    /// Gets the identifier of the user associated with this log entry.
    /// May be null if the log was not generated in a user context.
    /// </summary>
    public string? UserId { get; }

    /// <summary>
    /// Gets the identifier of the company associated with this log entry.
    /// May be null if the log was not generated in a company context.
    /// </summary>
    public string? CompanyId { get; }

    /// <summary>
    /// Gets the topic or category associated with this log entry.
    /// May be null if no specific topic was set.
    /// </summary>
    public string? Topic { get; }

    /// <summary>
    /// Gets the severity level of this log entry.
    /// </summary>
    public SeverityEnu Severity { get; }

    /// <summary>
    /// Gets the name of the machine where this log entry was generated.
    /// </summary>
    public string MachineName { get; }

    /// <summary>
    /// Gets the name of the method that generated this log entry.
    /// </summary>
    public string MemberName { get; }

    /// <summary>
    /// Gets the path to the source file where this log entry was generated.
    /// </summary>
    public string FilePath { get; }

    /// <summary>
    /// Gets the line number in the source file where this log entry was generated.
    /// </summary>
    public int LineNumber { get; }

    /// <summary>
    /// Gets the actual content of the log message.
    /// </summary>
    public string Message { get; }

    /// <summary>
    /// Gets the UTC timestamp when this log entry was created.
    /// </summary>
    public DateTime CreationDateUtc { get; }
}
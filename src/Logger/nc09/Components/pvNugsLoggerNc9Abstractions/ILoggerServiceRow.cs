namespace pvNugsLoggerNc9Abstractions;

/// <summary>
/// Represents a structured log entry with all its associated metadata.
/// This interface defines the contract for accessing individual fields
/// of a log record after it has been written.
/// </summary>
public interface ILoggerServiceRow
{
    /// <summary>
    /// Gets the unique identifier of the log entry.
    /// </summary>
    int Id { get; }

    /// <summary>
    /// Gets the identifier of the user associated with the log entry.
    /// </summary>
    string? UserId { get; }

    /// <summary>
    /// Gets the identifier of the company associated with the log entry.
    /// </summary>
    string? CompanyId { get; }

    /// <summary>
    /// Gets the topic or category of the log entry used for logical grouping.
    /// </summary>
    string? Topic { get; }

    /// <summary>
    /// Gets the severity level of the logged message.
    /// </summary>
    SeverityEnu Severity { get; }

    /// <summary>
    /// Gets the name of the machine where the log was generated.
    /// </summary>
    string MachineName { get; }

    /// <summary>
    /// Gets the name of the method that generated the log entry.
    /// </summary>
    string MemberName { get; }

    /// <summary>
    /// Gets the source file path where the log was generated.
    /// </summary>
    string FilePath { get; }

    /// <summary>
    /// Gets the line number in the source file where the log was generated.
    /// </summary>
    int LineNumber { get; }

    /// <summary>
    /// Gets the actual content of the log message.
    /// </summary>
    string Message { get; }

    /// <summary>
    /// Gets the UTC timestamp when the log entry was created.
    /// </summary>
    DateTime CreationDateUtc { get; }
}
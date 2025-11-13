namespace pvNugsLoggerNc6Abstractions;

/// <summary>
/// Represents a specialized log writer for unit testing scenarios.
/// Extends the base <see cref="ILogWriter"/> functionality with methods
/// to inspect, verify, and manage logged entries during test execution.
/// </summary>
/// <seealso cref="ILogWriter"/>
public interface IUTestLogWriter: ILogWriter
{
    /// <summary>
    /// Gets the collection of all logged entries.
    /// Provides access to the complete history of logs
    /// written during test execution.
    /// </summary>
    public IEnumerable<ILoggerServiceRow> LogRows { get; }

    /// <summary>
    /// Checks if the log entries contain a row where
    /// the message includes the specified search term.
    /// </summary>
    /// <param name="term">The text to search for within log messages.</param>
    /// <returns>
    /// True if a matching log entry is found; otherwise, false.
    /// </returns>
    public bool Contains(string term);

    /// <summary>
    /// Retrieves the first log entry whose message
    /// contains the specified search term.
    /// </summary>
    /// <param name="term">The text to search for within log messages.</param>
    /// <returns>
    /// The first matching log entry if found; otherwise, null.
    /// </returns>
    public ILoggerServiceRow? FindFirstMatchingRow(string term);

    /// <summary>
    /// Retrieves the most recent log entry whose message
    /// contains the specified search term.
    /// </summary>
    /// <param name="term">The text to search for within log messages.</param>
    /// <returns>
    /// The last matching log entry if found; otherwise, null.
    /// </returns>
    public ILoggerServiceRow? FindLastMatchingRow(string term);

    /// <summary>
    /// Removes all logged entries from the collection.
    /// Useful for resetting the log state between test cases.
    /// </summary>
    void ClearLogs();

    /// <summary>
    /// Outputs all logged entries to the test output.
    /// Helpful for debugging and verifying test behavior.
    /// </summary>
    void ListRows();
}
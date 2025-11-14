using pvNugsLoggerNc6Abstractions;

namespace pvNugsLoggerNc6UTest;

/// <summary>
/// In-memory implementation of IUTestLogWriter that captures log entries for unit testing
/// </summary>
/// <remarks>
/// This class stores all log entries in memory and provides methods to query, search,
/// and verify logged messages during unit tests. Each log entry is assigned a sequential ID.
/// </remarks>
internal sealed class UTestLogWriter: IUTestLogWriter
{
    private int _lastId;
    private readonly IList<ILoggerServiceRow> _rows = new List<ILoggerServiceRow>();
    
    /// <summary>
    /// Lists all captured log rows to the console
    /// </summary>
    public void ListRows()
    {
        foreach (var row in _rows)
        {
            Console.WriteLine(row.ToString());
        }
    }

    /// <summary>
    /// Gets all log entries that have been captured in memory
    /// </summary>
    /// <returns>An enumerable collection of log rows ordered by their ID</returns>
    public IEnumerable<ILoggerServiceRow> LogRows => _rows;

    /// <summary>
    /// Disposes the log writer (no operation for in-memory implementation)
    /// </summary>
    public void Dispose()
    {
        // nop
    }

    /// <summary>
    /// Asynchronously disposes the log writer (no operation for in-memory implementation)
    /// </summary>
    /// <returns>A completed ValueTask</returns>
    public ValueTask DisposeAsync()
    {
        return new ValueTask();
    }

    /// <summary>
    /// Asynchronously writes a log entry to the in-memory collection
    /// </summary>
    /// <param name="userId">Optional user identifier</param>
    /// <param name="companyId">Optional company identifier</param>
    /// <param name="topic">Optional topic or category for the log entry</param>
    /// <param name="severity">The severity level of the log entry</param>
    /// <param name="machineName">The name of the machine where the log was generated</param>
    /// <param name="memberName">The name of the calling member (method/property)</param>
    /// <param name="filePath">The source file path where the log was generated</param>
    /// <param name="lineNumber">The line number in the source file where the log was generated</param>
    /// <param name="message">The log message</param>
    /// <param name="dateUtc">The UTC date and time when the log was generated</param>
    /// <returns>A completed Task</returns>
    public async Task WriteLogAsync(
        string? userId, string? companyId, 
        string? topic, SeverityEnu severity, 
        string machineName, string memberName, 
        string filePath, int lineNumber, 
        string message, DateTime dateUtc)
    {
        var row = new LogRow(
            _lastId++,
            userId, companyId,
            topic, severity,
            machineName, memberName,
            filePath, lineNumber,
            message, dateUtc);
        _rows.Add(row);
        await Task.CompletedTask;
    }

    /// <summary>
    /// Synchronously writes a log entry to the in-memory collection
    /// </summary>
    /// <param name="userId">Optional user identifier</param>
    /// <param name="companyId">Optional company identifier</param>
    /// <param name="topic">Optional topic or category for the log entry</param>
    /// <param name="severity">The severity level of the log entry</param>
    /// <param name="machineName">The name of the machine where the log was generated</param>
    /// <param name="memberName">The name of the calling member (method/property)</param>
    /// <param name="filePath">The source file path where the log was generated</param>
    /// <param name="lineNumber">The line number in the source file where the log was generated</param>
    /// <param name="message">The log message</param>
    /// <param name="dateUtc">The UTC date and time when the log was generated</param>
    public void WriteLog(
        string? userId, string? companyId,
        string? topic, SeverityEnu severity,
        string machineName, string memberName,
        string filePath, int lineNumber,
        string message, DateTime dateUtc)
    {
        var row = new LogRow(
            _lastId++,
            userId, companyId,
            topic, severity,
            machineName, memberName,
            filePath, lineNumber,
            message, dateUtc);
        _rows.Add(row);
    }

    /// <summary>
    /// Checks if any log entry contains the specified search term
    /// </summary>
    /// <param name="term">The search term to look for</param>
    /// <returns>True if the term is found; otherwise, false</returns>
    public bool Contains(string term)
    {
        return _rows.Any(x => 
            x.Message.Contains(term, StringComparison.InvariantCultureIgnoreCase));
    }

    /// <summary>
    /// Finds the first log entry (by ID) that contains the specified search term
    /// </summary>
    /// <param name="term">The search term to look for in log messages</param>
    /// <returns>The first matching log row, or null if no match is found</returns>
    public ILoggerServiceRow? FindFirstMatchingRow(string term)
    {
        return _rows
            .OrderBy(x => x.Id)
            .FirstOrDefault(x => 
                x.Message.Contains(term, StringComparison.InvariantCultureIgnoreCase));
    }

    /// <summary>
    /// Finds the last log entry (by ID) that contains the specified search term
    /// </summary>
    /// <param name="term">The search term to look for in log messages</param>
    /// <returns>The last matching log row, or null if no match is found</returns>
    public ILoggerServiceRow? FindLastMatchingRow(string term)
    {
        return _rows
            .OrderByDescending(x => x.Id)
            .FirstOrDefault(x => 
                x.Message.Contains(term, StringComparison.InvariantCultureIgnoreCase));
    }

    /// <summary>
    /// Clears all captured log entries from memory
    /// </summary>
    /// <remarks>
    /// This method is useful for cleaning up between unit tests
    /// to ensure each test starts with an empty log collection
    /// </remarks>
    public void ClearLogs()
    {
        _rows.Clear();
    }
}
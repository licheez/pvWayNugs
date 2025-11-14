using pvNugsLoggerNc6Abstractions;

namespace pvNugsLoggerNc6UTest;

/// <summary>
/// In-memory logger service implementation for unit testing
/// </summary>
/// <remarks>
/// This logger service captures all log entries in memory regardless of severity level (set to Trace),
/// allowing unit tests to verify logging behavior. All logged messages are written to the provided
/// IUTestLogWriter instance where they can be queried and verified.
/// </remarks>
internal sealed class UTestLoggerService : 
    BaseLoggerService, 
    IUTestLoggerService
{
    /// <summary>
    /// Initializes a new instance of the UTestLoggerService class
    /// </summary>
    /// <param name="logWriter">The unit test log writer that will capture all log entries</param>
    public UTestLoggerService(
        IUTestLogWriter logWriter) : base(SeverityEnu.Trace, logWriter)
    {
    }
}

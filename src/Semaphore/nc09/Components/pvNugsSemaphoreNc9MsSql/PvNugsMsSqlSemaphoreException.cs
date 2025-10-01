namespace pvNugsSemaphoreNc9MsSql;

/// <summary>
/// Represents errors that occur during SQL Server-based semaphore operations.
/// Used to wrap and distinguish exceptions thrown by the distributed semaphore service.
/// </summary>
public class PvNugsMsSqlSemaphoreException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PvNugsMsSqlSemaphoreException"/> class with a specified error message.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    public PvNugsMsSqlSemaphoreException(string message) :
        base($"PvNugsMsSqlSemaphoreException: {message}"){}

    /// <summary>
    /// Initializes a new instance of the <see cref="PvNugsMsSqlSemaphoreException"/> class with a reference to the inner exception that is the cause of this exception.
    /// </summary>
    /// <param name="e">The exception that is the cause of the current exception.</param>
    public PvNugsMsSqlSemaphoreException(Exception e):
        base($"PvNugsMsSqlSemaphoreException: {e.Message}", e)
    {
    }
}
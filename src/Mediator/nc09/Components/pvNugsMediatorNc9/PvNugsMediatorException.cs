namespace pvNugsMediatorNc9;

/// <summary>
/// Represents errors that occur during mediator request handling or notification publishing.
/// </summary>
/// <remarks>
/// <para>
/// This exception is thrown when:
/// </para>
/// <list type="bullet">
/// <item><description>No handler is registered for a request type</description></item>
/// <item><description>A handler or pipeline doesn't have the required HandleAsync method</description></item>
/// <item><description>An exception occurs during handler or pipeline execution (wrapped from the original exception)</description></item>
/// </list>
/// <para>
/// When wrapping another exception, the original exception is preserved as the <see cref="Exception.InnerException"/>.
/// </para>
/// </remarks>
public class PvNugsMediatorException: Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PvNugsMediatorException"/> class with a specified error message.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public PvNugsMediatorException(string message) : 
        base($"pvNugsMediator Exception: {message}")
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PvNugsMediatorException"/> class with a reference to the inner exception.
    /// </summary>
    /// <param name="e">The exception that is the cause of the current exception.</param>
    /// <remarks>
    /// This constructor wraps the original exception, preserving the stack trace and error details
    /// in the <see cref="Exception.InnerException"/> property.
    /// </remarks>
    public PvNugsMediatorException(Exception e):
        base($"pvNugsMediator Exception: {e.Message}", e)
    {
    }
    
}
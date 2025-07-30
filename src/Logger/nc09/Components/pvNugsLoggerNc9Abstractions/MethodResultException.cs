namespace pvNugsLoggerNc9Abstractions;

/// <summary>
/// Represents an exception specific to method result operations.
/// This sealed class extends the base <see cref="Exception"/> class
/// to provide specialized exception handling for method result scenarios.
/// </summary>
/// <seealso cref="IMethodResult"/>
public sealed class MethodResultException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MethodResultException"/> class
    /// with a specified error message.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public MethodResultException(string message) : 
        base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MethodResultException"/> class
    /// with a specified error message and a reference to the inner exception
    /// that is the cause of this exception.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="innerException">
    /// The exception that is the cause of the current exception.
    /// </param>
    public MethodResultException(
        string message, Exception innerException) : 
        base(message, innerException)
    {
    }
}
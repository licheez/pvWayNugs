namespace pvNugsMessagingNc10Kafka;

/// <summary>
/// Represents an error raised by the pvNugs Kafka messaging provider.
/// </summary>
/// <remarks>
/// This exception provides a provider-level abstraction over errors occurring
/// while interacting with the Kafka messaging infrastructure.
///
/// When the error originates from an underlying exception, the original
/// exception is preserved through <see cref="Exception.InnerException"/>.
/// </remarks>
public sealed class PvNugsMessagingException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PvNugsMessagingException"/>
    /// class with the specified error message.
    /// </summary>
    /// <param name="message">
    /// The message describing the error.
    /// </param>
    public PvNugsMessagingException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PvNugsMessagingException"/>
    /// class from an underlying exception.
    /// </summary>
    /// <param name="exception">
    /// The exception that caused the messaging operation to fail.
    /// </param>
    public PvNugsMessagingException(Exception exception)
        : base(exception.Message, exception)
    {
    }
}
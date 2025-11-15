namespace pvNugsCryptoNc6Aes;

/// <summary>
/// Exception type used by pvNugs AES crypto implementations to surface
/// errors in a consistent, implementation-specific wrapper.
/// </summary>
/// <remarks>
/// This exception wraps lower-level exceptions (for example cryptographic
/// or I/O exceptions) so callers can catch a single well-known type when
/// interacting with pvNugs crypto components. The original exception is
/// preserved as the <see cref="System.Exception.InnerException"/> when
/// constructed with an inner exception.
/// </remarks>
public class PvWayCryptoException : Exception
{
    /// <summary>
    /// Create a new <see cref="PvWayCryptoException"/> with a custom message.
    /// </summary>
    /// <param name="message">A human-readable message describing the error condition.</param>
    public PvWayCryptoException(string message) : 
        base($"PvWayCryptoException: {message}")
    {
    }

    /// <summary>
    /// Create a new <see cref="PvWayCryptoException"/> that wraps an existing exception.
    /// </summary>
    /// <param name="e">The original exception to wrap. Its message is included and it is stored as the <see cref="System.Exception.InnerException"/>.</param>
    public PvWayCryptoException(Exception e):
        base($"PvWayCryptoException: {e.Message}", e)
    {
    }
}
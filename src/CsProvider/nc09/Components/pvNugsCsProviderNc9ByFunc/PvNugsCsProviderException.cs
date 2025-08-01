namespace pvNugsCsProviderNc9ByFunc;

/// <summary>
/// Represents errors that occur during connection string provider operations.
/// </summary>
/// <param name="e">The inner exception that caused this provider exception.</param>
/// <remarks>
/// This exception wraps other exceptions that occur during connection string operations,
/// prefixing the message with "pvNugsCsProvider:" for easier identification of the error source.
/// </remarks>
public class PvNugsCsProviderException(Exception e) : 
    Exception($"pvNugsCsProvider: {e.Message}", e);
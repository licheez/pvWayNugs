namespace pvNugsLoggerNc9Abstractions;

/// <summary>
/// Provides extension methods for exception handling and processing.
/// </summary>
public static class ExceptionExtension
{
    /// <summary>
    /// Gets a comprehensive message that includes both the exception message chain and stack trace.
    /// This method traverses through all inner exceptions to build a complete error message.
    /// </summary>
    /// <param name="e">The exception to process.</param>
    /// <returns>A string containing the complete exception message chain and stack trace.
    /// The format is "Exception: [All Exception Messages]- newline - StackTrace: [Stack Trace]"</returns>
    /// <example>
    /// <code>
    /// try
    /// {
    ///     // Some code that might throw
    /// }
    /// catch (Exception ex)
    /// {
    ///     string fullMessage = ex.GetDeepMessage();
    ///     // Log or handle the full message
    /// }
    /// </code>
    /// </example>
    public static string GetDeepMessage(this Exception e)
    {
        var message = RecursiveDeepMessage(e);
        var stackTrace = e.StackTrace;
        return $"Exception: {message}{Environment.NewLine}StackTrace: {stackTrace}";
    }
    
    /// <summary>
    /// Recursively builds a message string by concatenating messages from all nested inner exceptions.
    /// </summary>
    /// <param name="e">The exception to process recursively.</param>
    /// <returns>A string containing all exception messages from the exception chain.</returns>
    private static string RecursiveDeepMessage(Exception e)
    {
        var message = e.Message;
        if (e.InnerException != null)
            message += RecursiveDeepMessage(e.InnerException);
        return message;
    }
}
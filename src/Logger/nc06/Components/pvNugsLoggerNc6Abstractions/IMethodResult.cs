namespace pvNugsLoggerNc6Abstractions;

/// <summary>
/// Represents a method execution result with status and notifications.
/// Provides functionality to track success/failure states
/// and manage execution notifications.
/// </summary>
public interface IMethodResult
{
    /// <summary>
    /// Gets a value indicating whether the method execution resulted in a failure.
    /// Returns true if at least one notification has a severity
    /// greater or equal to Error.
    /// </summary>
    bool Failure { get; }

    /// <summary>
    /// Gets a value indicating whether the method execution was successful.
    /// Returns true if there are no notifications
    /// or all notifications have severity lower than Error.
    /// </summary>
    bool Success { get; }

    /// <summary>
    /// Gets the highest severity level among all notifications.
    /// Returns the lowest severity level if there are no notifications.
    /// </summary>
    SeverityEnu Severity { get; }

    /// <summary>
    /// Gets a concatenated string containing all notification messages.
    /// Messages in the resulting string are separated
    /// by newline characters.
    /// </summary>
    string ErrorMessage { get; }

    /// <summary>
    /// Gets the collection of all notifications
    /// associated with this method result.
    /// </summary>
    IEnumerable<IMethodResultNotification> Notifications { get; }

    /// <summary>
    /// Adds a new notification to the result
    /// with the specified message and severity level.
    /// </summary>
    /// <param name="message">The text message for the notification.</param>
    /// <param name="severity">The severity level to assign.</param>
    void AddNotification(string message, SeverityEnu severity);

    /// <summary>
    /// Adds an existing notification object to this result.
    /// </summary>
    /// <param name="notification">
    /// The pre-configured notification object to add.
    /// </param>
    void AddNotification(IMethodResultNotification notification);

    /// <summary>
    /// Creates and throws a new Exception using ErrorMessage.
    /// Useful for converting notification messages
    /// into an exception when needed.
    /// </summary>
    /// <exception cref="Exception">
    /// Always thrown containing the ErrorMessage.
    /// </exception>
    void Throw();
}

/// <summary>
/// Represents a method result that includes a typed data payload
/// along with the standard execution status and notifications.
/// </summary>
/// <typeparam name="T">
/// The type of the result data payload.
/// </typeparam>
public interface IMethodResult<out T> : IMethodResult
{
    /// <summary>
    /// Gets the data payload associated with the method result.
    /// May be null if the operation failed
    /// or did not produce any data.
    /// </summary>
    T? Data { get; }
}

/// <summary>
/// Represents a single notification within a method result,
/// containing a severity level and a message.
/// </summary>
public interface IMethodResultNotification
{
    /// <summary>
    /// Gets the severity level assigned to this notification.
    /// Used to determine the overall result status.
    /// </summary>
    SeverityEnu Severity { get; }

    /// <summary>
    /// Gets the descriptive message associated
    /// with this notification.
    /// </summary>
    string Message { get; }
}
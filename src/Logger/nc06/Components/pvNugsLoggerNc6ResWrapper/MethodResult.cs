using System.Text;
using pvNugsLoggerNc6Abstractions;

namespace pvNugsLoggerNc6ResWrapper;

/// <summary>
/// Represents the result of a method execution, including success/failure status,
/// severity level, and associated notifications.
/// Implements <see cref="IMethodResult"/>.
/// </summary>
public class MethodResult : IMethodResult
{
    /// <summary>
    /// Gets a value indicating whether the method execution resulted in a failure.
    /// True if at least one notification has a severity greater than or equal to Error.
    /// </summary>
    public bool Failure => _notifications
        .Any(n => n.Severity >= SeverityEnu.Error);

    /// <summary>
    /// Gets a value indicating whether the method execution was successful.
    /// True if there are no notifications or all notifications have severity lower than Error.
    /// </summary>
    public bool Success => !Failure;

    /// <summary>
    /// Gets the highest severity level from all notifications.
    /// Returns Ok if there are no notifications.
    /// </summary>
    public SeverityEnu Severity =>
        _notifications.Any()
            ? _notifications.Max(x => x.Severity)
            : SeverityEnu.Ok;

    /// <summary>
    /// Gets the collection of notifications associated with the method execution.
    /// </summary>
    public IEnumerable<IMethodResultNotification> Notifications => _notifications;

    /// <summary>
    /// Gets the error message by combining all notifications.
    /// </summary>
    public string ErrorMessage => ToString();

    /// <summary>
    /// Gets a successful result with no notifications.
    /// </summary>
    public static MethodResult Ok => new();

    private readonly IList<IMethodResultNotification> _notifications;

    private sealed class Notification : IMethodResultNotification
    {
        public Notification(SeverityEnu severity, string message)
        {
            Severity = severity;
            Message = message;
        }

        public SeverityEnu Severity { get; }
        public string Message { get; }

        public override string ToString()
        {
            return $"{Severity}:{Message}";
        }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MethodResult"/> class with no notifications.
    /// </summary>
    public MethodResult()
    {
        _notifications = new List<IMethodResultNotification>();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MethodResult"/> class by copying notifications from another result.
    /// </summary>
    /// <param name="res">The source result to copy notifications from.</param>
    public MethodResult(IMethodResult res)
        : this()
    {
        foreach (var notification in res.Notifications)
        {
            AddNotification(notification);
        }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MethodResult"/> class with a single notification.
    /// </summary>
    /// <param name="message">The notification message.</param>
    /// <param name="severity">The severity level of the notification.</param>
    public MethodResult(string message, SeverityEnu severity) :
        this()
    {
        AddNotification(message, severity);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MethodResult"/> class from an exception.
    /// </summary>
    /// <param name="e">The exception to create the notification from.</param>
    /// <param name="severity">The severity level for the notification (defaults to Fatal).</param>
    public MethodResult(Exception e, SeverityEnu severity = SeverityEnu.Fatal)
        : this(e.GetDeepMessage(), severity)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MethodResult"/> class with multiple notifications.
    /// </summary>
    /// <param name="messages">The collection of notification messages.</param>
    /// <param name="severity">The severity level to apply to all notifications.</param>
    public MethodResult(IEnumerable<string> messages, SeverityEnu severity) :
        this()
    {
        foreach (var message in messages)
        {
            AddNotification(message, severity);
        }
    }

    /// <summary>
    /// Adds a new notification with the specified message and severity.
    /// </summary>
    /// <param name="message">The notification message.</param>
    /// <param name="severity">The severity level of the notification.</param>
    public void AddNotification(string message, SeverityEnu severity)
    {
        AddNotification(new Notification(severity, message));
    }

    /// <summary>
    /// Adds a pre-constructed notification to the result.
    /// </summary>
    /// <param name="notification">The notification to add.</param>
    public void AddNotification(IMethodResultNotification notification)
    {
        _notifications.Add(notification);
    }

    /// <summary>
    /// Throws a <see cref="MethodResultException"/> with the current error message.
    /// </summary>
    public void Throw()
    {
        throw new MethodResultException(ErrorMessage);
    }

    
    /// <summary>
    /// Returns a string representation of the method result, combining all notifications.
    /// Each notification is converted to a string and separated by a new line.
    /// </summary>
    /// <returns>A string containing all notifications, one per line. Returns an empty string if there are no notifications.</returns>
    public override string ToString()
    {
        var sb = new StringBuilder();
        foreach (var notification in _notifications)
        {
            if (sb.Length > 0) 
                sb.Append(Environment.NewLine);
            sb.Append(notification);
        }
        return sb.ToString();
    }
}

/// <summary>
/// Represents a generic method execution result that includes a typed return value along with execution status information.
/// Implements <see cref="IMethodResult{T}"/>.
/// </summary>
/// <typeparam name="T">The type of the result data.</typeparam>
public class MethodResult<T> : MethodResult, IMethodResult<T>
{
    /// <summary>
    /// Gets the result data of type T.
    /// </summary>
    public T? Data { get; }

    /// <summary>
    /// Gets a result instance with default(T) as data.
    /// </summary>
    public static IMethodResult<T> Null => new MethodResult<T>(default(T));

    /// <summary>
    /// Initializes a new instance of the <see cref="MethodResult{T}"/> class with the specified data.
    /// </summary>
    /// <param name="data">The result data.</param>
    public MethodResult(T? data)
    {
        Data = data;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MethodResult{T}"/> class by copying notifications from another result.
    /// </summary>
    /// <param name="methodResult">The source result to copy notifications from.</param>
    public MethodResult(IMethodResult methodResult) :
        base(methodResult)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MethodResult{T}"/> class with a single notification.
    /// </summary>
    /// <param name="message">The notification message.</param>
    /// <param name="severity">The severity level of the notification.</param>
    public MethodResult(string message, SeverityEnu severity)
        : base(message, severity)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MethodResult{T}"/> class with multiple notifications.
    /// </summary>
    /// <param name="messages">The collection of notification messages.</param>
    /// <param name="severity">The severity level to apply to all notifications.</param>
    public MethodResult(IEnumerable<string> messages, SeverityEnu severity)
        : base(messages, severity)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MethodResult{T}"/> class from an exception.
    /// </summary>
    /// <param name="e">The exception to create the notification from.</param>
    /// <param name="severity">The severity level for the notification (defaults to Fatal).</param>
    public MethodResult(Exception e, SeverityEnu severity = SeverityEnu.Fatal)
        : base(e, severity)
    {
    }
}
using pvNugsEnumConvNc9;
using pvNugsLoggerNc9Abstractions;

// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace pvNugsLoggerNc9ResWrapper;

/// <summary>
/// Represents a notification message within an HTTP result, containing severity and message information.
/// </summary>
public class DsoHttpResultNotification
{
    /// <summary>
    /// Gets the severity code string representation derived from the severity enum value.
    /// The code is obtained using the Description attribute of the severity enum.
    /// </summary>
    public string SeverityCode { get; }

    /// <summary>
    /// Gets the notification message text.
    /// </summary>
    public string Message { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="DsoHttpResultNotification"/> class.
    /// </summary>
    /// <param name="severity">The severity level of the notification.</param>
    /// <param name="message">The notification message text.</param>
    public DsoHttpResultNotification(
        SeverityEnu severity,
        string message)
    {
        SeverityCode = severity.GetCode();
        Message = message;
    }
}
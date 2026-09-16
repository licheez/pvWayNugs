namespace pvNugsMessagingNc10Abstractions;

/// <summary>
/// Specifies the availability status of a communication monitoring metric.
/// </summary>
public enum PvNugsCommunicationMetricStatusEnu
{
    /// <summary>
    /// The metric is supported and its current value is available.
    /// </summary>
    Available,

    /// <summary>
    /// The metric is supported by the underlying communication technology,
    /// but does not apply in the current context.
    /// </summary>
    /// <remarks>
    /// For example, a pending message count may not be applicable when no
    /// consumer group has been specified.
    /// </remarks>
    NotApplicable,

    /// <summary>
    /// The metric cannot be provided because it is not supported by the
    /// underlying communication technology.
    /// </summary>
    /// <remarks>
    /// For example, Redis Pub/Sub does not maintain a backlog of messages
    /// for disconnected subscribers.
    /// </remarks>
    NotSupported,

    /// <summary>
    /// The metric is normally supported and applicable, but its current
    /// value could not be retrieved.
    /// </summary>
    /// <remarks>
    /// This may occur, for example, when the communication infrastructure
    /// is temporarily unreachable or an error occurs while retrieving
    /// the metric.
    /// </remarks>
    Unavailable
}
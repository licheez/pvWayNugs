namespace pvNugsMessagingNc10Abstractions;

/// <summary>
/// Represents the current observable status of a communication topic,
/// optionally from the perspective of a specific consumer group.
/// </summary>
/// <remarks>
/// The information exposed by this class represents the current state
/// reported by the underlying communication infrastructure. Individual
/// metrics may not be applicable, supported, or available depending on
/// the communication technology and the supplied consumer group.
/// </remarks>
// ReSharper disable once ClassNeverInstantiated.Global
public sealed class PvNugsCommunicationTopicStatus
{
    /// <summary>
    /// Gets the name of the topic being observed.
    /// </summary>
    public required string Topic { get; init; }

    /// <summary>
    /// Gets the consumer group for which the topic status was requested,
    /// or <see langword="null"/> when no consumer group was specified.
    /// </summary>
    /// <remarks>
    /// Some metrics, such as the number of pending messages, may only be
    /// meaningful when evaluated in the context of a consumer group.
    /// </remarks>
    public string? ConsumerGroup { get; init; }

    /// <summary>
    /// Gets the number of messages currently waiting to be consumed
    /// by the specified consumer group.
    /// </summary>
    /// <remarks>
    /// The availability and meaning of this metric depend on the underlying
    /// communication technology and on whether a consumer group was specified.
    /// </remarks>
    public required PvNugsCommunicationMetric<long> PendingMessages { get; init; }

    /// <summary>
    /// Gets the number of consumers currently active for the specified
    /// topic and consumer group.
    /// </summary>
    /// <remarks>
    /// The underlying communication technology determines how active
    /// consumers are identified and whether this information is available.
    /// </remarks>
    public required PvNugsCommunicationMetric<int> ActiveConsumers { get; init; }

    /// <summary>
    /// Gets the timestamp of the most recently observed activity for the
    /// specified topic and consumer group.
    /// </summary>
    /// <remarks>
    /// The definition and availability of activity information may vary
    /// between communication technologies.
    /// </remarks>
    public required PvNugsCommunicationMetric<DateTimeOffset> LastActivity { get; init; }
}
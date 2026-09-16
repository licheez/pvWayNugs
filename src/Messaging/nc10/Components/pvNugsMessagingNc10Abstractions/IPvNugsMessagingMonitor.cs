namespace pvNugsMessagingNc10Abstractions;

/// <summary>
/// Provides access to the current observable state of the underlying
/// messaging infrastructure.
/// </summary>
/// <remarks>
/// Implementations should retrieve status information directly from the
/// underlying messaging technology whenever possible, without maintaining
/// application-specific monitoring state.
/// 
/// The availability of individual metrics may vary depending on the
/// capabilities of the underlying messaging technology.
/// </remarks>
public interface IPvNugsMessagingMonitor
{
    /// <summary>
    /// Retrieves the current observable status of a topic, optionally from
    /// the perspective of a specific consumer group.
    /// </summary>
    /// <param name="topic">
    /// The name of the topic to observe.
    /// </param>
    /// <param name="consumerGroup">
    /// The consumer group for which consumer-specific information should be
    /// retrieved, or <see langword="null"/> to request topic-level information only.
    /// </param>
    /// <param name="cancellationToken">
    /// A token that can be used to cancel the asynchronous operation.
    /// </param>
    /// <returns>
    /// A task representing the asynchronous operation. The task result contains
    /// the current observable status of the requested topic.
    /// </returns>
    /// <remarks>
    /// Metrics that require a consumer group may be reported as
    /// <see cref="PvNugsCommunicationMetricStatusEnu.NotApplicable"/> when
    /// <paramref name="consumerGroup"/> is not specified.
    ///
    /// Metrics that cannot be provided by the underlying messaging technology
    /// may be reported as
    /// <see cref="PvNugsCommunicationMetricStatusEnu.NotSupported"/>.
    /// </remarks>
    Task<PvNugsCommunicationTopicStatus> GetTopicStatusAsync(
        string topic,
        string? consumerGroup = null,
        CancellationToken cancellationToken = default);
}
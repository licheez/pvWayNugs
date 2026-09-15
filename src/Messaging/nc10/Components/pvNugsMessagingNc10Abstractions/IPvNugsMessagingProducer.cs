namespace pvNugsMessagingNc10Abstractions;

/// <summary>
/// Defines the contract for publishing messages to a topic using a broker-neutral result.
/// </summary>
/// <remarks>
/// Implementations are responsible for serializing and forwarding the message to the configured broker.
/// The publish operation returns a <see cref="PvNugsPublishResult"/> containing broker-agnostic metadata,
/// such as the message identifier and any optional broker-specific details like offset or partition.
/// </remarks>
public interface IPvNugsMessagingProducer
{
    /// <summary>
    /// Publishes a raw string message to the specified topic.
    /// </summary>
    /// <param name="topic">The target topic to publish to.</param>
    /// <param name="message">The message payload to send.</param>
    /// <param name="cancellationToken">
    /// A token that can be used to cancel the publish operation.
    /// </param>
    /// <returns>
    /// A <see cref="PvNugsPublishResult"/> containing the publication metadata, including any broker-specific values that are available.
    /// </returns>
    Task<PvNugsPublishResult> PublishAsync(
        string topic,
        string message,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Publishes a strongly typed message object to the specified topic.
    /// </summary>
    /// <typeparam name="T">The type of the message payload.</typeparam>
    /// <param name="topic">The target topic to publish to.</param>
    /// <param name="message">The message object to serialize and publish.</param>
    /// <param name="cancellationToken">
    /// A token that can be used to cancel the publish operation.
    /// </param>
    /// <returns>
    /// A <see cref="PvNugsPublishResult"/> containing the publication metadata, including any broker-specific values that are available.
    /// </returns>
    Task<PvNugsPublishResult> PublishAsync<T>(
        string topic,
        T message,
        CancellationToken cancellationToken = default);
}

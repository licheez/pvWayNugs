namespace pvNugsMessagingNc10Abstractions;

/// <summary>
/// Defines the contract for publishing messages to a messaging topic.
/// </summary>
/// <remarks>
/// Implementations are responsible for forwarding messages to the underlying
/// messaging infrastructure and returning publication metadata through the
/// broker-neutral <see cref="PvNugsPublishResult"/> contract.
///
/// The exact publication semantics and the metadata available in the result
/// depend on the capabilities of the underlying messaging technology.
/// </remarks>
public interface IPvNugsMessagingProducer
{
    /// <summary>
    /// Publishes a raw string message to the specified topic.
    /// </summary>
    /// <param name="topic">
    /// The topic to which the message should be published.
    /// </param>
    /// <param name="message">
    /// The raw message payload to publish.
    /// </param>
    /// <param name="cancellationToken">
    /// A token that can be used to cancel the publish operation.
    /// </param>
    /// <returns>
    /// A task representing the asynchronous operation. The task result contains
    /// the publication metadata made available by the underlying messaging
    /// technology.
    /// </returns>
    Task<PvNugsPublishResult> PublishAsync(
        string topic,
        string message,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Publishes a strongly typed message to the specified topic.
    /// </summary>
    /// <typeparam name="T">
    /// The type of the message payload.
    /// </typeparam>
    /// <param name="topic">
    /// The topic to which the message should be published.
    /// </param>
    /// <param name="message">
    /// The message object to publish.
    /// </param>
    /// <param name="cancellationToken">
    /// A token that can be used to cancel the publish operation.
    /// </param>
    /// <returns>
    /// A task representing the asynchronous operation. The task result contains
    /// the publication metadata made available by the underlying messaging
    /// technology.
    /// </returns>
    /// <remarks>
    /// The concrete provider is responsible for converting the message into
    /// the representation required by the underlying messaging technology.
    /// </remarks>
    Task<PvNugsPublishResult> PublishAsync<T>(
        string topic,
        T message,
        CancellationToken cancellationToken = default);
}
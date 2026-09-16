namespace pvNugsMessagingNc10Abstractions;

/// <summary>
/// Defines the contract for subscribing to a message topic and processing
/// incoming messages.
/// </summary>
/// <remarks>
/// Implementations are responsible for registering a callback that receives
/// message payloads and their associated publication metadata.
///
/// Multiple subscriptions may use the same consumer group to represent
/// instances of the same logical consumer. The underlying messaging provider
/// determines how messages are distributed between those instances.
/// </remarks>
public interface IPvNugsMessagingConsumer
{
    /// <summary>
    /// Subscribes to a topic and starts processing incoming messages.
    /// </summary>
    /// <param name="topic">
    /// The topic to subscribe to.
    /// </param>
    /// <param name="handleIncomingMessageAsync">
    /// A callback invoked for each incoming message. The callback receives
    /// the raw message payload and its associated publication metadata,
    /// and returns <see langword="true"/> when the message was processed
    /// successfully.
    /// </param>
    /// <param name="consumerGroup">
    /// The logical consumer group associated with the subscription,
    /// or <see langword="null"/> to use the provider's default behavior.
    /// </param>
    /// <param name="cancellationToken">
    /// A token that can be used to cancel the subscription operation.
    /// </param>
    /// <returns>
    /// A task representing the asynchronous operation. The task result contains
    /// a unique identifier for the created subscription.
    /// </returns>
    /// <remarks>
    /// Subscriptions using the same consumer group represent instances of the
    /// same logical consumer and may share the processing workload when supported
    /// by the underlying messaging technology.
    ///
    /// Subscriptions using different consumer groups are logically independent
    /// and may process the same published messages independently.
    /// </remarks>
    Task<Guid> SubscribeAsync(
        string topic,
        Func<string, PvNugsPublishResult, Task<bool>> handleIncomingMessageAsync,
        string? consumerGroup = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes an existing subscription for the specified subscriber.
    /// </summary>
    /// <param name="subscriberId">
    /// The unique identifier of the subscription to remove.
    /// </param>
    /// <param name="cancellationToken">
    /// A token that can be used to cancel the unsubscription operation.
    /// </param>
    /// <returns>
    /// A task that completes when the subscription has been removed.
    /// </returns>
    Task UnsubscribeAsync(
        Guid subscriberId,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Defines the contract for subscribing to a message topic and converting
/// incoming messages into instances of a specific domain type.
/// </summary>
/// <typeparam name="T">
/// The target type produced from each incoming message payload.
/// </typeparam>
/// <remarks>
/// Multiple subscriptions may use the same consumer group to represent
/// instances of the same logical consumer. The underlying messaging provider
/// determines how messages are distributed between those instances.
/// </remarks>
public interface IPvNugsMessagingConsumer<T> where T : class
{
    /// <summary>
    /// Subscribes to a topic and converts each incoming message payload into
    /// an instance of <typeparamref name="T"/>.
    /// </summary>
    /// <param name="topic">
    /// The topic to subscribe to.
    /// </param>
    /// <param name="factorAsync">
    /// A factory that asynchronously converts the raw message payload into
    /// an instance of <typeparamref name="T"/>.
    /// </param>
    /// <param name="handleIncomingMessageAsync">
    /// A callback invoked for each converted message. The callback receives
    /// the raw message payload, its associated publication metadata, and the
    /// materialized <typeparamref name="T"/> instance, and returns
    /// <see langword="true"/> when the message was processed successfully.
    /// </param>
    /// <param name="consumerGroup">
    /// The logical consumer group associated with the subscription,
    /// or <see langword="null"/> to use the provider's default behavior.
    /// </param>
    /// <param name="cancellationToken">
    /// A token that can be used to cancel the subscription operation.
    /// </param>
    /// <returns>
    /// A task representing the asynchronous operation. The task result contains
    /// a unique identifier for the created subscription.
    /// </returns>
    /// <remarks>
    /// Subscriptions using the same consumer group represent instances of the
    /// same logical consumer and may share the processing workload when supported
    /// by the underlying messaging technology.
    ///
    /// Subscriptions using different consumer groups are logically independent
    /// and may process the same published messages independently.
    /// </remarks>
    Task<Guid> SubscribeAsync(
        string topic,
        Func<string, Task<T>> factorAsync,
        Func<string, PvNugsPublishResult, T, Task<bool>> handleIncomingMessageAsync,
        string? consumerGroup = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes an existing typed subscription for the specified subscriber.
    /// </summary>
    /// <param name="subscriberId">
    /// The unique identifier of the subscription to remove.
    /// </param>
    /// <param name="cancellationToken">
    /// A token that can be used to cancel the unsubscription operation.
    /// </param>
    /// <returns>
    /// A task that completes when the subscription has been removed.
    /// </returns>
    Task UnsubscribeAsync(
        Guid subscriberId,
        CancellationToken cancellationToken = default);
}
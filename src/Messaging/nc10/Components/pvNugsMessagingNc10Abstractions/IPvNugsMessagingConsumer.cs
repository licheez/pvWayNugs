namespace pvNugsMessagingNc10Abstractions;

/// <summary>
/// Defines the contract for subscribing to a message topic and processing incoming messages.
/// </summary>
/// <remarks>
/// Implementations are responsible for registering a callback that receives message payloads,
/// the associated publish result metadata, and returning whether the message was handled successfully.
/// </remarks>
public interface IPvNugsMessagingConsumer
{
    /// <summary>
    /// Subscribes to a topic and starts processing incoming messages.
    /// </summary>
    /// <param name="topic">The topic to subscribe to.</param>
    /// <param name="handleIncomingMessageAsync">
    /// A callback invoked for each incoming message. The callback receives the raw message payload,
    /// the associated publish result metadata, and returns <see langword="true"/> when the message was processed successfully.
    /// </param>
    /// <param name="cancellationToken">
    /// A token that can be used to cancel the subscription operation.
    /// </param>
    /// <returns>
    /// A unique identifier for the created subscription.
    /// </returns>
    Task<Guid> SubscribeAsync(
        string topic,
        Func<string, PvNugsPublishResult, Task<bool>> handleIncomingMessageAsync,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes an existing subscription for the specified subscriber.
    /// </summary>
    /// <param name="subscriberId">The unique identifier of the subscription to remove.</param>
    /// <param name="cancellationToken">
    /// A token that can be used to cancel the unsubscription operation.
    /// </param>
    /// <returns>A task that completes when the subscription has been removed.</returns>
    Task UnsubscribeAsync(
        Guid subscriberId,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Defines the contract for subscribing to a message topic and deserializing incoming messages into a specific domain type.
/// </summary>
/// <typeparam name="T">The target type produced from each incoming message payload.</typeparam>
public interface IPvNugsMessagingConsumer<T> where T : class
{
    /// <summary>
    /// Subscribes to a topic and converts each incoming message payload into an instance of <typeparamref name="T"/>.
    /// </summary>
    /// <param name="topic">The topic to subscribe to.</param>
    /// <param name="factorAsync">
    /// A factory that converts the raw message payload into an instance of <typeparamref name="T"/>.
    /// </param>
    /// <param name="handleIncomingMessageAsync">
    /// A callback invoked for each deserialized message. The callback receives the raw payload,
    /// the associated publish result metadata, and the materialized object instance.
    /// </param>
    /// <param name="cancellationToken">
    /// A token that can be used to cancel the subscription operation.
    /// </param>
    /// <returns>
    /// A unique identifier for the created subscription.
    /// </returns>
    Task<Guid> SubscribeAsync(
        string topic,
        Func<string, Task<T>> factorAsync,
        Func<string, PvNugsPublishResult, T, Task<bool>> handleIncomingMessageAsync,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes an existing generic subscription for the specified subscriber.
    /// </summary>
    /// <param name="subscriberId">The unique identifier of the subscription to remove.</param>
    /// <param name="cancellationToken">
    /// A token that can be used to cancel the unsubscription operation.
    /// </param>
    /// <returns>A task that completes when the subscription has been removed.</returns>
    Task UnsubscribeAsync(
        Guid subscriberId,
        CancellationToken cancellationToken = default);
}

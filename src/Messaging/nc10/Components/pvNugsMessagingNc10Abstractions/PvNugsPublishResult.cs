namespace pvNugsMessagingNc10Abstractions;

/// <summary>
/// Represents the publication metadata returned after a message has been
/// published to the underlying messaging infrastructure.
/// </summary>
/// <remarks>
/// This type is intentionally broker-neutral. Concrete provider implementations
/// populate the metadata supported by the underlying messaging technology and
/// leave optional values unset when they are not available.
/// </remarks>
// ReSharper disable once ClassNeverInstantiated.Global
public sealed class PvNugsPublishResult
{
    /// <summary>
    /// Gets the identifier associated with the published message.
    /// </summary>
    /// <remarks>
    /// The exact format and uniqueness guarantees of the identifier depend on
    /// the underlying messaging technology and provider implementation.
    /// </remarks>
    public string MessageId { get; init; } = string.Empty;

    /// <summary>
    /// Gets the position of the published message within its message stream,
    /// when such information is provided by the underlying messaging technology.
    /// </summary>
    public long? Offset { get; init; }

    /// <summary>
    /// Gets the partition to which the message was published, when the underlying
    /// messaging technology exposes a partitioning concept.
    /// </summary>
    public int? Partition { get; init; }

    /// <summary>
    /// Gets the timestamp associated with the publication of the message.
    /// </summary>
    /// <remarks>
    /// The exact semantics of this timestamp depend on the provider implementation.
    /// </remarks>
    public DateTimeOffset PublishedAt { get; init; } = DateTimeOffset.UtcNow;
}
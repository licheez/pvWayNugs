namespace pvNugsMessagingNc10Abstractions;

/// <summary>
/// Represents the metadata returned after publishing a message to a broker.
/// </summary>
/// <remarks>
/// This type is intentionally broker-neutral. Concrete implementations can populate the fields they support,
/// while leaving other values unset when the underlying system does not expose them.
/// </remarks>
// ReSharper disable once ClassNeverInstantiated.Global
public sealed class PvNugsPublishResult
{
    /// <summary>
    /// Gets the unique identifier assigned to the published message.
    /// </summary>
    public string MessageId { get; init; } = string.Empty;

    /// <summary>
    /// Gets the broker-specific offset associated with the published message when available.
    /// </summary>
    public long? Offset { get; init; }

    /// <summary>
    /// Gets the partition associated with the published message when available.
    /// </summary>
    public int? Partition { get; init; }

    /// <summary>
    /// Gets the timestamp when the message was published.
    /// </summary>
    public DateTimeOffset PublishedAt { get; init; } = DateTimeOffset.UtcNow;
}
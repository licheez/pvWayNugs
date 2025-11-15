namespace pvNugsCryptoNc6Abstractions;

/// <summary>
/// Represents an ephemeral encrypted payload wrapper that contains
/// the underlying data and an expiration time.
/// </summary>
/// <typeparam name="T">The type of the wrapped data.</typeparam>
/// <remarks>
/// Implementations typically use this interface for ephemeral (time-limited)
/// encrypted payloads where <see cref="ValidUntil"/> indicates when the
/// payload should be considered expired and no longer usable.
/// </remarks>
internal interface ICryptoEphemeral<out T>
{
    /// <summary>
    /// Gets the date and time after which the payload is considered expired.
    /// </summary>
    /// <remarks>
    /// The specific kind (UTC/local) returned by implementations should be
    /// documented by the concrete type; callers should treat this value as the
    /// authoritative expiration moment for the associated <see cref="Data"/>.
    /// </remarks>
    DateTime ValidUntil { get; }

    /// <summary>
    /// Gets the underlying data contained in the ephemeral payload.
    /// </summary>
    T Data { get; }
}
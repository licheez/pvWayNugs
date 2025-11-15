// ReSharper disable MemberCanBePrivate.Global
namespace pvNugsCryptoNc6Aes;

/// <summary>
/// Internal wrapper type used to produce ephemeral (time-limited) payloads
/// for encryption. The wrapper contains the payload data and the moment
/// until which the payload is considered valid.
/// </summary>
/// <typeparam name="T">Type of the enclosed data.</typeparam>
/// <remarks>
/// Instances of this type are serialized and encrypted by the public
/// crypto implementation. The <see cref="ValidUntil"/> timestamp is
/// expressed in UTC (<see cref="DateTime.Kind"/> is <see cref="DateTimeKind.Utc"/>).
/// The constructor that takes a <see cref="TimeSpan"/> computes the
/// expiration as DateTime.UtcNow + validity.
/// </remarks>
internal class CryptoEphemeral<T> : ICryptoEphemeral<T>
{
    /// <summary>
    /// The moment (UTC) after which the payload should be considered expired
    /// and no longer usable.
    /// </summary>
    public DateTime ValidUntil { get; set; }

    /// <summary>
    /// The wrapped payload data.
    /// </summary>
    public T Data { get; set; } = default!;

    // ReSharper disable once UnusedMember.Global
    /// <summary>
    /// Parameterless constructor required for deserialization.
    /// </summary>
    public CryptoEphemeral()
    {
    }

    /// <summary>
    /// Create a new ephemeral payload that will expire after the provided <paramref name="validity"/> period.
    /// </summary>
    /// <param name="data">The payload data to wrap.</param>
    /// <param name="validity">The time span from now after which the payload will be considered expired.</param>
    /// <remarks>
    /// The expiration timestamp is computed as <c>DateTime.UtcNow + validity</c> and stored in <see cref="ValidUntil"/>.
    /// </remarks>
    public CryptoEphemeral(T data, TimeSpan validity)
    {
        ValidUntil = DateTime.UtcNow + validity;
        Data = data;
    }
}
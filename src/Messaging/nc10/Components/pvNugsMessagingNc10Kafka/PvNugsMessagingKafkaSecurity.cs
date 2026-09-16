namespace pvNugsMessagingNc10Kafka;

/// <summary>
/// Represents the resolved security information required to connect
/// to the Kafka infrastructure.
/// </summary>
/// <remarks>
/// Instances of this class contain the resolved SASL credentials and
/// SSL certificate verification setting used when configuring Kafka clients.
/// </remarks>
internal sealed class PvNugsMessagingKafkaSecurity
{
    /// <summary>
    /// Initializes a new instance of the
    /// <see cref="PvNugsMessagingKafkaSecurity"/> class.
    /// </summary>
    /// <param name="saslUsername">
    /// The SASL username used to authenticate with Kafka.
    /// </param>
    /// <param name="saslPassword">
    /// The SASL password used to authenticate with Kafka.
    /// </param>
    /// <param name="enableSslCertificateVerification">
    /// Indicates whether SSL certificate verification is enabled.
    /// </param>
    public PvNugsMessagingKafkaSecurity(
        string saslUsername,
        string saslPassword,
        bool enableSslCertificateVerification)
    {
        SaslUsername = saslUsername;
        SaslPassword = saslPassword;
        EnableSslCertificateVerification =
            enableSslCertificateVerification;
    }

    /// <summary>
    /// Gets the SASL username used to authenticate with Kafka.
    /// </summary>
    public string SaslUsername { get; }

    /// <summary>
    /// Gets the SASL password used to authenticate with Kafka.
    /// </summary>
    public string SaslPassword { get; }

    /// <summary>
    /// Gets a value indicating whether SSL certificate verification
    /// is enabled when connecting to Kafka.
    /// </summary>
    public bool EnableSslCertificateVerification { get; }
}
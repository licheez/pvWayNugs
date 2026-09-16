namespace pvNugsMessagingNc10Kafka;

/// <summary>
/// Defines the configuration used to retrieve Kafka security credentials.
/// </summary>
/// <remarks>
/// The configured parameter names identify the values required to retrieve
/// the Kafka SASL credentials from the configured secret provider.
/// </remarks>
// ReSharper disable once ClassNeverInstantiated.Global
public sealed class PvNugsMessagingKafkaSecurityConfig
{
    /// <summary>
    /// Gets the name of the configuration section used to bind
    /// <see cref="PvNugsMessagingKafkaSecurityConfig"/> settings.
    /// </summary>
    public const string Section =
        nameof(PvNugsMessagingKafkaSecurityConfig);

    /// <summary>
    /// Gets or sets the parameters used to retrieve the Kafka SASL username.
    /// </summary>
    public Dictionary<string, string> SaslUsernameParams { get; set; } = new ();

    /// <summary>
    /// Gets or sets the parameters used to retrieve the Kafka SASL password.
    /// </summary>
    public Dictionary<string, string> SaslPasswordParams { get; set; } = new ();

    /// <summary>
    /// Gets or sets a value indicating whether SSL certificate verification
    /// is enabled when connecting to Kafka.
    /// </summary>
    /// <remarks>
    /// The default value is <see langword="true"/>.
    /// Disabling certificate verification should only be used in controlled
    /// environments where certificate validation cannot be performed.
    /// </remarks>
    public bool EnableSslCertificateVerification { get; set; } = true;
}
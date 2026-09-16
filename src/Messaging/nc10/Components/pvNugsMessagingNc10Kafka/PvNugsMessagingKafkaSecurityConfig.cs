// ReSharper disable CollectionNeverUpdated.Global

using Confluent.Kafka;

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
    /// Gets or sets the authentication and transport security mode used
    /// when connecting to Kafka.
    /// </summary>
    /// <remarks>
    /// The default value is
    /// <see cref="PvNugsMessagingKafkaSecurityMode.SaslSsl"/>.
    ///
    /// Use <see cref="PvNugsMessagingKafkaSecurityMode.None"/> to connect
    /// without SASL authentication.
    ///
    /// Use <see cref="PvNugsMessagingKafkaSecurityMode.SaslPlaintext"/> to
    /// authenticate with SASL without SSL/TLS encryption.
    /// </remarks>
    public PvNugsMessagingKafkaSecurityMode Mode { get; set; } =
        PvNugsMessagingKafkaSecurityMode.SaslSsl;
    
    /// <summary>
    /// Gets or sets a value indicating whether SSL certificate verification
    /// is enabled when connecting to Kafka over SSL/TLS.
    /// </summary>
    /// <remarks>
    /// The default value is <see langword="true"/>.
    /// This setting only applies when <see cref="Mode"/> is
    /// <see cref="PvNugsMessagingKafkaSecurityMode.SaslSsl"/>.
    ///
    /// Disabling certificate verification should only be used in controlled
    /// environments where certificate validation cannot be performed.
    /// </remarks>
    public bool EnableSslCertificateVerification { get; set; } = true;
    
}
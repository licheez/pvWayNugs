namespace pvNugsMessagingNc10Kafka;

/// <summary>
/// Defines the authentication and transport security mode used
/// when connecting to Kafka.
/// </summary>
public enum PvNugsMessagingKafkaSecurityMode
{
    /// <summary>
    /// Connects to Kafka without SASL authentication or SSL/TLS encryption.
    /// </summary>
    None,

    /// <summary>
    /// Uses SASL authentication without SSL/TLS encryption.
    /// </summary>
    SaslPlaintext,

    /// <summary>
    /// Uses SASL authentication over an SSL/TLS encrypted connection.
    /// </summary>
    SaslSsl
}
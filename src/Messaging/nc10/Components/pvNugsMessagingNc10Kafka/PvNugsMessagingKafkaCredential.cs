using Confluent.Kafka;

namespace pvNugsMessagingNc10Kafka;

internal sealed class PvNugsMessagingKafkaCredential(
    string saslUsername, string saslPassword,
    SecurityProtocol securityProtocol,
    SaslMechanism saslMechanism, 
    bool enableSslCertificateVerification)
{
    public SecurityProtocol SecurityProtocol { get; } = securityProtocol;
    public SaslMechanism SaslMechanism { get; } = saslMechanism;
    public bool EnableSslCertificateVerification { get; } = enableSslCertificateVerification;
    public string SaslUsername { get; } = saslUsername;
    public string SaslPassword { get; } = saslPassword;
}
using Confluent.Kafka;

namespace pvNugsMessagingNc10Kafka;

public class PvNugsMessagingKafkaCredentialConfig
{
    public const string Section = nameof(PvNugsMessagingKafkaCredentialConfig);
    
    public Dictionary<string, string> SaslUserNameParams { get; set; } = new ();
    public Dictionary<string, string> SaslPasswordParams { get; set; } = new ();

    public bool EnableSslCertificateVerification { get; set; } = true;
    public SecurityProtocol SecurityProtocol { get; set; } = SecurityProtocol.SaslSsl;
    public SaslMechanism SaslMechanism { get; set; } = SaslMechanism.Plain;
}
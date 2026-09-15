using Confluent.Kafka;

namespace pvNugsMessagingNc10Kafka;

public class PvNugsMessagingKafkaProducerConfig
{
    public const string Section = nameof(PvNugsMessagingKafkaProducerConfig);
    
    public bool IsLocal =>BootstrapServers.StartsWith("localhost", StringComparison.InvariantCultureIgnoreCase) 
                          || BootstrapServers.StartsWith("127.0.0.1", StringComparison.InvariantCulture);
    public string BootstrapServers { get; set; } = string.Empty;
    
    public TimeSpan RetryBackoff { get; set; } = TimeSpan.FromMilliseconds(100);
    
    public int MessageSendMaxRetries { get; set; } = 5;
    
    public bool Dummy { get; set; } = false;
    public Acks Acks { get; set; } = Acks.All;
    public bool EnableIdempotence { get; set; } = true;
}
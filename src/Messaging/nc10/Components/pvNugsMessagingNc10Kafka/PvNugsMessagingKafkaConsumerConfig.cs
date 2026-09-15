using Confluent.Kafka;

namespace pvNugsMessagingNc10Kafka;

public class PvNugsMessagingKafkaConsumerConfig
{
    public const string Section = nameof(PvNugsMessagingKafkaConsumerConfig);

    public bool IsLocal =>BootstrapServers.StartsWith("localhost", StringComparison.InvariantCultureIgnoreCase) 
                          || BootstrapServers.StartsWith("127.0.0.1", StringComparison.InvariantCulture);
    public string BootstrapServers { get; set; } = string.Empty;
    
    public TimeSpan Timeout { get; set; } = TimeSpan.FromHours(1);
    
    public bool Dummy { get; set; } = false;
    public AutoOffsetReset AutoOffsetReset { get; set; } = Confluent.Kafka.AutoOffsetReset.Earliest;
    public bool EnableAutoCommit { get; set; } = false;
    public IsolationLevel IsolationLevel { get; set; } = IsolationLevel.ReadCommitted;
}
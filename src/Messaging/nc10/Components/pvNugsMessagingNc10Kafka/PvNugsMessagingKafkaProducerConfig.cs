using Confluent.Kafka;

namespace pvNugsMessagingNc10Kafka;

/// <summary>
/// Defines the configuration used by the Kafka messaging producer.
/// </summary>
/// <remarks>
/// This configuration contains settings that are specific to Kafka producers.
///
/// Kafka infrastructure settings shared with other messaging components are
/// defined in <see cref="PvNugsMessagingKafkaConfig"/>.
/// </remarks>
public sealed class PvNugsMessagingKafkaProducerConfig
{
    /// <summary>
    /// Gets the name of the configuration section used to bind
    /// <see cref="PvNugsMessagingKafkaProducerConfig"/> settings.
    /// </summary>
    public const string Section =
        nameof(PvNugsMessagingKafkaProducerConfig);

    /// <summary>
    /// Gets or sets the maximum backoff interval used between message
    /// delivery retries.
    /// </summary>
    /// <remarks>
    /// This value is mapped to the Kafka <c>retry.backoff.max.ms</c>
    /// producer setting.
    ///
    /// The default value is 100 milliseconds.
    /// </remarks>
    public TimeSpan RetryBackoffMax { get; set; } =
        TimeSpan.FromMilliseconds(100);

    /// <summary>
    /// Gets or sets the maximum number of retries performed when sending
    /// a message fails.
    /// </summary>
    /// <remarks>
    /// The default value is 5.
    /// </remarks>
    public int MessageSendMaxRetries { get; set; } = 5;

    /// <summary>
    /// Gets or sets a value indicating whether the producer should operate
    /// in dummy mode instead of publishing messages to Kafka.
    /// </summary>
    public bool Dummy { get; set; }

    /// <summary>
    /// Gets or sets the Kafka acknowledgement level required for
    /// published messages.
    /// </summary>
    /// <remarks>
    /// The default value is <see cref="Confluent.Kafka.Acks.All"/>,
    /// requiring acknowledgement from all in-sync replicas.
    /// </remarks>
    public Acks Acks { get; set; } = Acks.All;

    /// <summary>
    /// Gets or sets a value indicating whether Kafka idempotent
    /// message production is enabled.
    /// </summary>
    /// <remarks>
    /// The default value is <see langword="true"/>.
    /// Enabling idempotence helps prevent duplicate messages caused by
    /// retries performed by the Kafka producer.
    /// </remarks>
    public bool EnableIdempotence { get; set; } = true;
}
using Confluent.Kafka;

namespace pvNugsMessagingNc10Kafka;

/// <summary>
/// Defines the configuration used by the Kafka messaging consumer.
/// </summary>
/// <remarks>
/// This configuration contains settings that are specific to Kafka consumers.
///
/// Kafka infrastructure settings shared with other messaging components are
/// defined in <see cref="PvNugsMessagingKafkaConfig"/>.
/// </remarks>
public sealed class PvNugsMessagingKafkaConsumerConfig
{
    /// <summary>
    /// Gets the name of the configuration section used to bind
    /// <see cref="PvNugsMessagingKafkaConsumerConfig"/> settings.
    /// </summary>
    public const string Section =
        nameof(PvNugsMessagingKafkaConsumerConfig);

    /// <summary>
    /// Gets or sets the Kafka consumer session timeout.
    /// </summary>
    /// <remarks>
    /// This value is mapped to the Kafka <c>session.timeout.ms</c>
    /// consumer setting.
    ///
    /// The default value is one hour.
    /// </remarks>
    public TimeSpan SessionTimeout { get; set; } =
        TimeSpan.FromSeconds(10);

    /// <summary>
    /// Gets or sets a value indicating whether the consumer should operate
    /// in dummy mode instead of consuming messages from Kafka.
    /// </summary>
    public bool Dummy { get; set; }

    /// <summary>
    /// Gets or sets the behavior to use when no valid committed offset exists
    /// for a consumer group.
    /// </summary>
    /// <remarks>
    /// The default value is
    /// <see cref="Confluent.Kafka.AutoOffsetReset.Earliest"/>.
    /// </remarks>
    public AutoOffsetReset AutoOffsetReset { get; set; } =
        AutoOffsetReset.Earliest;

    /// <summary>
    /// Gets or sets the isolation level used when consuming Kafka messages.
    /// </summary>
    /// <remarks>
    /// The default value is
    /// <see cref="Confluent.Kafka.IsolationLevel.ReadCommitted"/>.
    /// </remarks>
    public IsolationLevel IsolationLevel { get; set; } =
        IsolationLevel.ReadCommitted;
}
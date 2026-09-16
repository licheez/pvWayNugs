namespace pvNugsMessagingNc10Kafka;

/// <summary>
/// Defines configuration settings shared by all Kafka messaging components.
/// </summary>
/// <remarks>
/// This configuration contains the Kafka infrastructure settings shared by
/// producers, consumers and monitoring components.
///
/// Component-specific settings are defined in their respective configuration
/// classes.
/// </remarks>
public sealed class PvNugsMessagingKafkaConfig
{
    /// <summary>
    /// Gets the name of the configuration section used to bind
    /// <see cref="PvNugsMessagingKafkaConfig"/> settings.
    /// </summary>
    public const string Section = nameof(PvNugsMessagingKafkaConfig);

    /// <summary>
    /// Gets or sets the Kafka bootstrap servers used to establish
    /// the initial connection to the Kafka cluster.
    /// </summary>
    public string BootstrapServers { get; set; } = string.Empty;
}
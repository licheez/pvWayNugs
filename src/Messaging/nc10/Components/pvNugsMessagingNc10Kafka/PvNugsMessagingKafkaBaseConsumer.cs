using System.Collections.Concurrent;
using Confluent.Kafka;
using Microsoft.Extensions.Options;
using pvNugsLoggerNc10Abstractions;

namespace pvNugsMessagingNc10Kafka;

/// <summary>
/// Provides the common infrastructure used by Kafka messaging consumers.
/// </summary>
/// <remarks>
/// This base class manages the Kafka consumer configuration and keeps track
/// of active subscriptions so that they can be stopped individually.
///
/// Concrete consumers are responsible for creating and running the actual
/// Kafka consumer instances.
/// </remarks>
internal abstract class PvNugsMessagingKafkaBaseConsumer(
    ILoggerService logger,
    IOptions<PvNugsMessagingKafkaConfig> globalOptions,
    IOptions<PvNugsMessagingKafkaConsumerConfig> consumerOptions,
    PvNugsMessagingKafkaSecurityService securityService)
{
    /// <summary>
    /// Gets the configuration shared by Kafka consumer implementations.
    /// </summary>
    protected readonly PvNugsMessagingKafkaConsumerConfig ConsumerConfig =
        consumerOptions.Value;

    /// <summary>
    /// Gets the global configuration shared by all Kafka messaging components.
    /// </summary>
    protected readonly PvNugsMessagingKafkaConfig GlobalConfig =
        globalOptions.Value;

    /// <summary>
    /// Gets the logger used by Kafka consumer implementations.
    /// </summary>
    protected readonly ILoggerService Logger = logger;

    /// <summary>
    /// Contains the actions used to stop active Kafka subscriptions.
    /// </summary>
    protected readonly ConcurrentDictionary<Guid, Action> ActiveListeners = new();

    /// <summary>
    /// Creates the Kafka consumer configuration for a subscription.
    /// </summary>
    /// <param name="consumerGroup">
    /// The Kafka consumer group associated with the subscription.
    /// </param>
    /// <param name="cancellationToken">
    /// A token that can be used to cancel the asynchronous operation.
    /// </param>
    /// <returns>
    /// A task representing the asynchronous operation. The task result contains
    /// the Kafka consumer configuration.
    /// </returns>
    /// <remarks>
    /// When dummy mode is enabled or the Kafka infrastructure is local,
    /// security credentials are not retrieved and no Kafka security settings
    /// are added to the configuration.
    ///
    /// The Kafka provider uses SASL/SSL with the PLAIN authentication mechanism
    /// when connecting to secured Kafka infrastructure.
    /// </remarks>
    protected async Task<ConsumerConfig> CreateConfigAsync(
        string consumerGroup,
        CancellationToken cancellationToken = default)
    {
        await Logger.LogAsync(
            "Creating Kafka consumer configuration",
            SeverityEnu.Trace);

        var consumerConfig = new ConsumerConfig
        {
            BootstrapServers = GlobalConfig.BootstrapServers,
            GroupId = consumerGroup,
            AutoOffsetReset = ConsumerConfig.AutoOffsetReset,
            EnableAutoCommit = false,
            SessionTimeoutMs =
                (int)ConsumerConfig.SessionTimeout.TotalMilliseconds,
            IsolationLevel = ConsumerConfig.IsolationLevel
        };

        if (ConsumerConfig.Dummy
            || securityService.Mode == PvNugsMessagingKafkaSecurityMode.None)
        {
            return consumerConfig;
        }

        var security = await securityService.GetKafkaSecurityAsync(
            cancellationToken);

        consumerConfig.SecurityProtocol =
            securityService.Mode == PvNugsMessagingKafkaSecurityMode.SaslSsl
                ? SecurityProtocol.SaslSsl
                : SecurityProtocol.SaslPlaintext;
        consumerConfig.SaslMechanism = SaslMechanism.Plain;
        consumerConfig.SaslUsername = security.SaslUsername;
        consumerConfig.SaslPassword = security.SaslPassword;
        consumerConfig.EnableSslCertificateVerification =
            security.EnableSslCertificateVerification;

        return consumerConfig;
    }

    /// <summary>
    /// Stops and removes an active Kafka subscription.
    /// </summary>
    /// <param name="subscriberId">
    /// The unique identifier of the subscription to stop.
    /// </param>
    /// <param name="cancellationToken">
    /// A token that can be used to cancel the asynchronous operation.
    /// </param>
    /// <returns>
    /// A task that completes when the subscription has been stopped.
    /// </returns>
    public async Task UnsubscribeAsync(
        Guid subscriberId,
        CancellationToken cancellationToken = default)
    {
        await Logger.LogAsync(
            $"Unsubscribing from Kafka subscription with ID: {subscriberId}",
            SeverityEnu.Trace);

        if (ActiveListeners.TryRemove(subscriberId, out var stopper))
        {
            stopper();
        }
    }
}
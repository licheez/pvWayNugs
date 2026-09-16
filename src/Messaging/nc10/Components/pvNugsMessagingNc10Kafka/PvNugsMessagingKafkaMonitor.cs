using Confluent.Kafka;
using Microsoft.Extensions.Options;
using pvNugsLoggerNc10Abstractions;
using pvNugsMessagingNc10Abstractions;

namespace pvNugsMessagingNc10Kafka;

/// <summary>
/// Provides access to the current observable state of Kafka topics
/// and consumer groups.
/// </summary>
/// <remarks>
/// The monitor retrieves information directly from the Kafka infrastructure
/// and does not maintain application-specific monitoring state.
///
/// Metrics that require a consumer group are reported as
/// <see cref="PvNugsCommunicationMetricStatusEnu.NotApplicable"/> when no
/// consumer group is specified.
///
/// Metrics that cannot be obtained reliably from Kafka are reported as
/// <see cref="PvNugsCommunicationMetricStatusEnu.NotSupported"/>.
/// </remarks>
internal sealed class PvNugsMessagingKafkaMonitor(
    ILoggerService logger,
    IOptions<PvNugsMessagingKafkaConfig> options,
    PvNugsMessagingKafkaSecurityService securityService)
    : IPvNugsMessagingMonitor
{
    private static readonly TimeSpan QueryTimeout =
        TimeSpan.FromSeconds(10);

    private readonly PvNugsMessagingKafkaConfig _config =
        options.Value;

    /// <inheritdoc />
    public async Task<PvNugsCommunicationTopicStatus> GetTopicStatusAsync(
        string topic,
        string? consumerGroup = null,
        CancellationToken cancellationToken = default)
    {
        await logger.LogAsync(
            $"Retrieving Kafka status for topic '{topic}'" +
            (consumerGroup is null
                ? "."
                : $" and consumer group '{consumerGroup}'."),
            SeverityEnu.Trace);

        if (consumerGroup is null)
        {
            return new PvNugsCommunicationTopicStatus
            {
                Topic = topic,
                ConsumerGroup = null,

                PendingMessages = NotApplicable<long>(
                    "A consumer group is required to determine " +
                    "pending messages."),

                ActiveConsumers = NotApplicable<int>(
                    "A consumer group is required to determine " +
                    "active consumers."),

                LastActivity = NotApplicable<DateTimeOffset>(
                    "A consumer group is required to determine " +
                    "consumer activity.")
            };
        }

        try
        {
            var adminClientConfig =
                await CreateAdminClientConfigAsync(
                    cancellationToken);

            var pendingMessages = await GetPendingMessagesAsync(
                adminClientConfig,
                topic,
                consumerGroup,
                cancellationToken);

            var activeConsumers = await GetActiveConsumersAsync(
                adminClientConfig,
                topic,
                consumerGroup,
                cancellationToken);

            var lastActivity = GetLastActivity();

            await logger.LogAsync(
                $"Kafka status retrieved for topic '{topic}' " +
                $"and consumer group '{consumerGroup}'.",
                SeverityEnu.Trace);

            return new PvNugsCommunicationTopicStatus
            {
                Topic = topic,
                ConsumerGroup = consumerGroup,
                PendingMessages = pendingMessages,
                ActiveConsumers = activeConsumers,
                LastActivity = lastActivity
            };
        }
        catch (OperationCanceledException)
            when (cancellationToken.IsCancellationRequested)
        {
            await logger.LogAsync(
                $"Retrieving Kafka status for topic '{topic}' " +
                $"and consumer group '{consumerGroup}' was canceled.",
                SeverityEnu.Trace);

            throw;
        }
        catch (Exception e)
        {
            await logger.LogAsync(
                $"Unable to retrieve Kafka status for topic '{topic}' " +
                $"and consumer group '{consumerGroup}': {e}",
                SeverityEnu.Error);

            return new PvNugsCommunicationTopicStatus
            {
                Topic = topic,
                ConsumerGroup = consumerGroup,

                PendingMessages = Unavailable<long>(e.Message),

                ActiveConsumers = Unavailable<int>(e.Message),

                LastActivity = NotSupported<DateTimeOffset>(
                    "Kafka does not expose a direct last-activity " +
                    "timestamp for a topic and consumer group.")
            };
        }
    }

    /// <summary>
    /// Creates the Kafka admin client configuration used by monitoring
    /// operations.
    /// </summary>
    /// <param name="cancellationToken">
    /// A token that can be used to cancel security credential retrieval.
    /// </param>
    /// <returns>
    /// The Kafka admin client configuration used by monitoring operations.
    /// </returns>
    private async Task<AdminClientConfig> CreateAdminClientConfigAsync(
        CancellationToken cancellationToken)
    {
        var config = new AdminClientConfig
        {
            BootstrapServers = _config.BootstrapServers
        };

        if (_config.IsLocal)
        {
            return config;
        }

        var security = await securityService.GetKafkaSecurityAsync(
            cancellationToken);

        config.SecurityProtocol = SecurityProtocol.SaslSsl;
        config.SaslMechanism = SaslMechanism.Plain;
        config.SaslUsername = security.SaslUsername;
        config.SaslPassword = security.SaslPassword;
        config.EnableSslCertificateVerification =
            security.EnableSslCertificateVerification;

        return config;
    }

    /// <summary>
    /// Retrieves the number of messages that are pending for the specified
    /// consumer group on the requested topic.
    /// </summary>
    /// <remarks>
    /// The pending message count is calculated from the difference between
    /// the high watermark and the committed offset for each topic partition.
    ///
    /// If no committed offset exists for any partition, the metric is
    /// reported as unavailable because the monitor cannot determine which
    /// offset reset policy a future consumer would apply.
    /// </remarks>
    /// <param name="adminClientConfig">
    /// The Kafka admin client configuration used to query the broker.
    /// </param>
    /// <param name="topic">
    /// The Kafka topic to inspect.
    /// </param>
    /// <param name="consumerGroup">
    /// The Kafka consumer group whose committed offsets are inspected.
    /// </param>
    /// <param name="cancellationToken">
    /// A token that can be used to cancel the monitoring operation.
    /// </param>
    /// <returns>
    /// A metric containing the number of pending messages when it can be
    /// determined reliably.
    /// </returns>
    private async Task<PvNugsCommunicationMetric<long>>
        GetPendingMessagesAsync(
            AdminClientConfig adminClientConfig,
            string topic,
            string consumerGroup,
            CancellationToken cancellationToken)
    {
        try
        {
            await logger.LogAsync(
                $"Retrieving pending messages for Kafka topic '{topic}' " +
                $"and consumer group '{consumerGroup}'.",
                SeverityEnu.Trace);

            cancellationToken.ThrowIfCancellationRequested();

            using var adminClient =
                new AdminClientBuilder(adminClientConfig).Build();

            var metadata = adminClient.GetMetadata(
                topic,
                QueryTimeout);

            cancellationToken.ThrowIfCancellationRequested();

            var topicMetadata = metadata.Topics
                .FirstOrDefault(t =>
                    string.Equals(
                        t.Topic,
                        topic,
                        StringComparison.Ordinal));

            if (topicMetadata is null)
            {
                return Unavailable<long>(
                    $"Metadata for Kafka topic '{topic}' " +
                    "could not be retrieved.");
            }

            if (topicMetadata.Error.IsError)
            {
                return Unavailable<long>(
                    $"Kafka returned an error while retrieving metadata " +
                    $"for topic '{topic}': " +
                    $"{topicMetadata.Error.Reason}");
            }

            // Important:
            // ConsumerConfig(AdminClientConfig) shares the underlying
            // configuration collection. Adding consumer-specific properties
            // would therefore also mutate adminClientConfig and cause
            // librdkafka CONFWARN messages when it is subsequently reused
            // by an AdminClient.
            //
            // Materializing the configuration into a new dictionary creates
            // an independent backing collection for the consumer.
            var consumerConfig = new ConsumerConfig(
                adminClientConfig.ToDictionary(
                    item => item.Key,
                    item => item.Value))
            {
                GroupId = consumerGroup,
                EnableAutoCommit = false
            };

            using var consumer =
                new ConsumerBuilder<Ignore, Ignore>(
                    consumerConfig).Build();

            long pendingMessages = 0;

            foreach (var partitionMetadata in topicMetadata.Partitions)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var topicPartition = new TopicPartition(
                    topic,
                    new Partition(partitionMetadata.PartitionId));

                var watermarkOffsets =
                    consumer.QueryWatermarkOffsets(
                        topicPartition,
                        QueryTimeout);

                cancellationToken.ThrowIfCancellationRequested();

                var committedOffsets = consumer.Committed(
                    [topicPartition],
                    QueryTimeout);

                var committedOffset =
                    committedOffsets[0].Offset;

                if (committedOffset == Offset.Unset)
                {
                    return Unavailable<long>(
                        $"No committed offset exists for Kafka consumer " +
                        $"group '{consumerGroup}' on topic '{topic}', " +
                        $"partition {partitionMetadata.PartitionId}.");
                }

                var partitionPending =
                    watermarkOffsets.High.Value -
                    committedOffset.Value;

                if (partitionPending > 0)
                {
                    pendingMessages += partitionPending;
                }
            }

            return Available(pendingMessages);
        }
        catch (OperationCanceledException)
            when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception e)
        {
            await logger.LogAsync(
                $"Unable to retrieve pending messages for Kafka topic " +
                $"'{topic}' and consumer group '{consumerGroup}': {e}",
                SeverityEnu.Error);

            return Unavailable<long>(e.Message);
        }
    }

    /// <summary>
    /// Retrieves the number of active consumers in the specified consumer
    /// group that currently own at least one partition of the requested topic.
    /// </summary>
    /// <param name="adminClientConfig">
    /// The Kafka admin client configuration used to query the broker.
    /// </param>
    /// <param name="topic">
    /// The Kafka topic to inspect.
    /// </param>
    /// <param name="consumerGroup">
    /// The Kafka consumer group to inspect.
    /// </param>
    /// <param name="cancellationToken">
    /// A token that can be used to cancel the monitoring operation.
    /// </param>
    /// <returns>
    /// A metric containing the number of active consumers when it can be
    /// determined.
    /// </returns>
    private async Task<PvNugsCommunicationMetric<int>>
        GetActiveConsumersAsync(
            AdminClientConfig adminClientConfig,
            string topic,
            string consumerGroup,
            CancellationToken cancellationToken)
    {
        try
        {
            await logger.LogAsync(
                $"Retrieving active consumers for Kafka topic '{topic}' " +
                $"and consumer group '{consumerGroup}'.",
                SeverityEnu.Trace);

            cancellationToken.ThrowIfCancellationRequested();

            using var adminClient =
                new AdminClientBuilder(adminClientConfig).Build();

            var result =
                await adminClient.DescribeConsumerGroupsAsync(
                    [consumerGroup]);

            cancellationToken.ThrowIfCancellationRequested();

            var group = result.ConsumerGroupDescriptions
                .FirstOrDefault();

            if (group is null)
            {
                return Available(0);
            }

            var activeConsumers = group.Members.Count(
                member =>
                    member.Assignment.TopicPartitions.Any(
                        topicPartition =>
                            string.Equals(
                                topicPartition.Topic,
                                topic,
                                StringComparison.Ordinal)));

            return Available(activeConsumers);
        }
        catch (OperationCanceledException)
            when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception e)
        {
            await logger.LogAsync(
                $"Unable to retrieve active consumers for Kafka topic " +
                $"'{topic}' and consumer group '{consumerGroup}': {e}",
                SeverityEnu.Error);

            return Unavailable<int>(e.Message);
        }
    }

    /// <summary>
    /// Returns the availability status of the last-activity metric.
    /// </summary>
    /// <remarks>
    /// Kafka does not expose a direct and unambiguous timestamp representing
    /// the last activity of a consumer group for a specific topic.
    /// </remarks>
    /// <returns>
    /// A metric indicating that last activity is not supported by this
    /// provider.
    /// </returns>
    private static PvNugsCommunicationMetric<DateTimeOffset>
        GetLastActivity()
    {
        return NotSupported<DateTimeOffset>(
            "Kafka does not expose a direct last-activity timestamp " +
            "for a topic and consumer group.");
    }

    /// <summary>
    /// Creates a metric indicating that the requested value does not apply
    /// in the current context.
    /// </summary>
    private static PvNugsCommunicationMetric<T> NotApplicable<T>(
        string reason)
    {
        return new PvNugsCommunicationMetric<T>
        {
            Status = PvNugsCommunicationMetricStatusEnu.NotApplicable,
            Reason = reason
        };
    }

    /// <summary>
    /// Creates a metric indicating that the requested value is not supported
    /// by the underlying messaging technology.
    /// </summary>
    private static PvNugsCommunicationMetric<T> NotSupported<T>(
        string reason)
    {
        return new PvNugsCommunicationMetric<T>
        {
            Status = PvNugsCommunicationMetricStatusEnu.NotSupported,
            Reason = reason
        };
    }

    /// <summary>
    /// Creates a metric indicating that the requested value could not
    /// currently be retrieved.
    /// </summary>
    private static PvNugsCommunicationMetric<T> Unavailable<T>(
        string reason)
    {
        return new PvNugsCommunicationMetric<T>
        {
            Status = PvNugsCommunicationMetricStatusEnu.Unavailable,
            Reason = reason
        };
    }

    /// <summary>
    /// Creates a metric containing an available value.
    /// </summary>
    private static PvNugsCommunicationMetric<T> Available<T>(
        T value)
    {
        return new PvNugsCommunicationMetric<T>
        {
            Status = PvNugsCommunicationMetricStatusEnu.Available,
            Value = value
        };
    }
}
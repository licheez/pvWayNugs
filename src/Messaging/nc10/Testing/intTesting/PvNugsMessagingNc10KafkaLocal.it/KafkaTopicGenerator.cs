using Confluent.Kafka;
using Confluent.Kafka.Admin;
using Microsoft.Extensions.Options;
using pvNugsLoggerNc10Abstractions;
using pvNugsMessagingNc10Kafka;

namespace PvNugsMessagingNc10KafkaLocal.it;

internal sealed class KafkaTopicGenerator(
    ILoggerService logger,
    IOptions<PvNugsMessagingKafkaConfig> kafkaOptions)
{
    private readonly PvNugsMessagingKafkaConfig _kafkaConfig =
        kafkaOptions.Value;

    public async Task EnsureTopicExistsAsync(
        string topic,
        CancellationToken cancellationToken = default)
    {
        await logger.LogAsync(
            $"Checking whether Kafka topic '{topic}' exists");

        using var adminClient = new AdminClientBuilder(
            new AdminClientConfig
            {
                BootstrapServers = _kafkaConfig.BootstrapServers
            })
            .Build();

        cancellationToken.ThrowIfCancellationRequested();

        var metadata = adminClient.GetMetadata(
            TimeSpan.FromSeconds(10));

        var topicExists = metadata.Topics.Any(
            t => string.Equals(
                t.Topic,
                topic,
                StringComparison.Ordinal));

        if (topicExists)
        {
            await logger.LogAsync(
                $"Kafka topic '{topic}' already exists");

            return;
        }

        await logger.LogAsync(
            $"Creating Kafka topic '{topic}'");

        try
        {
            await adminClient.CreateTopicsAsync(
            [
                new TopicSpecification
                {
                    Name = topic,
                    NumPartitions = 1,
                    ReplicationFactor = 1
                }
            ]);

            await logger.LogAsync(
                $"Kafka topic '{topic}' successfully created");
        }
        catch (CreateTopicsException ex)
            when (ex.Results.All(
                r => r.Error.Code == ErrorCode.TopicAlreadyExists))
        {
            // The topic may have been created between the metadata
            // lookup and the CreateTopics request.
            await logger.LogAsync(
                $"Kafka topic '{topic}' already exists");
        }
    }
}
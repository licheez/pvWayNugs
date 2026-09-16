using Confluent.Kafka;
using Confluent.Kafka.Admin;
using Microsoft.Extensions.Options;
using pvNugsLoggerNc10Abstractions;
using pvNugsMessagingNc10Kafka;
using pvNugsSecretManagerNc10Abstractions;

namespace PvNugsMessagingNc10KafkaLocal.it;

internal sealed class KafkaTopicGenerator(
    ILoggerService logger,
    IOptions<PvNugsMessagingKafkaConfig> kafkaOptions,
    IOptions<PvNugsMessagingKafkaSecurityConfig> securityOptions,
    IPvNugsSecretManager secretManager)
{
    private readonly PvNugsMessagingKafkaConfig _kafkaConfig =
        kafkaOptions.Value;

    private readonly PvNugsMessagingKafkaSecurityConfig _securityConfig =
        securityOptions.Value;

    public async Task EnsureTopicExistsAsync(
        string topic,
        CancellationToken cancellationToken = default)
    {
        await logger.LogAsync(
            $"Checking whether Kafka topic '{topic}' exists");

        var adminConfig = new AdminClientConfig
        {
            BootstrapServers = _kafkaConfig.BootstrapServers
        };

        if (_securityConfig.Mode != PvNugsMessagingKafkaSecurityMode.None)
        {
            var username = await secretManager.GetStaticSecretAsync(
                _securityConfig.SaslUsernameParams,
                cancellationToken);

            var password = await secretManager.GetStaticSecretAsync(
                _securityConfig.SaslPasswordParams,
                cancellationToken);

            adminConfig.SecurityProtocol =
                _securityConfig.Mode switch
                {
                    PvNugsMessagingKafkaSecurityMode.SaslPlaintext =>
                        SecurityProtocol.SaslPlaintext,

                    PvNugsMessagingKafkaSecurityMode.SaslSsl =>
                        SecurityProtocol.SaslSsl,

                    _ => throw new InvalidOperationException(
                        $"Unsupported Kafka security mode '{_securityConfig.Mode}'.")
                };

            adminConfig.SaslMechanism = SaslMechanism.Plain;
            adminConfig.SaslUsername = username;
            adminConfig.SaslPassword = password;

            if (_securityConfig.Mode ==
                PvNugsMessagingKafkaSecurityMode.SaslSsl)
            {
                adminConfig.EnableSslCertificateVerification =
                    _securityConfig.EnableSslCertificateVerification;
            }
        }

        using var adminClient = new AdminClientBuilder(adminConfig)
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
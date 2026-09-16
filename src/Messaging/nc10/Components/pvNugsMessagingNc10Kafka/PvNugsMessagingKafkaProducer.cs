using System.Text.Json;
using Confluent.Kafka;
using Microsoft.Extensions.Options;
using pvNugsLoggerNc10Abstractions;
using pvNugsMessagingNc10Abstractions;

namespace pvNugsMessagingNc10Kafka;

/// <summary>
/// Implements message publishing through Kafka.
/// </summary>
/// <remarks>
/// The producer supports both raw string messages and strongly typed messages.
/// Typed messages are serialized to JSON before being published.
///
/// A Kafka producer instance is created lazily and reused for subsequent
/// publications performed by this service instance.
/// </remarks>
internal sealed class PvNugsMessagingKafkaProducer(
    ILoggerService logger,
    IOptions<PvNugsMessagingKafkaConfig> globalOptions,
    IOptions<PvNugsMessagingKafkaProducerConfig> producerOptions,
    PvNugsMessagingKafkaSecurityService securityService)
    : IPvNugsMessagingProducer, IDisposable
{
    private readonly PvNugsMessagingKafkaConfig _globalConfig =
        globalOptions.Value;

    private readonly PvNugsMessagingKafkaProducerConfig _producerConfig =
        producerOptions.Value;

    private readonly SemaphoreSlim _producerLock = new(1, 1);

    private IProducer<Null, string>? _producer;

    private long _lastDummyOffset;

    /// <inheritdoc />
    public async Task<PvNugsPublishResult> PublishAsync(
        string topic,
        string message,
        CancellationToken cancellationToken = default)
    {
        if (_producerConfig.Dummy)
        {
            await logger.LogAsync(
                $"Publishing dummy message to topic '{topic}'.",
                SeverityEnu.Trace);

            return GenerateDummyPublishResult(topic);
        }

        await logger.LogAsync(
            $"Publishing message to Kafka topic '{topic}'.",
            SeverityEnu.Trace);

        try
        {
            var producer = await GetProducerAsync(cancellationToken);

            var deliveryResult = await producer.ProduceAsync(
                topic,
                new Message<Null, string>
                {
                    Value = message
                },
                cancellationToken);

            var result = CreatePublishResult(
                topic,
                deliveryResult);

            await logger.LogAsync(
                $"Message published to Kafka topic '{topic}', " +
                $"partition {result.Partition}, offset {result.Offset}.",
                SeverityEnu.Trace);

            return result;
        }
        catch (OperationCanceledException)
            when (cancellationToken.IsCancellationRequested)
        {
            await logger.LogAsync(
                $"Publishing to Kafka topic '{topic}' was canceled.",
                SeverityEnu.Trace);

            throw;
        }
        catch (Exception e)
        {
            await logger.LogAsync(
                $"An error occurred while publishing to Kafka topic " +
                $"'{topic}': {e}",
                SeverityEnu.Error);

            throw new PvNugsMessagingException(e);
        }
    }

    /// <inheritdoc />
    public async Task<PvNugsPublishResult> PublishAsync<T>(
        string topic,
        T message,
        CancellationToken cancellationToken = default)
    {
        await logger.LogAsync(
            $"Serializing message of type '{typeof(T).Name}' " +
            $"for topic '{topic}'.",
            SeverityEnu.Trace);

        var json = JsonSerializer.Serialize(message);

        return await PublishAsync(
            topic,
            json,
            cancellationToken);
    }

    /// <summary>
    /// Gets the Kafka producer used by this service, creating it when required.
    /// </summary>
    /// <param name="cancellationToken">
    /// A token that can be used to cancel producer initialization.
    /// </param>
    /// <returns>
    /// The initialized Kafka producer.
    /// </returns>
    private async Task<IProducer<Null, string>> GetProducerAsync(
        CancellationToken cancellationToken)
    {
        if (_producer is not null)
        {
            return _producer;
        }

        await _producerLock.WaitAsync(cancellationToken);

        try
        {
            if (_producer is not null)
            {
                return _producer;
            }

            await logger.LogAsync(
                "Creating Kafka producer.",
                SeverityEnu.Trace);

            var config = await CreateProducerConfigAsync(
                cancellationToken);

            _producer = new ProducerBuilder<Null, string>(
                config).Build();

            await logger.LogAsync(
                "Kafka producer created.",
                SeverityEnu.Trace);

            return _producer;
        }
        finally
        {
            _producerLock.Release();
        }
    }

    /// <summary>
    /// Creates the configuration used by the Kafka producer.
    /// </summary>
    /// <param name="cancellationToken">
    /// A token that can be used to cancel security credential retrieval.
    /// </param>
    /// <returns>
    /// The Kafka producer configuration.
    /// </returns>
    private async Task<ProducerConfig> CreateProducerConfigAsync(
        CancellationToken cancellationToken = default)
    {
        await logger.LogAsync(
            "Creating Kafka producer configuration.",
            SeverityEnu.Trace);

        var producerConfig = new ProducerConfig
        {
            BootstrapServers = _globalConfig.BootstrapServers,
            Acks = _producerConfig.Acks,
            EnableIdempotence = _producerConfig.EnableIdempotence,
            RetryBackoffMaxMs =
                (int)_producerConfig.RetryBackoffMax.TotalMilliseconds,
            MessageSendMaxRetries =
                _producerConfig.MessageSendMaxRetries
        };

        if (securityService.Mode == PvNugsMessagingKafkaSecurityMode.None)
        {
            await logger.LogAsync(
                "Kafka producer security mode is None: " +
                "SASL credentials will not be applied.",
                SeverityEnu.Trace);
            return producerConfig;
        }

        var security = await securityService.GetKafkaSecurityAsync(
            cancellationToken);

        producerConfig.SecurityProtocol =
            securityService.Mode == PvNugsMessagingKafkaSecurityMode.SaslSsl
                ? SecurityProtocol.SaslSsl
                : SecurityProtocol.SaslPlaintext;
        producerConfig.SaslMechanism = SaslMechanism.Plain;
        producerConfig.SaslUsername = security.SaslUsername;
        producerConfig.SaslPassword = security.SaslPassword;
        producerConfig.EnableSslCertificateVerification =
            security.EnableSslCertificateVerification;

        return producerConfig;
    }

    /// <summary>
    /// Creates broker-neutral publication metadata from a Kafka
    /// delivery result.
    /// </summary>
    /// <param name="topic">
    /// The Kafka topic to which the message was published.
    /// </param>
    /// <param name="deliveryResult">
    /// The Kafka delivery result returned by the producer.
    /// </param>
    /// <returns>
    /// Broker-neutral metadata describing the published message.
    /// </returns>
    private static PvNugsPublishResult CreatePublishResult(
        string topic,
        DeliveryResult<Null, string> deliveryResult)
    {
        return new PvNugsPublishResult
        {
            MessageId =
                $"{topic}:" +
                $"{deliveryResult.Partition.Value}:" +
                $"{deliveryResult.Offset.Value}",
            Partition = deliveryResult.Partition.Value,
            Offset = deliveryResult.Offset.Value,
            PublishedAt =
                deliveryResult.Message.Timestamp.Type !=
                TimestampType.NotAvailable
                    ? new DateTimeOffset(
                        deliveryResult.Message.Timestamp.UtcDateTime,
                        TimeSpan.Zero)
                    : DateTimeOffset.UtcNow
        };
    }

    /// <summary>
    /// Generates publication metadata when the provider operates
    /// in dummy mode.
    /// </summary>
    /// <param name="topic">
    /// The topic associated with the dummy publication.
    /// </param>
    /// <returns>
    /// Broker-neutral metadata representing the dummy publication.
    /// </returns>
    private PvNugsPublishResult GenerateDummyPublishResult(
        string topic)
    {
        var offset = Interlocked.Increment(
            ref _lastDummyOffset);

        return new PvNugsPublishResult
        {
            MessageId = $"{topic}:0:{offset}",
            Partition = 0,
            Offset = offset,
            PublishedAt = DateTimeOffset.UtcNow
        };
    }

    /// <summary>
    /// Releases resources held by the Kafka producer.
    /// </summary>
    public void Dispose()
    {
        _producer?.Flush(TimeSpan.FromSeconds(10));
        _producer?.Dispose();
        _producerLock.Dispose();
    }
}
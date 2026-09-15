using System.Text.Json;
using Confluent.Kafka;
using Microsoft.Extensions.Options;
using pvNugsLoggerNc10Abstractions;
using pvNugsMessagingNc10Abstractions;

namespace pvNugsMessagingNc10Kafka;

internal class PvNugsMessagingKafkaProducer(
    ILoggerService logger,
    IOptions<PvNugsMessagingKafkaProducerConfig> options,
    PvNugsMessagingKafkaCredentialService kcs): IPvNugsMessagingProducer
{
    private readonly PvNugsMessagingKafkaProducerConfig _config = options.Value;
    private ProducerConfig? _producerConfig;
    private long _lastOffset = 1;
    
    public async Task<PvNugsPublishResult> PublishAsync(
        string topic, string message, 
        CancellationToken cancellationToken = default)
    {
        if (_config.Dummy) return GenerateDummyPublishResult();
        if (_producerConfig is null)
        {
            await CreateProducerConfigAsync(cancellationToken);
        }

        using var producer = new ProducerBuilder<Null, string>(_producerConfig).Build(); 
        try
        {
            var dr = await producer.ProduceAsync(topic, new Message<Null, string> {
                Value = message
            }, cancellationToken);
            var result = new PvNugsPublishResult
            {
                MessageId = dr.Message.Value ?? Guid.NewGuid().ToString(),
                Partition = dr.Partition.Value,
                Offset = dr.Offset.Value,
                PublishedAt = DateTimeOffset.UtcNow
            };
            return result;
        }
        catch (Exception e)
        {
            await logger.LogAsync(e);
            throw new PvNugsMessagingException(e);
        }
    }
    
    public async Task<PvNugsPublishResult> PublishAsync<T>(
        string topic, T message, 
        CancellationToken cancellationToken = default)
    {
        if (_config.Dummy) return GenerateDummyPublishResult();
        if (_producerConfig is null)
        {
            await CreateProducerConfigAsync(cancellationToken);
        }

        using var producer = new ProducerBuilder<Null, string>(_producerConfig).Build(); 
        try
        {
            var json = JsonSerializer.Serialize(message);
            var dr = await producer.ProduceAsync(topic, new Message<Null, string> {
                Value = json
            }, cancellationToken);
            var result = new PvNugsPublishResult
            {
                MessageId = dr.Message.Value ?? Guid.NewGuid().ToString(),
                Partition = dr.Partition.Value,
                Offset = dr.Offset.Value,
                PublishedAt = DateTimeOffset.UtcNow
            };
            return result;
        }
        catch (Exception e)
        {
            await logger.LogAsync(e);
            throw new PvNugsMessagingException(e);
        }
    }

    private PvNugsPublishResult GenerateDummyPublishResult()
    {
        return new PvNugsPublishResult
        {
            MessageId = Guid.NewGuid().ToString(),
            Partition = 0,
            Offset = _lastOffset++,
            PublishedAt = DateTimeOffset.UtcNow
        };
    }

    private async Task CreateProducerConfigAsync(
        CancellationToken cancellationToken = default)
    {
        if (_producerConfig is not null)
        {
            return;
        }

        _producerConfig = new ProducerConfig
        {
            BootstrapServers = _config.BootstrapServers,
            Acks = _config.Acks,
            EnableIdempotence = _config.EnableIdempotence,
            RetryBackoffMaxMs = (int)_config.RetryBackoff.TotalMilliseconds,
            MessageSendMaxRetries = _config.MessageSendMaxRetries,
        };

        if (_config.IsLocal) return;
        
        var kc = await kcs.GetKafkaCredentialAsync(
            cancellationToken);

        _producerConfig.SaslUsername = kc.SaslUsername;
        _producerConfig.SaslPassword = kc.SaslPassword;
        _producerConfig.SecurityProtocol = kc.SecurityProtocol;
        _producerConfig.SaslMechanism = kc.SaslMechanism;
        _producerConfig.EnableSslCertificateVerification = kc.EnableSslCertificateVerification;
    }
}
using System.Collections.Concurrent;
using Confluent.Kafka;
using Microsoft.Extensions.Options;
using pvNugsLoggerNc10Abstractions;

namespace pvNugsMessagingNc10Kafka;

internal abstract class PvNugsMessageKafkaBaseConsumer(
    ILoggerService logger,
    IOptions<PvNugsMessagingKafkaConsumerConfig> options,
    PvNugsMessagingKafkaCredentialService kcs)
{
    protected readonly PvNugsMessagingKafkaConsumerConfig Config = options.Value;
    protected readonly ILoggerService Logger = logger;
    
    protected readonly IDictionary<Guid, Action> ActiveListeners = new ConcurrentDictionary<Guid, Action>();
    protected ConsumerConfig? ConsumerConfig;

    protected async Task CreateConfigAsync(CancellationToken cancellationToken = default)
    {
        ConsumerConfig = new ConsumerConfig
        {
            BootstrapServers = Config.BootstrapServers,
            AutoOffsetReset = Config.AutoOffsetReset,
            EnableAutoCommit = Config.EnableAutoCommit,
            SessionTimeoutMs = (int)Config.Timeout.TotalMilliseconds,
            IsolationLevel = Config.IsolationLevel
        };
        if (Config.Dummy) return;

        var kc = await kcs.GetKafkaCredentialAsync(cancellationToken);
        
        ConsumerConfig.SecurityProtocol = kc.SecurityProtocol;
        ConsumerConfig.SaslMechanism = kc.SaslMechanism;
        ConsumerConfig.SaslUsername = kc.SaslUsername;
        ConsumerConfig.SaslPassword = kc.SaslPassword;
        ConsumerConfig.EnableSslCertificateVerification = kc.EnableSslCertificateVerification;
    }

    public async Task UnsubscribeAsync(Guid subscriberId, 
        CancellationToken cancellationToken = default)
    {
        ActiveListeners.TryGetValue(subscriberId, out var stopper);
        stopper?.Invoke();
        ActiveListeners.Remove(subscriberId);
        await Task.CompletedTask;
    }
    
}
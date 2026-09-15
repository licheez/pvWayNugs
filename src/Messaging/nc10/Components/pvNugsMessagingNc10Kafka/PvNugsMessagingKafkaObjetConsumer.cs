using Confluent.Kafka;
using Microsoft.Extensions.Options;
using pvNugsLoggerNc10Abstractions;
using pvNugsMessagingNc10Abstractions;

namespace pvNugsMessagingNc10Kafka;

internal class PvNugsMessagingKafkaObjetConsumer<T>(
    ILoggerService logger,
    IOptions<PvNugsMessagingKafkaConsumerConfig> options,
    PvNugsMessagingKafkaCredentialService kcs) :
    PvNugsMessageKafkaBaseConsumer(logger, options, kcs),
    IPvNugsMessagingConsumer<T> where T : class
{
    public async Task<Guid> SubscribeAsync(
        string topic,
        Func<string, Task<T>> factorAsync,
        Func<string, PvNugsPublishResult, T, Task<bool>> handleIncomingMessageAsync,
        CancellationToken cancellationToken = default)
    {
        var subscriberId = Guid.NewGuid();
        if (Config.Dummy) return subscriberId;
        if (ConsumerConfig is null) await CreateConfigAsync(cancellationToken);
        ConsumerConfig!.GroupId = topic;
        var worker = new Thread(Listener);
        var wb = new WorkerBag(topic, ConsumerConfig, subscriberId, 
            factorAsync, handleIncomingMessageAsync);
        worker.Start(wb);
        return subscriberId;
    }

    private void Listener(object? obj)
    {
        if (obj is not WorkerBag bag) return;
        var consumerConfig = bag.ConsumerConfig;
        var subscriberId = bag.SubscriberId;
        var topic = bag.Topic;
        var handleIncomingMessageAsync =
            bag.HandleIncomingMessageAsync;

        using var cts = new CancellationTokenSource();
        using var consumer = new ConsumerBuilder<Ignore, string>(consumerConfig).Build();

        var listen = true;
        ActiveListeners.Add(subscriberId, () =>
        {
            listen = false;
            // ReSharper disable once AccessToDisposedClosure
            cts.Cancel();
        });
        consumer.Subscribe(topic);

        try
        {
            consumer.Subscribe(topic);

            while (listen)
            {
                try
                {
                    var cr = consumer.Consume(cts.Token)!;
                    var json = cr.Message.Value!;
                    
                    var message = bag.FactorAsync(json).Result;

                    var publishResult = new PvNugsPublishResult
                    {
                        MessageId = cr.Offset.ToString(),
                        Partition = cr.Partition.Value,
                        Offset = cr.Offset.Value,
                        PublishedAt = DateTimeOffset.UtcNow
                    };

                    var commit = handleIncomingMessageAsync(topic, publishResult, message).Result;
                    if (commit) consumer.Commit(cr);
                }
                catch (Exception e)
                {
                    Logger.Log(e);
                    throw new PvNugsMessagingException(e);
                }
            }
        }
        catch (OperationCanceledException)
        {
            Logger.Log($"Subscription for topic '{topic}' has been canceled.");
        }
        catch (Exception e)
        {
            if (listen)
            {
                Logger.Log(e);
                throw new PvNugsMessagingException(e);
            }
        }
        finally
        {
            consumer.Close();
        }
    }

    private sealed class WorkerBag(
        string topic,
        ConsumerConfig consumerConfig,
        Guid subscriberId,
        Func<string, Task<T>> factorAsync,
        Func<string, PvNugsPublishResult, T, Task<bool>> handleIncomingMessageAsync)
    {
        public string Topic { get; } = topic;
        public ConsumerConfig ConsumerConfig { get; } = consumerConfig;
        public Guid SubscriberId { get; } = subscriberId;
        public Func<string, Task<T>> FactorAsync { get; } = factorAsync;

        public Func<string, PvNugsPublishResult, T, Task<bool>>
            HandleIncomingMessageAsync { get; } = handleIncomingMessageAsync;
    }
}
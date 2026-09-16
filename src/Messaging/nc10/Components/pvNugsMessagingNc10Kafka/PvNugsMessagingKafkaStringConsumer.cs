using Confluent.Kafka;
using Microsoft.Extensions.Options;
using pvNugsLoggerNc10Abstractions;
using pvNugsMessagingNc10Abstractions;

namespace pvNugsMessagingNc10Kafka;

/// <summary>
/// Implements a Kafka consumer that receives message payloads as raw strings.
/// </summary>
/// <remarks>
/// Each subscription creates an independent Kafka consumer instance.
///
/// When no consumer group is explicitly specified, the topic name is used
/// as the consumer group to preserve the historical behavior of the provider.
///
/// Message offsets are committed explicitly only when the message handler
/// confirms successful processing.
/// </remarks>
internal sealed class PvNugsMessagingKafkaStringConsumer(
    ILoggerService logger,
    IOptions<PvNugsMessagingKafkaConfig> globalOptions,
    IOptions<PvNugsMessagingKafkaConsumerConfig> consumerOptions,
    PvNugsMessagingKafkaSecurityService securityService)
    : PvNugsMessagingKafkaBaseConsumer(
        logger,
        globalOptions,
        consumerOptions,
        securityService),
      IPvNugsMessagingConsumer
{
    /// <inheritdoc />
    public async Task<Guid> SubscribeAsync(
        string topic,
        Func<string, PvNugsPublishResult, string, Task<bool>>
            handleIncomingMessageAsync,
        string? consumerGroup = null,
        CancellationToken cancellationToken = default)
    {
        var subscriberId = Guid.NewGuid();

        if (ConsumerConfig.Dummy)
        {
            return subscriberId;
        }

        var effectiveConsumerGroup = consumerGroup ?? topic;

        var config = await CreateConfigAsync(
            effectiveConsumerGroup,
            cancellationToken);

        var listenerCts = new CancellationTokenSource();

        if (!ActiveListeners.TryAdd(
                subscriberId,
                listenerCts.Cancel))
        {
            listenerCts.Dispose();

            throw new InvalidOperationException(
                $"Unable to register Kafka subscriber '{subscriberId}'.");
        }

        _ = Task.Run(
            () => ListenerAsync(
                topic,
                config,
                subscriberId,
                handleIncomingMessageAsync,
                listenerCts),
            CancellationToken.None);

        return subscriberId;
    }

    /// <summary>
    /// Runs the Kafka consume loop for a subscription.
    /// </summary>
    /// <param name="topic">
    /// The Kafka topic to consume.
    /// </param>
    /// <param name="consumerConfig">
    /// The Kafka configuration associated with the subscription.
    /// </param>
    /// <param name="subscriberId">
    /// The unique identifier of the subscription.
    /// </param>
    /// <param name="handleIncomingMessageAsync">
    /// The callback invoked for each consumed message. The callback receives,
    /// in order, the source topic, the publication metadata, and the raw
    /// message payload.
    /// </param>    /// <param name="cts">
    /// The cancellation source controlling the lifetime of the subscription.
    /// </param>
    private async Task ListenerAsync(
        string topic,
        ConsumerConfig consumerConfig,
        Guid subscriberId,
        Func<string, PvNugsPublishResult, string, Task<bool>>
            handleIncomingMessageAsync,
        CancellationTokenSource cts)
    {
        try
        {
            using var consumer =
                new ConsumerBuilder<Ignore, string>(consumerConfig).Build();

            consumer.Subscribe(topic);

            try
            {
                while (!cts.IsCancellationRequested)
                {
                    var consumeResult = consumer.Consume(cts.Token);

                    var publishedAt =
                        consumeResult.Message.Timestamp.Type !=
                        TimestampType.NotAvailable
                            ? new DateTimeOffset(
                                consumeResult.Message.Timestamp.UtcDateTime,
                                TimeSpan.Zero)
                            : DateTimeOffset.UtcNow;

                    var publishResult = new PvNugsPublishResult
                    {
                        MessageId =
                            $"{topic}:" +
                            $"{consumeResult.Partition.Value}:" +
                            $"{consumeResult.Offset.Value}",
                        Partition = consumeResult.Partition.Value,
                        Offset = consumeResult.Offset.Value,
                        PublishedAt = publishedAt
                    };
                    
                    var payload = consumeResult.Message.Value;

                    var commit = await handleIncomingMessageAsync(
                        topic,
                        publishResult,
                        payload);

                    if (commit)
                    {
                        consumer.Commit(consumeResult);
                    }
                }
            }
            catch (OperationCanceledException)
                when (cts.IsCancellationRequested)
            {
                await Logger.LogAsync(
                    $"Subscription for topic '{topic}' has been canceled.",
                    SeverityEnu.Trace);
            }
            finally
            {
                consumer.Close();
            }
        }
        catch (Exception e)
        {
            await Logger.LogAsync(
                $"An error occurred in the subscription for topic " +
                $"'{topic}': {e}",
                SeverityEnu.Error);
        }
        finally
        {
            ActiveListeners.TryRemove(subscriberId, out _);
            cts.Dispose();
        }
    }
}
using Confluent.Kafka;
using Microsoft.Extensions.Options;
using pvNugsLoggerNc10Abstractions;
using pvNugsMessagingNc10Abstractions;

namespace pvNugsMessagingNc10Kafka;

/// <summary>
/// Implements a Kafka consumer that converts raw string payloads into
/// strongly typed message instances before processing them.
/// </summary>
/// <typeparam name="T">
/// The type into which incoming message payloads are converted.
/// </typeparam>
/// <remarks>
/// Each subscription creates an independent Kafka consumer instance.
///
/// When no consumer group is explicitly specified, the topic name is used
/// as the consumer group to preserve the historical behavior of the provider.
///
/// Message offsets are committed explicitly only when the message handler
/// confirms successful processing.
/// </remarks>
internal sealed class PvNugsMessagingKafkaTypedConsumer<T>(
    ILoggerService logger,
    IOptions<PvNugsMessagingKafkaConfig> globalOptions,
    IOptions<PvNugsMessagingKafkaConsumerConfig> consumerOptions,
    PvNugsMessagingKafkaSecurityService securityService)
    : PvNugsMessagingKafkaBaseConsumer(
        logger,
        globalOptions,
        consumerOptions,
        securityService),
      IPvNugsMessagingConsumer<T>
    where T : class
{
    /// <inheritdoc />
    public async Task<Guid> SubscribeAsync(
        string topic,
        Func<string, Task<T>> factorAsync,
        Func<string, PvNugsPublishResult, T, Task<bool>>
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
                factorAsync,
                handleIncomingMessageAsync,
                listenerCts),
            CancellationToken.None);

        return subscriberId;
    }

    /// <summary>
    /// Runs the Kafka consume loop for a typed subscription.
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
    /// <param name="factorAsync">
    /// The factory used to convert the raw message payload into an instance
    /// of <typeparamref name="T"/>.
    /// </param>
    /// <param name="handleIncomingMessageAsync">
    /// The callback invoked for each converted message.
    /// </param>
    /// <param name="cts">
    /// The cancellation source controlling the lifetime of the subscription.
    /// </param>
    private async Task ListenerAsync(
        string topic,
        ConsumerConfig consumerConfig,
        Guid subscriberId,
        Func<string, Task<T>> factorAsync,
        Func<string, PvNugsPublishResult, T, Task<bool>>
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

                    var rawMessage = consumeResult.Message.Value;

                    var message = await factorAsync(rawMessage);

                    var publishResult = new PvNugsPublishResult
                    {
                        MessageId =
                            $"{topic}:" +
                            $"{consumeResult.Partition.Value}:" +
                            $"{consumeResult.Offset.Value}",
                        Partition = consumeResult.Partition.Value,
                        Offset = consumeResult.Offset.Value,
                        PublishedAt =
                            consumeResult.Message.Timestamp.Type !=
                            TimestampType.NotAvailable
                                ? new DateTimeOffset(
                                    consumeResult.Message.Timestamp.UtcDateTime,
                                    TimeSpan.Zero)
                                : DateTimeOffset.UtcNow
                    };

                    var commit = await handleIncomingMessageAsync(
                        topic,
                        publishResult,
                        message);

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
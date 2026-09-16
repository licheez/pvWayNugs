using pvNugsLoggerNc10Abstractions;
using pvNugsMessagingNc10Abstractions;

namespace PvNugsMessagingNc10KafkaLocal.it;

internal sealed class ConsumerRunner(
    ILoggerService logger,
    IPvNugsMessagingConsumer consumer)
{
    private Guid? _subscriptionId;

    public async Task LaunchConsumerAsync(
        string topic,
        CancellationToken cancellationToken = default)
    {
        await logger.LogAsync(
            $"Launching consumer for topic '{topic}'",
            SeverityEnu.Trace);

        _subscriptionId = await consumer.SubscribeAsync(
            topic,
            async (receivedTopic, publishResult, payload) =>
            {
                await logger.LogAsync(
                    $"Consumer received message for topic '{receivedTopic}'",
                    $"Message received - " +
                    $"Message: '{payload}', " +
                    $"MessageId: {publishResult.MessageId}, " +
                    $"Partition: {publishResult.Partition}, " +
                    $"Offset: {publishResult.Offset}, " +
                    $"PublishedAt: {publishResult.PublishedAt:O}");

                return true;
            },
            cancellationToken: cancellationToken);

        await logger.LogAsync(
            $"Consumer launched - SubscriptionId: {_subscriptionId}",
            SeverityEnu.Trace);
    }

    public async Task StopConsumerAsync(
        CancellationToken cancellationToken = default)
    {
        if (_subscriptionId is null)
            return;

        await logger.LogAsync(
            $"Stopping consumer - SubscriptionId: {_subscriptionId}",
            SeverityEnu.Trace);

        await consumer.UnsubscribeAsync(
            _subscriptionId.Value,
            cancellationToken);

        _subscriptionId = null;

        await logger.LogAsync(
            "Consumer stopped",
            SeverityEnu.Trace);
    }
}
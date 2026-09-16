using pvNugsLoggerNc10Abstractions;
using pvNugsMessagingNc10Abstractions;

namespace PvNugsMessagingNc10KafkaLocal.it;

internal sealed class ProducerRunner(
    ILoggerService logger,
    IPvNugsMessagingProducer producer)
{
    public async Task PublishAsync(
        string topic,
        CancellationToken cancellationToken = default)
    {
        await logger.LogAsync(
            $"Publishing test message to topic '{topic}'",
            SeverityEnu.Trace);

        var message = $"Hello Kafka! {DateTimeOffset.Now:O}";

        var result = await producer.PublishAsync(
            topic,
            message,
            cancellationToken);

        await logger.LogAsync(
            $"Message published successfully - " +
            $"MessageId: {result.MessageId}, " +
            $"Partition: {result.Partition}, " +
            $"Offset: {result.Offset}, " +
            $"PublishedAt: {result.PublishedAt:O}");
    }
}
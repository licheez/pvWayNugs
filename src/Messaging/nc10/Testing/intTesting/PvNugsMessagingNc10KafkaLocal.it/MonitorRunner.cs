using pvNugsLoggerNc10Abstractions;
using pvNugsMessagingNc10Abstractions;

namespace PvNugsMessagingNc10KafkaLocal.it;

internal sealed class MonitorRunner(
    ILoggerService logger,
    IPvNugsMessagingMonitor monitor)
{
    public async Task GetStatusAsync(
        string topic,
        string? consumerGroup = null,
        CancellationToken cancellationToken = default)
    {
        var effectiveConsumerGroup = consumerGroup ?? topic;
        await logger.LogAsync(
            $"Monitoring topic '{topic}'" +
            (consumerGroup is null
                ? string.Empty
                : $" for consumer group '{effectiveConsumerGroup}.'"),
            SeverityEnu.Trace);

        var status = await monitor.GetTopicStatusAsync(
            topic,
            effectiveConsumerGroup,
            cancellationToken);

        await logger.LogAsync(
            $"Topic status - " +
            $"Topic: '{status.Topic}', " +
            $"ConsumerGroup: '{status.ConsumerGroup ?? "<none>"}', " +
            $"PendingMessages: {FormatMetric(status.PendingMessages)}, " +
            $"ActiveConsumers: {FormatMetric(status.ActiveConsumers)}, " +
            $"LastActivity: {FormatMetric(status.LastActivity)}");
    }

    private static string FormatMetric<T>(
        PvNugsCommunicationMetric<T> metric)
    {
        var value = metric.Value is null
            ? "<none>"
            : metric.Value.ToString();

        var reason = string.IsNullOrWhiteSpace(metric.Reason)
            ? string.Empty
            : $", Reason: '{metric.Reason}'";

        return $"[{metric.Status}] {value}{reason}";
    }
}
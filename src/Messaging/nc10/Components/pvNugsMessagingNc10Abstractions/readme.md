# 📡 pvNugsMessagingNc10Abstractions

A broker-neutral messaging abstraction library for .NET 10, designed to keep application code independent from specific messaging technologies such as Kafka, IBM MQ, RabbitMQ, Azure Service Bus, or Redis Pub/Sub.

[![NuGet](https://img.shields.io/badge/NuGet-pvNugsMessagingNc10Abstractions-0078D4?logo=nuget)](https://www.nuget.org/)
[![.NET 10](https://img.shields.io/badge/.NET-10.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![License](https://img.shields.io/badge/License-Project%20License-lightgrey)](https://github.com/)

## ✨ What this package provides

`pvNugsMessagingNc10Abstractions` defines a small set of broker-neutral contracts for publishing, consuming, and observing messages.

Application code depends on these abstractions, while concrete provider packages implement the behavior required by a specific messaging technology.

The abstraction deliberately exposes messaging concepts such as topics and consumer groups without exposing provider SDK types or configuration objects.

### Included contracts

- `IPvNugsMessagingProducer` — publishes messages to a topic
- `IPvNugsMessagingConsumer` — subscribes to a topic and processes raw messages
- `IPvNugsMessagingConsumer<T>` — subscribes to a topic and materializes messages into a domain type
- `IPvNugsMessagingMonitor` — observes the current state of a topic and its consumers
- `PvNugsPublishResult` — broker-neutral publication metadata
- `PvNugsCommunicationTopicStatus` — current observable state of a topic
- `PvNugsCommunicationMetric<T>` — an observable value together with its availability status
- `PvNugsCommunicationMetricStatusEnu` — describes whether a metric is available, applicable, supported, or temporarily unavailable

## 🧩 Design goals

- ✅ Broker-neutral public API
- ✅ No dependency on broker-specific SDK types in application code
- ✅ Support for both raw string and strongly typed messages
- ✅ Support for logical consumer groups
- ✅ Real-time messaging observability
- ✅ Graceful handling of monitoring capabilities that differ between providers
- ✅ Stable contracts that can be implemented by different messaging technologies
- ✅ Clear separation between application behavior and messaging infrastructure

## 🚀 Installation

### .NET CLI

```bash
dotnet add package pvNugsMessagingNc10Abstractions
```

### Package Manager Console

```powershell
Install-Package pvNugsMessagingNc10Abstractions
```

## 📦 Core contracts

The abstraction separates messaging into three responsibilities:

```text
Producer   → publish messages
Consumer   → receive and process messages
Monitor    → observe the current messaging state
```

### Producer

`IPvNugsMessagingProducer` publishes raw or strongly typed messages to a topic.

```csharp
public interface IPvNugsMessagingProducer
{
    Task<PvNugsPublishResult> PublishAsync(
        string topic,
        string message,
        CancellationToken cancellationToken = default);

    Task<PvNugsPublishResult> PublishAsync<T>(
        string topic,
        T message,
        CancellationToken cancellationToken = default);
}
```

The concrete provider is responsible for converting strongly typed messages into the representation required by the underlying messaging technology.

### Publication result

Publishing returns a broker-neutral `PvNugsPublishResult`.

```csharp
public sealed class PvNugsPublishResult
{
    public string MessageId { get; init; } = string.Empty;

    public long? Offset { get; init; }

    public int? Partition { get; init; }

    public DateTimeOffset PublishedAt { get; init; } = DateTimeOffset.UtcNow;
}
```

Not every messaging technology exposes the same publication metadata.

`MessageId` and `PublishedAt` provide general publication information, while `Offset` and `Partition` are optional capabilities that providers can populate when the underlying technology exposes equivalent concepts.

For example, a Kafka provider can populate both `Offset` and `Partition`, while another provider may leave them unset.

This allows applications to access useful publication metadata without introducing provider-specific types into the abstraction.

## 📥 Consumer

`IPvNugsMessagingConsumer` subscribes to a topic and invokes an asynchronous callback for incoming messages.

```csharp
public interface IPvNugsMessagingConsumer
{
    Task<Guid> SubscribeAsync(
        string topic,
        Func<string, PvNugsPublishResult, Task<bool>> handleIncomingMessageAsync,
        string? consumerGroup = null,
        CancellationToken cancellationToken = default);

    Task UnsubscribeAsync(
        Guid subscriberId,
        CancellationToken cancellationToken = default);
}
```

The returned `Guid` identifies the subscription and can subsequently be passed to `UnsubscribeAsync`.

### Consumer groups

A subscription can optionally specify a logical consumer group:

```csharp
await consumer.SubscribeAsync(
    "orders.created",
    HandleOrderAsync,
    consumerGroup: "billing");
```

Consumer groups allow several subscriptions to identify themselves as instances of the same logical consumer.

When supported by the underlying messaging technology:

- subscriptions in the **same consumer group** can share the processing workload
- subscriptions in **different consumer groups** can process the same published messages independently

For example:

```text
                         ┌── billing instance 1
orders.created ── billing group
                         └── billing instance 2

               ── audit group ─── audit instance
```

The exact distribution semantics depend on the underlying provider.

When `consumerGroup` is `null`, the provider applies its default behavior.

This keeps the abstraction independent from provider-specific terminology such as Kafka's `GroupId`.

### Typed consumer

`IPvNugsMessagingConsumer<T>` adds a materialization step before invoking the message handler.

```csharp
public interface IPvNugsMessagingConsumer<T> where T : class
{
    Task<Guid> SubscribeAsync(
        string topic,
        Func<string, Task<T>> factorAsync,
        Func<string, PvNugsPublishResult, T, Task<bool>> handleIncomingMessageAsync,
        string? consumerGroup = null,
        CancellationToken cancellationToken = default);

    Task UnsubscribeAsync(
        Guid subscriberId,
        CancellationToken cancellationToken = default);
}
```

The `factorAsync` delegate converts the raw payload into the required domain type.

This allows serialization concerns to remain outside the business callback while keeping the abstraction independent from a particular serialization format.

## 👁️ Monitor

`IPvNugsMessagingMonitor` provides a broker-neutral view of the current observable state of the messaging infrastructure.

```csharp
public interface IPvNugsMessagingMonitor
{
    Task<PvNugsCommunicationTopicStatus> GetTopicStatusAsync(
        string topic,
        string? consumerGroup = null,
        CancellationToken cancellationToken = default);
}
```

The monitor is intended for **current-state observability and operational diagnostics**, rather than historical statistics or broker administration.

Implementations should retrieve information directly from the underlying messaging infrastructure whenever possible, without requiring application-maintained monitoring state.

A status can be requested for a topic alone:

```csharp
var status = await monitor.GetTopicStatusAsync(
    "orders.created");
```

or in the context of a particular consumer group:

```csharp
var status = await monitor.GetTopicStatusAsync(
    "orders.created",
    consumerGroup: "billing");
```

## 📊 Topic status

`PvNugsCommunicationTopicStatus` represents the current observable state of the requested topic.

```csharp
public sealed class PvNugsCommunicationTopicStatus
{
    public required string Topic { get; init; }

    public string? ConsumerGroup { get; init; }

    public required PvNugsCommunicationMetric<long> PendingMessages { get; init; }

    public required PvNugsCommunicationMetric<int> ActiveConsumers { get; init; }

    public required PvNugsCommunicationMetric<DateTimeOffset> LastActivity { get; init; }
}
```

Some observations are consumer-group specific.

For example, the number of pending messages can differ between two consumer groups consuming the same topic:

```text
orders.created
    │
    ├── billing   → 12 pending messages
    │
    └── audit     →  0 pending messages
```

The exact meaning and availability of each observation depend on the capabilities of the underlying messaging technology.

## 📏 Communication metrics

Different messaging technologies expose different monitoring capabilities.

Returning a nullable value alone would not distinguish between:

- a metric that is not relevant to the current request
- a metric that the provider cannot support
- a metric that should be available but could not currently be retrieved

For this reason, observable values are represented by `PvNugsCommunicationMetric<T>`:

```csharp
public class PvNugsCommunicationMetric<T>
{
    public PvNugsCommunicationMetricStatusEnu Status { get; init; }

    public T? Value { get; init; }

    public string? Reason { get; init; }
}
```

with the following status:

```csharp
public enum PvNugsCommunicationMetricStatusEnu
{
    Available,
    NotApplicable,
    NotSupported,
    Unavailable
}
```

The four states have deliberately different semantics:

| Status | Meaning |
|---|---|
| `Available` | The metric is supported and its current value was successfully retrieved. |
| `NotApplicable` | The metric is supported conceptually, but does not apply in the current context. |
| `NotSupported` | The underlying messaging technology cannot provide the metric. |
| `Unavailable` | The metric is normally supported and applicable, but its current value could not be retrieved. |

For example, a provider could return:

```csharp
new PvNugsCommunicationMetric<long>
{
    Status = PvNugsCommunicationMetricStatusEnu.Available,
    Value = 42
};
```

while a technology without persistent message backlog could return:

```csharp
new PvNugsCommunicationMetric<long>
{
    Status = PvNugsCommunicationMetricStatusEnu.NotSupported,
    Reason = "The provider does not maintain a persistent consumer backlog."
};
```

Applications should therefore inspect `Status` before using `Value`.

## 🏗️ Example usage

### Publishing a message

```csharp
public sealed class OrderPublisher
{
    private readonly IPvNugsMessagingProducer _producer;

    public OrderPublisher(IPvNugsMessagingProducer producer)
    {
        _producer = producer;
    }

    public async Task PublishOrderAsync(string orderId)
    {
        var result = await _producer.PublishAsync(
            "orders.created",
            $"{{\"orderId\":\"{orderId}\"}}");

        Console.WriteLine(
            $"Published message {result.MessageId} at {result.PublishedAt:O}");
    }
}
```

### Consuming messages

```csharp
var subscriptionId = await consumer.SubscribeAsync(
    "orders.created",
    async (message, metadata) =>
    {
        await ProcessOrderAsync(message);
        return true;
    },
    consumerGroup: "billing");
```

### Observing the consumer

```csharp
var status = await monitor.GetTopicStatusAsync(
    "orders.created",
    consumerGroup: "billing");

if (status.PendingMessages.Status ==
    PvNugsCommunicationMetricStatusEnu.Available)
{
    Console.WriteLine(
        $"Pending messages: {status.PendingMessages.Value}");
}

if (status.ActiveConsumers.Status ==
    PvNugsCommunicationMetricStatusEnu.Available)
{
    Console.WriteLine(
        $"Active consumers: {status.ActiveConsumers.Value}");
}
```

The same `topic` and `consumerGroup` concepts are therefore used consistently when subscribing and when observing a subscription.

## 🔌 Provider implementations

Concrete provider packages implement these contracts and translate the capabilities of their messaging technology into the common abstraction.

For example, a Kafka provider can map:

| Abstraction | Kafka concept |
|---|---|
| `Topic` | Topic |
| `ConsumerGroup` | Consumer Group / `GroupId` |
| `PendingMessages` | Consumer lag |
| `ActiveConsumers` | Active consumer group members |
| `Offset` | Record offset |
| `Partition` | Partition |

Provider-specific APIs, configuration objects, connection details, and administrative concepts remain outside the abstraction package.

Other providers can map the same contracts according to their own messaging semantics.

## 🌐 Recommended usage

Use this package when you want to:

- keep application code independent from broker-specific SDKs
- use a stable messaging contract across applications
- support multiple messaging providers over time
- distinguish logical consumers through consumer groups
- build testable message-driven components
- expose operational messaging information without coupling application code to broker monitoring APIs

## 🔜 Provider-friendly by design

The abstraction is designed to support implementations for messaging technologies such as:

- Apache Kafka
- IBM MQ
- RabbitMQ
- Azure Service Bus
- Redis Pub/Sub
- test, dummy, or in-memory providers

Not every provider is expected to support every optional capability.

The abstraction provides a common contract while allowing each implementation to accurately represent the capabilities and limitations of its underlying messaging technology.

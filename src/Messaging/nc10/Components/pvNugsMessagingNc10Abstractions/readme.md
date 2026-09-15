# 📡 pvNugsMessagingNc10Abstractions

A broker-neutral messaging abstraction library for .NET 10, designed to keep application code independent from specific message brokers such as Kafka, MQ Series, RabbitMQ, or Redis Pub/Sub.

[![NuGet](https://img.shields.io/badge/NuGet-pvNugsMessagingNc10Abstractions-0078D4?logo=nuget)](https://www.nuget.org/)
[![.NET 10](https://img.shields.io/badge/.NET-10.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![License](https://img.shields.io/badge/License-Project%20License-lightgrey)](https://github.com/)

## ✨ What this package provides

This abstraction package is intentionally generic and portable. It defines the contracts your application can depend on while leaving the concrete broker implementation to a provider package.

### Included contracts

- `IPvNugsMessagingProducer` — publishes a payload to a topic
- `IPvNugsMessagingConsumer` — subscribes to a topic and handles incoming messages
- `IPvNugsMessagingConsumer<T>` — typed consumer contract for deserializing payloads
- `PvNugsPublishResult` — broker-neutral result model for publication metadata

## 🧩 Design goals

- ✅ Broker-agnostic public API
- ✅ Ready for Kafka-first implementations without locking the abstraction to Kafka semantics
- ✅ Supports both string and typed payloads
- ✅ Keeps production code focused on business behavior, not infrastructure details
- ✅ Allows provider-specific implementations to add metadata without leaking broker-specific contracts into the core domain

## 🚀 Installation

### .NET CLI

```bash
dotnet add package pvNugsMessagingNc10Abstractions
```

### Package Manager Console

```powershell
Install-Package pvNugsMessagingNc10Abstractions
```

## 📦 Core types

### Producer contract

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

### Consumer contract

```csharp
public interface IPvNugsMessagingConsumer
{
    Task<Guid> SubscribeAsync(
        string topic,
        Func<string, PvNugsPublishResult, Task<bool>> handleIncomingMessageAsync,
        CancellationToken cancellationToken = default);

    Task UnsubscribeAsync(
        Guid subscriberId,
        CancellationToken cancellationToken = default);
}
```

### Typed consumer contract

```csharp
public interface IPvNugsMessagingConsumer<T> where T : class
{
    Task<Guid> SubscribeAsync(
        string topic,
        Func<string, Task<T>> factorAsync,
        Func<string, PvNugsPublishResult, T, Task<bool>> handleIncomingMessageAsync,
        CancellationToken cancellationToken = default);

    Task UnsubscribeAsync(
        Guid subscriberId,
        CancellationToken cancellationToken = default);
}
```

### Publication result

```csharp
public sealed class PvNugsPublishResult
{
    public string MessageId { get; init; } = string.Empty;
    public long? Offset { get; init; }
    public int? Partition { get; init; }
    public DateTimeOffset PublishedAt { get; init; } = DateTimeOffset.UtcNow;
}
```

## 🧠 Why `PvNugsPublishResult` instead of `long`?

Because the abstraction is intentionally generic.

A Kafka implementation can set:

- `Offset`
- `Partition`
- `MessageId`

Another broker may only populate:

- `MessageId`
- `PublishedAt`

This keeps the interface broker-neutral while still exposing useful metadata when available.

## 🏗️ Example usage

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

        Console.WriteLine($"Published message {result.MessageId} at {result.PublishedAt:O}");
    }
}
```

## 📌 Typical implementation pattern

A concrete broker package can implement the abstraction like this:

- Kafka provider implements `IPvNugsMessagingProducer` and `IPvNugsMessagingConsumer`
- The provider maps Kafka metadata into `PvNugsPublishResult`
- Application code depends only on the abstraction layer, not on Kafka types

This gives you a clean separation between:

- business logic
- messaging abstractions
- infrastructure-specific integrations

## 🌐 Recommended usage

Use this package when you want to:

- keep the application decoupled from broker-specific contracts
- introduce a messaging layer without committing to Kafka upfront
- support multiple providers over time with one stable interface
- build testable message-driven application components

## 🔜 Future-friendly

This package is the right foundation for additional provider implementations such as:

- Kafka
- RabbitMQ
- Azure Service Bus
- Redis Pub/Sub
- IBM MQ

If you want to keep the abstraction generic, this package is the right place to standardize the contracts.

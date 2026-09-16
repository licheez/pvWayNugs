# 📨 pvNugsMessagingNc10Kafka

[![NuGet](https://img.shields.io/nuget/v/pvNugsMessagingNc10Kafka.svg)](https://www.nuget.org/packages/pvNugsMessagingNc10Kafka/)
[![NuGet Downloads](https://img.shields.io/nuget/dt/pvNugsMessagingNc10Kafka.svg)](https://www.nuget.org/packages/pvNugsMessagingNc10Kafka/)
[![.NET](https://img.shields.io/badge/.NET-10.0-512BD4)](https://dotnet.microsoft.com/)
[![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)

Kafka implementation of the
`pvNugsMessagingNc10Abstractions` messaging contracts for .NET 10.

The package provides broker-neutral **Producer**, **Consumer** and
**Monitor** services backed by Apache Kafka.

---

## ✨ Features

- 🚀 Publish string or strongly typed messages to Kafka topics
- 📥 Consume string or strongly typed messages
- 👥 Explicit Kafka consumer-group support
- ✅ Explicit message acknowledgement through Kafka offset commits
- 🔍 Observe pending messages and active consumers
- 🔐 SASL/SSL authentication with credentials resolved through a secret manager
- 🏠 Simplified local Kafka configuration without authentication
- 🧪 Dummy producer and consumer modes
- 💉 Modular dependency-injection registration
- 🔄 Designed behind broker-neutral pvNugs messaging abstractions

---

## 📦 Installation

```bash
dotnet add package pvNugsMessagingNc10Kafka
```

The package implements the contracts defined by:

```text
pvNugsMessagingNc10Abstractions
```

---

## 🏗️ Architecture

The package separates messaging into three independent responsibilities:

```text
                  pvNugsMessagingNc10Abstractions
                             │
          ┌──────────────────┼───────────────────┐
          │                  │                   │
      Producer            Consumer             Monitor
          │                  │                   │
          └──────────────────┼───────────────────┘
                             │
                         Apache Kafka
```

Applications can register only the components they actually need.

For example, an API publishing commands may only require the Producer,
while a worker may only require a Consumer.

---

## ⚙️ Configuration

Kafka infrastructure configuration is shared by all components:

```json
{
  "PvNugsMessagingKafkaConfig": {
    "BootstrapServers": "localhost:9092"
  }
}
```

`IsLocal` is automatically derived from the bootstrap server.

A Kafka instance whose bootstrap server starts with `localhost` or
`127.0.0.1` is considered local and does not use SASL authentication.

### Producer configuration

```json
{
  "PvNugsMessagingKafkaProducerConfig": {
    "RetryBackoffMax": "00:00:00.100",
    "MessageSendMaxRetries": 5,
    "Dummy": false,
    "Acks": "All",
    "EnableIdempotence": true
  }
}
```

### Consumer configuration

```json
{
  "PvNugsMessagingKafkaConsumerConfig": {
    "SessionTimeout": "00:00:10",
    "Dummy": false,
    "AutoOffsetReset": "Earliest",
    "IsolationLevel": "ReadCommitted"
  }
}
```

Kafka consumers always use explicit offset commits.

Auto-commit is deliberately disabled by the provider.

---

## 🔐 Security

For non-local Kafka infrastructure, the provider uses:

```text
SecurityProtocol : SASL_SSL
SaslMechanism    : PLAIN
```

Credentials are resolved through the configured pvNugs secret manager
rather than stored directly in the Kafka configuration.

```json
{
  "PvNugsMessagingKafkaSecurityConfig": {
    "SaslUserNameParams": {
      "key": "value"
    },
    "SaslPasswordParams": {
      "key": "value"
    },
    "EnableSslCertificateVerification": true
  }
}
```

The parameter dictionaries are passed directly to the configured secret
manager. Their contents therefore depend on the secret-provider
implementation.

---

## 💉 Dependency Injection

Producer, Consumer and Monitor can be registered independently.

### 🚀 Producer only

```csharp
services.TryAddPvNugsMessagingKafkaProducer(configuration);
```

Registers:

```text
PvNugsMessagingKafkaConfig
PvNugsMessagingKafkaSecurityConfig
PvNugsMessagingKafkaProducerConfig

IPvNugsMessagingProducer
```

### 📥 Consumer only

```csharp
services.TryAddPvNugsMessagingKafkaConsumer(configuration);
```

Registers:

```text
PvNugsMessagingKafkaConfig
PvNugsMessagingKafkaSecurityConfig
PvNugsMessagingKafkaConsumerConfig

IPvNugsMessagingConsumer
IPvNugsMessagingConsumer<T>
```

### 🔍 Monitor only

```csharp
services.TryAddPvNugsMessagingKafkaMonitor(configuration);
```

Registers:

```text
PvNugsMessagingKafkaConfig
PvNugsMessagingKafkaSecurityConfig

IPvNugsMessagingMonitor
```

### 📦 Complete Kafka provider

Applications requiring all messaging capabilities can use:

```csharp
services.TryAddPvNugsMessagingKafka(configuration);
```

This registers Producer, Consumer and Monitor together.

---

## 🚀 Publishing messages

Inject the broker-neutral producer interface:

```csharp
public sealed class MyService(
    IPvNugsMessagingProducer producer)
{
    public async Task SendAsync()
    {
        var result = await producer.PublishAsync(
            "orders",
            "Hello Kafka!");
    }
}
```

### Publishing typed messages

Objects are serialized to JSON before being published:

```csharp
var order = new Order
{
    Id = 123,
    Product = "Coffee"
};

var result = await producer.PublishAsync(
    "orders",
    order);
```

The returned `PvNugsPublishResult` contains broker-neutral publication
metadata.

For Kafka, the message identifier is generated from:

```text
topic:partition:offset
```

Kafka partition and offset information are also exposed when available.

---

## 📥 Consuming messages

A subscription returns a unique subscription identifier.

The raw string consumer callback receives, in order, the source topic,
the publication metadata and the raw message payload:

```text
topic → publication metadata → raw message payload
```

For example:

```csharp
var subscriptionId =
    await consumer.SubscribeAsync(
        "orders",
        async (topic, publishResult, message) =>
        {
            Console.WriteLine(
                $"Received message {publishResult.MessageId} " +
                $"from topic '{topic}'.");

            await ProcessAsync(message);

            return true;
        });
```

Returning `true` confirms successful processing and causes the Kafka
offset to be committed.

Returning `false` leaves the offset uncommitted.

Subscriptions can be stopped explicitly:

```csharp
await consumer.UnsubscribeAsync(subscriptionId);
```

---

## 👥 Consumer Groups

A consumer group can be specified explicitly:

```csharp
await consumer.SubscribeAsync(
    "orders",
    async (topic, publishResult, message) =>
    {
        await ProcessAsync(message);
        return true;
    },
    consumerGroup: "billing");
```

Consumers belonging to the same Kafka consumer group share the topic
partitions assigned to that group.

Different consumer groups consume the topic independently.

When no consumer group is supplied, the provider uses the topic name as
the consumer group:

```text
consumerGroup ?? topic
```

This preserves the historical pvNugs Kafka consumption behaviour while
allowing applications to opt into explicit consumer groups.

---

## 🧩 Typed Consumers

Typed consumers separate message deserialization from message
processing.

They follow the same callback structure as raw string consumers:

```text
topic → publication metadata → typed message
```

The raw Kafka payload is first passed to the materialization factory,
which creates the application object supplied to the message handler.

```csharp
var subscriptionId =
    await consumer.SubscribeAsync(
        "orders",

        rawMessage =>
            Task.FromResult(
                JsonSerializer.Deserialize<Order>(rawMessage)!),

        async (topic, publishResult, order) =>
        {
            await ProcessOrderAsync(order);

            return true;
        },

        consumerGroup: "billing");
```

The factory receives the raw Kafka payload and creates the application
object.

The message handler then receives:

```text
topic
publication metadata
deserialized object
```

The raw and typed consumer contracts are therefore symmetrical:

```text
IPvNugsMessagingConsumer
    topic + PvNugsPublishResult + string

IPvNugsMessagingConsumer<T>
    topic + PvNugsPublishResult + T
```

---

## 🔍 Monitoring

The Kafka provider implements `IPvNugsMessagingMonitor`.

```csharp
var status = await monitor.GetTopicStatusAsync(
    "orders",
    "billing");
```

The monitor queries the current Kafka infrastructure directly and does
not maintain local monitoring state.

It currently exposes:

| Metric            | Kafka support    | Description                                                                                  |
| ----------------- | ---------------- | -------------------------------------------------------------------------------------------- |
| `PendingMessages` | ✅ Available      | Difference between partition high watermarks and committed consumer-group offsets            |
| `ActiveConsumers` | ✅ Available      | Active group members owning at least one partition of the requested topic                    |
| `LastActivity`    | 🚫 Not supported | Kafka does not expose an unambiguous last-activity timestamp for a topic/consumer-group pair |

### Metric availability

Monitoring values expose an explicit status:

```text
Available
NotApplicable
NotSupported
Unavailable
```

This distinction allows applications to differentiate between a value
that is zero, a metric that does not apply, a broker capability that
does not exist, and a temporary inability to retrieve a value.

If no consumer group is provided, group-dependent Kafka metrics are
reported as `NotApplicable`.

If a consumer group has no committed offset for one of the topic
partitions, `PendingMessages` is reported as `Unavailable` because the
monitor cannot safely infer the consumer's offset-reset policy.

---

## 🧪 Dummy Mode

Producer and Consumer configurations support a `Dummy` mode.

For a Producer, messages are not sent to Kafka and a synthetic
`PvNugsPublishResult` is generated.

For a Consumer, subscriptions are accepted without starting a Kafka
listener.

This is useful for applications that need the messaging contracts
available while external messaging is intentionally disabled.

---

## 🧠 Design Principles

The Kafka provider deliberately keeps Kafka-specific concepts behind
the broker-neutral pvNugs abstractions wherever possible.

The public application contracts use generic messaging concepts such
as:

```text
topic
consumer group
producer
consumer
monitor
```

Kafka-specific information such as partitions and offsets is exposed
only as optional metadata where useful.

The abstraction is designed so that other messaging providers can
implement the same contracts, including technologies such as:

* Apache Kafka
* IBM MQ
* RabbitMQ
* Azure Service Bus
* Redis Pub/Sub

Applications can therefore depend on the pvNugs messaging abstractions
rather than directly on a specific message broker.

---

## 📄 License

This project is licensed under the MIT License.

---

## 🏢 pvWay

Part of the **pvWay / pvNugs** collection of reusable .NET components.
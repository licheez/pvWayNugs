# ⏳ pvNugsSemaphoreNc9Abstractions

Distributed Semaphore (Mutex) Abstractions for .NET

---

## ✨ Features

- 🔒 Distributed, named semaphore (mutex) abstraction
- 🏷️ Identifies owners and supports timeouts
- ⏰ Expiry, force-release, and state machine support
- 🧩 Designed for dependency injection and extensibility
- 🧪 Testable and mockable interfaces

---

## 📦 Installation

```shell
# .NET CLI
 dotnet add package pvNugsSemaphoreNc9Abstractions

# or Package Manager
 Install-Package pvNugsSemaphoreNc9Abstractions
```

---

## 🚦 State Machine

The semaphore state machine is represented by the `SemaphoreStatusEnu` enum:

| State                  | Description                                                                 |
|------------------------|-----------------------------------------------------------------------------|
| 🟢 `Acquired`          | The semaphore is successfully acquired and exclusive access is granted.      |
| 🟡 `OwnedBySomeoneElse`| The semaphore is currently held by another requester and cannot be acquired. |
| 🔴 `ForcedAcquired`    | The semaphore was forcefully acquired (stolen) by the requester because the previous lock timed out and expired.   |

---

## 🧩 Main Interfaces

### `IPvNugsSemaphoreService`

Provides distributed semaphore management for coordinating access to shared resources across processes.

```csharp
public interface IPvNugsSemaphoreService
{
    Task<IPvNugsSemaphoreInfo> AcquireSemaphoreAsync(
        string name,
        string requester,
        TimeSpan timeout,
        CancellationToken ct = default
    );

    Task TouchSemaphoreAsync(
        string name,
        CancellationToken ct = default
    );

    Task ReleaseSemaphoreAsync(
        string name,
        CancellationToken ct = default
    );

    Task<IPvNugsSemaphoreInfo?> GetSemaphoreAsync(
        string name,
        string requester,
        CancellationToken ct = default
    );

    Task<T> IsolateWorkAsync<T>(
        string semaphoreName,
        string requester,
        TimeSpan timeout,
        Func<Task<T>> workAsync,
        Action<string>? notify = null,
        int sleepBetweenAttemptsInSeconds = 15,
        CancellationToken ct = default
    );

    Task IsolateWorkAsync(
        string semaphoreName,
        string requester,
        TimeSpan timeout,
        Func<Task> workAsync,
        Action<string>? notify = null,
        int sleepBetweenAttemptsInSeconds = 15,
        CancellationToken ct = default
    );
}
```

### `IPvNugsSemaphoreInfo`

Describes the state and metadata of a semaphore instance:

```csharp
public interface IPvNugsSemaphoreInfo
{
    SemaphoreStatusEnu Status { get; }
    string Name { get; }
    string Owner { get; }
    TimeSpan Timeout { get; }
    DateTime ExpiresAtUtc { get; }
    DateTime CreateDateUtc { get; }
    DateTime UpdateDateUtc { get; }
}
```

---

## 📝 Usage Example

```csharp
var info = await semaphoreService.AcquireSemaphoreAsync(
    "my-resource",
    Environment.MachineName,
    TimeSpan.FromMinutes(5),
    ct
);

if (info.Status == SemaphoreStatusEnu.Acquired)
{
    // Do exclusive work here
    await semaphoreService.ReleaseSemaphoreAsync("my-resource", ct);
}
else if (info.Status == SemaphoreStatusEnu.OwnedBySomeoneElse)
{
    // Handle contention
}
```

Or, to run code in an isolated context:

```csharp
await semaphoreService.IsolateWorkAsync(
    "my-resource",
    Environment.MachineName,
    TimeSpan.FromMinutes(5),
    async () =>
    {
        // Your exclusive work here
    },
    notify: msg => Console.WriteLine(msg),
    sleepBetweenAttemptsInSeconds: 10,
    ct
);
```

---

## 🧪 Testing & Extensibility

- All interfaces are mockable for unit testing.
- Designed for extension: implement your own distributed semaphore provider.

---

## 📄 License

MIT License. See LICENSE file for details.

---

## 🔗 Related Packages

- `pvNugsSemaphoreNc9Local` – Local in-memory implementation
- `pvNugsSemaphoreNc9Abstractions` – This package (interfaces & contracts)

---

## 🏷️ NuGet Package Tags

```
distributed-semaphore, distributed-mutex, in-process-synchronization, thread-safe, locking, concurrency, .NET, abstractions, DI, dependency-injection, testable, extensible
```

Add these tags to your NuGet package metadata for better discoverability and clarity.

---

For more information, see the source repository or contact the maintainer.

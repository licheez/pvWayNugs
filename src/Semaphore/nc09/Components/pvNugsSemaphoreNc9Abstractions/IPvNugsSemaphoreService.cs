namespace pvNugsSemaphoreNc9Abstractions;

/// <summary>
/// Provides distributed semaphore (mutex) management for coordinating
/// access to shared resources across processes.
/// </summary>
public interface IPvNugsSemaphoreService
{
    /// <summary>
    /// Attempts to acquire a named semaphore for the specified requester
    /// and timeout period.
    /// </summary>
    /// <param name="name">
    /// The unique name of the semaphore.
    /// </param>
    /// <param name="requester">
    /// The identifier of the process or entity requesting the semaphore.
    /// </param>
    /// <param name="timeout">
    /// The duration for which the lock should remain valid. If the lock
    /// is held longer than this period, it may be forcefully released.
    /// </param>
    /// <param name="ct">
    /// A cancellation token to observe while waiting for the task to complete.
    /// </param>
    /// <returns>
    /// A task that returns semaphore information. The status indicates
    /// whether the semaphore was acquired or the reason for failure.
    /// </returns>
    Task<IPvNugsSemaphoreInfo> AcquireSemaphoreAsync(
        string name,
        string requester,
        TimeSpan timeout,
        CancellationToken ct = default
    );

    /// <summary>
    /// Extends the validity period of an acquired semaphore, preventing
    /// it from expiring.
    /// </summary>
    /// <param name="name">
    /// The name of the semaphore to touch.
    /// </param>
    /// <param name="ct">
    /// A cancellation token to observe while waiting for the task to complete.
    /// </param>
    Task TouchSemaphoreAsync(
        string name,
        CancellationToken ct = default
    );

    /// <summary>
    /// Releases a previously acquired semaphore, making it available for
    /// other requesters.
    /// </summary>
    /// <param name="name">
    /// The name of the semaphore to release.
    /// </param>
    /// <param name="ct">
    /// A cancellation token to observe while waiting for the task to complete.
    /// </param>
    Task ReleaseSemaphoreAsync(
        string name,
        CancellationToken ct = default
    );

    /// <summary>
    /// Retrieves information about a specific semaphore for a given requester.
    /// </summary>
    /// <param name="name">
    /// The name of the semaphore.
    /// </param>
    /// <param name="requester">
    /// The identifier of the requester.
    /// </param>
    /// <param name="ct">
    /// A cancellation token to observe while waiting for the task to complete.
    /// </param>
    /// <returns>
    /// A task that returns semaphore information if found; otherwise, null.
    /// </returns>
    Task<IPvNugsSemaphoreInfo?> GetSemaphoreAsync(
        string name,
        string requester,
        CancellationToken ct = default
    );

    /// <summary>
    /// Executes a function in a semaphore-protected context, ensuring
    /// exclusive access for the duration of the work.
    /// </summary>
    /// <typeparam name="T">
    /// The return type of the asynchronous work function.
    /// </typeparam>
    /// <param name="semaphoreName">
    /// The name of the semaphore (mutex) to acquire.
    /// </param>
    /// <param name="requester">
    /// The identifier of the requester (e.g., machine name).
    /// </param>
    /// <param name="timeout">
    /// The validity period for the lock.
    /// </param>
    /// <param name="workAsync">
    /// The asynchronous function to execute within the protected context.
    /// </param>
    /// <param name="notify">
    /// Optional callback for status or sleep notifications.
    /// </param>
    /// <param name="sleepBetweenAttemptsInSeconds">
    /// Seconds to wait between acquisition attempts if the semaphore is unavailable.
    /// </param>
    /// <param name="ct">
    /// A cancellation token to observe while waiting for the task to complete.
    /// </param>
    /// <returns>
    /// The result of the executed work function.
    /// </returns>
    Task<T> IsolateWorkAsync<T>(
        string semaphoreName,
        string requester,
        TimeSpan timeout,
        Func<Task<T>> workAsync,
        Action<string>? notify = null,
        int sleepBetweenAttemptsInSeconds = 15,
        CancellationToken ct = default
    );

    /// <summary>
    /// Executes an asynchronous action in a semaphore-protected context,
    /// ensuring exclusive access for the duration of the work.
    /// </summary>
    /// <param name="semaphoreName">
    /// The name of the semaphore (mutex) to acquire.
    /// </param>
    /// <param name="requester">
    /// The identifier of the requester (e.g., machine name).
    /// </param>
    /// <param name="timeout">
    /// The validity period for the lock.
    /// </param>
    /// <param name="workAsync">
    /// The asynchronous action to execute within the protected context.
    /// </param>
    /// <param name="notify">
    /// Optional callback for status or sleep notifications.
    /// </param>
    /// <param name="sleepBetweenAttemptsInSeconds">
    /// Seconds to wait between acquisition attempts if the semaphore is unavailable.
    /// </param>
    /// <param name="ct">
    /// A cancellation token to observe while waiting for the task to complete.
    /// </param>
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

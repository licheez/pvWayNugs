namespace pvNugsSemaphoreNc9Abstractions;

/// <summary>
/// Represents information about a distributed semaphore instance, including
/// its status, ownership, timeout, and relevant timestamps.
/// </summary>
public interface IPvNugsSemaphoreInfo
{
    /// <summary>
    /// Gets the current status of the semaphore (e.g., acquired, released, owned by someone else).
    /// </summary>
    SemaphoreStatusEnu Status { get; }

    /// <summary>
    /// Gets the unique name of the semaphore.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Gets the identifier of the current owner of the semaphore.
    /// </summary>
    string Owner { get; }

    /// <summary>
    /// Gets the timeout duration for which the semaphore is valid.
    /// </summary>
    TimeSpan Timeout { get; }

    /// <summary>
    /// Gets the UTC date and time when the semaphore will expire.
    /// </summary>
    DateTime ExpiresAtUtc { get; }

    /// <summary>
    /// Gets the UTC date and time when the semaphore was created.
    /// </summary>
    DateTime CreateDateUtc { get; }

    /// <summary>
    /// Gets the UTC date and time when the semaphore was last updated.
    /// </summary>
    DateTime UpdateDateUtc { get; }
}
using pvNugsSemaphoreNc9Abstractions;

namespace pvNugsSemaphoreNc9MsSql;

/// <summary>
/// Represents the state and metadata of a distributed semaphore instance as returned by the SQL Server-based implementation.
/// </summary>
internal class SemaphoreInfo : IPvNugsSemaphoreInfo
{
    /// <summary>
    /// Gets the current status of the semaphore (e.g., acquired, owned by someone else, or forcefully acquired).
    /// </summary>
    public SemaphoreStatusEnu Status { get; }

    /// <summary>
    /// Gets the unique name of the semaphore.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets the identifier of the current owner of the semaphore.
    /// </summary>
    public string Owner { get; }

    /// <summary>
    /// Gets the timeout duration for which the semaphore is valid.
    /// </summary>
    public TimeSpan Timeout { get; }

    /// <summary>
    /// Gets the UTC date and time when the semaphore will expire.
    /// </summary>
    public DateTime ExpiresAtUtc { get; }

    /// <summary>
    /// Gets the UTC date and time when the semaphore was created.
    /// </summary>
    public DateTime CreateDateUtc { get; }

    /// <summary>
    /// Gets the UTC date and time when the semaphore was last updated.
    /// </summary>
    public DateTime UpdateDateUtc { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="SemaphoreInfo"/> class with the specified state and metadata.
    /// </summary>
    /// <param name="status">The status of the semaphore.</param>
    /// <param name="name">The unique name of the semaphore.</param>
    /// <param name="owner">The identifier of the semaphore owner.</param>
    /// <param name="timeout">The timeout duration for the semaphore.</param>
    /// <param name="expiresAtUtc">The UTC date and time when the semaphore will expire.</param>
    /// <param name="createDateUtc">The UTC date and time when the semaphore was created.</param>
    /// <param name="updateDateUtc">The UTC date and time when the semaphore was last updated.</param>
    public SemaphoreInfo(
        SemaphoreStatusEnu status,
        string name,
        string? owner,
        TimeSpan timeout,
        DateTime expiresAtUtc,
        DateTime createDateUtc,
        DateTime updateDateUtc)
    {
        Status = status;
        Name = name;
        Owner = owner ?? "unknown";
        Timeout = timeout;
        ExpiresAtUtc = expiresAtUtc;
        CreateDateUtc = createDateUtc;
        UpdateDateUtc = updateDateUtc;
    }

    public override string ToString()
    {
        return $"semaphore {Name} " +
               $"status: {Status} " +
               $"owner: {Owner} " +
               $"timeout: {Timeout} " +
               $"expires at: {ExpiresAtUtc}";
    }
}
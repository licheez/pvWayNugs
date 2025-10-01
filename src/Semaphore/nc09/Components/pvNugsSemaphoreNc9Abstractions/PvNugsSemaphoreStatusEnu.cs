namespace pvNugsSemaphoreNc9Abstractions;

/// <summary>
/// Represents the possible states of a distributed semaphore in the
/// semaphore service state machine.
/// </summary>
public enum SemaphoreStatusEnu
{
    /// <summary>
    /// The semaphore has been successfully acquired by the requester and
    /// exclusive access is granted.
    /// </summary>
    Acquired,

    /// <summary>
    /// The semaphore is currently owned by another requester and cannot be
    /// acquired at this time.
    /// </summary>
    OwnedBySomeoneElse,

    /// <summary>
    /// The semaphore was forcefully acquired (stolen) by the requester because
    /// the previous lock timed out and expired.
    /// </summary>
    ForcedAcquired
}
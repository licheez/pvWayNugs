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
    /// The semaphore was acquired, but has been released by another process
    /// or due to timeout before the requester could complete its work.
    /// </summary>
    ReleasedInTheMeanTime,

    /// <summary>
    /// The semaphore is currently owned by another requester and cannot be
    /// acquired at this time.
    /// </summary>
    OwnedBySomeoneElse,

    /// <summary>
    /// The semaphore was forcefully released, typically due to expiration
    /// or administrative intervention.
    /// </summary>
    ForcedReleased
}
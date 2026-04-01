using pvNugsMediatorNc9Abstractions.pvNugs;

namespace pvNugsMediatorNc9.it.Requests.TaskReturning;

/// <summary>
/// Request to delete a user - using NEW Task-returning handler (no Unit.Value needed)
/// </summary>
public class DeleteUserRequest : IPvNugsMediatorRequest
{
    public int UserId { get; init; }
}


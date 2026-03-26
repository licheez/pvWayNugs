using pvNugsMediatorNc9Abstractions.Mediator;

namespace pvNugsMediatorNc9.it.ScopingTests;

/// <summary>
/// Test request that returns database context information.
/// </summary>
public record ScopedTestRequest(string RequestName) : IRequest<ScopedTestResponse>;

/// <summary>
/// Response containing information about the scoped services.
/// </summary>
public record ScopedTestResponse
{
    public required string RequestName { get; init; }
    public required Guid DbContextId { get; init; }
    public required Guid HandlerId { get; init; }
    public required int DataCount { get; init; }
}


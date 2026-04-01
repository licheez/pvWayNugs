using pvNugsMediatorNc9Abstractions;
using pvNugsMediatorNc9Abstractions.pvNugs;
using pvNugsMediatorNc9.it.Requests.TaskReturning;

namespace pvNugsMediatorNc9.it.Handlers.TaskReturning;

/// <summary>
/// Handler using the NEW IPvNugsMediatorRequestHandler&lt;TRequest&gt; interface
/// Returns Task instead of Task&lt;Unit&gt; - cleaner!
/// </summary>
[MediatorHandler(Lifetime = ServiceLifetime.Transient)]
public class DeleteUserHandler : IPvNugsMediatorRequestHandler<DeleteUserRequest>
{
    public static int CallCount { get; private set; }
    public static int? LastDeletedUserId { get; private set; }

    public async Task HandleAsync(DeleteUserRequest request, CancellationToken cancellationToken = default)
    {
        // Simulate async work
        await Task.Delay(10, cancellationToken);
        
        CallCount++;
        LastDeletedUserId = request.UserId;
        
        Console.WriteLine($"✓ [NEW] Deleted user with ID: {request.UserId} (no Unit.Value needed!)");
        
        // No need to return Unit.Value! Just returns Task
    }

    public static void Reset()
    {
        CallCount = 0;
        LastDeletedUserId = null;
    }
}


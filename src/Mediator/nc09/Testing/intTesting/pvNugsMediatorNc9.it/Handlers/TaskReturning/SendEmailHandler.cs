using pvNugsMediatorNc9Abstractions;
using pvNugsMediatorNc9Abstractions.Mediator;
using pvNugsMediatorNc9.it.Requests.TaskReturning;

namespace pvNugsMediatorNc9.it.Handlers.TaskReturning;

/// <summary>
/// Handler using the MediatR-style IRequestHandler&lt;TRequest&gt; interface
/// Returns Task instead of Task&lt;Unit&gt; - MediatR compatibility!
/// </summary>
[MediatorHandler(Lifetime = ServiceLifetime.Transient)]
public class SendEmailHandler : IRequestHandler<SendEmailRequest>
{
    public static int CallCount { get; private set; }
    public static SendEmailRequest? LastRequest { get; private set; }

    public async Task Handle(SendEmailRequest request, CancellationToken cancellationToken = default)
    {
        // Simulate async work
        await Task.Delay(10, cancellationToken);
        
        CallCount++;
        LastRequest = request;
        
        Console.WriteLine($"✓ [MediatR-style] Sent email to: {request.To} (no Unit.Value needed!)");
        
        // No need to return Unit.Value! Just returns Task
    }

    public static void Reset()
    {
        CallCount = 0;
        LastRequest = null;
    }
}


using pvNugsLoggerNc9Abstractions;
using pvNugsMediatorNc9Abstractions;
using pvNugsMediatorNc9Abstractions.pvNugs;

namespace pvNugsMediatorNc9.it.ScopingTests;

/// <summary>
/// Transient handler that should get a NEW scoped DbContext for each invocation.
/// </summary>
[MediatorHandler(Lifetime = ServiceLifetime.Transient)]
public class TransientTestHandler : IPvNugsMediatorRequestHandler<ScopedTestRequest, ScopedTestResponse>
{
    private readonly TestDbContext _db;
    private readonly ILoggerService _logger;
    private Guid HandlerId { get; } = Guid.NewGuid();
    
    public TransientTestHandler(
        TestDbContext db, 
        ILoggerService logger)
    {
        _db = db;
        _logger = logger;
        Console.WriteLine($"  [Transient Handler {HandlerId:N}] Created with DbContext {_db.InstanceId:N}");
    }
    
    public async Task<ScopedTestResponse> HandleAsync(
        ScopedTestRequest request, 
        CancellationToken cancellationToken)
    {
        await _logger.LogAsync(
            $"Processing {request.RequestName} in TRANSIENT handler {HandlerId:N}", 
            SeverityEnu.Info);
        
        _db.Data.Add($"Processed by transient: {request.RequestName}");
        
        return new ScopedTestResponse
        {
            RequestName = request.RequestName,
            DbContextId = _db.InstanceId,
            HandlerId = HandlerId,
            DataCount = _db.Data.Count
        };
    }
}


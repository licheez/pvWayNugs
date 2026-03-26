using pvNugsLoggerNc9Abstractions;
using pvNugsMediatorNc9Abstractions;
using pvNugsMediatorNc9Abstractions.pvNugs;

namespace pvNugsMediatorNc9.it.ScopingTests;

/// <summary>
/// Scoped handler that demonstrates proper scoped dependency injection.
/// Each request scope should get a new instance of this handler with its own scoped DbContext.
/// </summary>
[MediatorHandler(Lifetime = ServiceLifetime.Scoped)]
public class ScopedTestHandler : IPvNugsMediatorRequestHandler<ScopedTestRequest, ScopedTestResponse>
{
    private readonly TestDbContext _db;
    private readonly ILoggerService _logger;
    private Guid HandlerId { get; } = Guid.NewGuid();
    
    public ScopedTestHandler(TestDbContext db, ILoggerService logger)
    {
        _db = db;
        _logger = logger;
        Console.WriteLine($"  [Handler {HandlerId:N}] Created with DbContext {_db.InstanceId:N}");
    }
    
    public async Task<ScopedTestResponse> HandleAsync(
        ScopedTestRequest request, 
        CancellationToken cancellationToken)
    {
        await _logger.LogAsync(
            $"Processing {request.RequestName} in handler {HandlerId:N}", 
            SeverityEnu.Info);
        
        // Simulate database operation
        _db.Data.Add($"Processed by {request.RequestName}");
        
        return new ScopedTestResponse
        {
            RequestName = request.RequestName,
            DbContextId = _db.InstanceId,
            HandlerId = HandlerId,
            DataCount = _db.Data.Count
        };
    }
}


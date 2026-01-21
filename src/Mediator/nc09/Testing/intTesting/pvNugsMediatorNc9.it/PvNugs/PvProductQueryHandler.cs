using pvNugsLoggerNc9Abstractions;
using pvNugsMediatorNc9Abstractions;
using pvNugsMediatorNc9Abstractions.pvNugs;

namespace pvNugsMediatorNc9.it.PvNugs;

/// <summary>
/// Handler for ProductQueryRequest that will be invoked directly without pipelines
/// </summary>
[MediatorHandler(Lifetime = ServiceLifetime.Transient)]
public class PvProductQueryHandler(
    IConsoleLoggerService logger): 
    IPvNugsMediatorRequestHandler<PvProductQueryRequest, string>
{

    public async Task<string> HandleAsync(
        PvProductQueryRequest request, 
        CancellationToken cancellationToken = default)
    {
        await logger.LogAsync(
            $"[ProductQueryHandler] Retrieving product with ID {request.ProductId} (NO PIPELINES)",
            SeverityEnu.Trace);
        
        // Simulate database lookup
        return $"Product #{request.ProductId}: Sample Product";
    }
}

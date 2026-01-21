using pvNugsLoggerNc9Abstractions;
using pvNugsMediatorNc9Abstractions;
using pvNugsMediatorNc9Abstractions.Mediator;

namespace pvNugsMediatorNc9.it.Mediator;

/// <summary>
/// Handler for ProductQueryRequest that will be invoked directly without pipelines
/// </summary>
[MediatorHandler(Lifetime = ServiceLifetime.Transient)]
public class MdProductQueryHandler(
    IConsoleLoggerService logger): 
    IRequestHandler<MdProductQueryRequest, string>
{
    public async Task<string> Handle(
        MdProductQueryRequest request, 
        CancellationToken cancellationToken = default)
    {
        await logger.LogAsync(
            $"[ProductQueryHandler] Retrieving product with ID {request.ProductId} (NO PIPELINES)",
            SeverityEnu.Trace);
        
        // Simulate database lookup
        return $"Product #{request.ProductId}: Sample Product";
    }
}

using pvNugsLoggerNc9Abstractions;
using pvNugsMediatorNc9Abstractions.Mediator;

namespace pvNugsMediatorNc9.it;

/// <summary>
/// Request to get product information by ID (for testing non-pipeline scenario)
/// </summary>
public class ProductQueryRequest(int productId): 
    IRequest<string>
{
    public int ProductId { get; } = productId;
}

/// <summary>
/// Handler for ProductQueryRequest that will be invoked directly without pipelines
/// </summary>
public class ProductQueryHandler(
    IConsoleLoggerService logger): 
    IRequestHandler<ProductQueryRequest, string>
{
    public async Task<string> Handle(
        ProductQueryRequest request, 
        CancellationToken cancellationToken = default)
    {
        await logger.LogAsync(
            $"[ProductQueryHandler] Retrieving product with ID {request.ProductId} (NO PIPELINES)",
            SeverityEnu.Trace);
        
        // Simulate database lookup
        return $"Product #{request.ProductId}: Sample Product";
    }
}

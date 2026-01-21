using pvNugsMediatorNc9Abstractions;
using pvNugsMediatorNc9Abstractions.Mediator;

namespace pvNugsMediatorNc9.it.Mediator;

/// <summary>
/// Request to get product information by ID (for testing non-pipeline scenario)
/// </summary>
[MediatorHandler(Lifetime = ServiceLifetime.Transient)]
public class MdProductQueryRequest(int productId): 
    IRequest<string>
{
    public int ProductId { get; } = productId;
}
using pvNugsMediatorNc9Abstractions;
using pvNugsMediatorNc9Abstractions.pvNugs;

namespace pvNugsMediatorNc9.it.PvNugs;

/// <summary>
/// Request to get product information by ID (for testing non-pipeline scenario)
/// </summary>
[MediatorHandler(Lifetime = ServiceLifetime.Transient)]
public class PvProductQueryRequest(int productId): 
    IPvNugsMediatorRequest<string>
{
    public int ProductId { get; } = productId;
}
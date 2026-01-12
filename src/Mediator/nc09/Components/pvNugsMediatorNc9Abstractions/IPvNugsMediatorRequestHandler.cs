namespace pvNugsMediatorNc9Abstractions;

public interface IPvNugsMediatorRequestHandler<in TRequest, TResponse>
    where TRequest : IPvNugsMediatorRequest<TResponse>
{
    Task<TResponse> HandleAsync(
        TRequest request, 
        CancellationToken cancellationToken = default);
}
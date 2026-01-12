namespace pvNugsMediatorNc9Abstractions;

public delegate Task RequestHandlerDelegate<TResponse>();

public interface IPvNugsPipelineMediator<in TRequest, TResponse>
    where TRequest : IPvNugsMediatorRequest<TResponse>
{
    Task<TResponse> HandleAsync(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken = default);
}
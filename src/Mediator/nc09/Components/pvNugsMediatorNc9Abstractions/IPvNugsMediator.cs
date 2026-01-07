namespace pvNugsMediatorNc9Abstractions;

public interface IPvNugsMediatorRequest : IPvNugsMediatorRequest<Unit>;

public interface IPvNugsMediatorRequestHandler<in TRequest>: 
    IPvNugsMediatorRequestHandler<TRequest, Unit>
    where TRequest : IPvNugsMediatorRequest;

public interface IPvNugsMediator
{
    Task<TResponse> SendAsync<TResponse>(
        IPvNugsMediatorRequest<TResponse> request, 
        CancellationToken cancellationToken = default);
    
    Task PublishAsync<TNotification>(
        IPvNugsMediatorNotification notification, 
        CancellationToken cancellationToken = default)
        where TNotification : IPvNugsMediatorNotification;
    
    Task PublishAsync(
        object notification, 
        CancellationToken cancellationToken = default);
}
namespace pvNugsMediatorNc10Abstractions;

public interface IPvNugsMediatorRequest<TResponse>;
public interface IPvNugsMediatorRequest : IPvNugsMediatorRequest<Unit>;

public interface IPvNugsMediatorRequestHandler<in TRequest, TResponse>
    where TRequest : IPvNugsMediatorRequest<TResponse>
{
    Task<TResponse> Handle(
        TRequest request, 
        CancellationToken cancellationToken = default);
}

public interface IPvNugsMediatorRequestHandler<in TRequest>: 
    IPvNugsMediatorRequestHandler<TRequest, Unit>
    where TRequest : IPvNugsMediatorRequest;
    
public interface IPvNugsMediatorNotification;

public interface IPvNugsMediatorNotificationHandler<in TNotification>
    where TNotification : IPvNugsMediatorNotification
{
    Task Handle(
        TNotification notification, 
        CancellationToken cancellationToken = default);
}

public interface IPvNugsMediator
{
    Task<TResponse> Send<TResponse>(
        IPvNugsMediatorRequest<TResponse> request, 
        CancellationToken cancellationToken = default);
    
    Task Publish<TNotification>(
        IPvNugsMediatorNotification notification, 
        CancellationToken cancellationToken = default)
        where TNotification : IPvNugsMediatorNotification;
    
    Task Publish(
        object notification, 
        CancellationToken cancellationToken = default);
}

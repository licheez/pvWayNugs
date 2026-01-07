using pvNugsMediatorNc10Abstractions;

namespace pvNugsMediatorNc10;

public class Mediator(IServiceProvider sp): IPvNugsMediator
{
    public async Task<TResponse> Send<TResponse>(
        IPvNugsMediatorRequest<TResponse> request, 
        CancellationToken cancellationToken = default)
    {
        
        throw new NotImplementedException();
    }

    public async Task Publish<TNotification>(
        IPvNugsMediatorNotification notification, 
        CancellationToken cancellationToken = default) 
        where TNotification : IPvNugsMediatorNotification
    {
        throw new NotImplementedException();
    }

    public async Task Publish(
        object notification, 
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
namespace pvNugsMediatorNc9Abstractions;

public interface IPvNugsMediatorNotificationHandler<in TNotification>
    where TNotification : IPvNugsMediatorNotification
{
    Task HandleAsync(
        TNotification notification, 
        CancellationToken cancellationToken = default);
}
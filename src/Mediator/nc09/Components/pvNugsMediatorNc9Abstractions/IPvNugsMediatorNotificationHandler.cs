namespace pvNugsMediatorNc9Abstractions;

public interface IPvNugsMediatorNotificationHandler<in TNotification>
    where TNotification : IPvNugsMediatorNotification
{
    Task Handle(
        TNotification notification, 
        CancellationToken cancellationToken = default);
}
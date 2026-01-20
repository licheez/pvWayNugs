using pvNugsLoggerNc9Abstractions;
using pvNugsMediatorNc9Abstractions.Mediator;

namespace pvNugsMediatorNc9.it;

public class MainNotificationHandler(IConsoleLoggerService logger) : 
    INotificationHandler<Notification>
{
    public async Task Handle(Notification notification,
        CancellationToken cancellationToken = default)
    {
        await logger.LogAsync($"MainNotificationHandler is publishing '{notification.Message}'");
    }
}

public class AlternateNotificationHandler(IConsoleLoggerService logger) : 
    INotificationHandler<Notification>
{
    public async Task Handle(Notification notification,
        CancellationToken cancellationToken = default)
    {
        await logger.LogAsync($"AlternateNotificationHandler is publishing '{notification.Message}'");
    }
}
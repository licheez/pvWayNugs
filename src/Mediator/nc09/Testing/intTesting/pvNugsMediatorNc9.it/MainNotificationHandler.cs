using pvNugsLoggerNc9Abstractions;
using pvNugsMediatorNc9Abstractions;

namespace pvNugsMediatorNc9.it;

public class MainNotificationHandler(IConsoleLoggerService logger) : IPvNugsMediatorNotificationHandler<Notification>
{
    public async Task HandleAsync(Notification notification,
        CancellationToken cancellationToken = default)
    {
        await logger.LogAsync($"MainNotificationHandler is publishing '{notification.Message}'");
    }
}

public class AlternateNotificationHandler(IConsoleLoggerService logger) : IPvNugsMediatorNotificationHandler<Notification>
{
    public async Task HandleAsync(Notification notification,
        CancellationToken cancellationToken = default)
    {
        await logger.LogAsync($"AlternateNotificationHandler is publishing '{notification.Message}'");
    }
}
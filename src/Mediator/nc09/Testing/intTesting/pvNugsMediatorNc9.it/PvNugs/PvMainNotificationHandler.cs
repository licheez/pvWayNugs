using pvNugsLoggerNc9Abstractions;
using pvNugsMediatorNc9Abstractions.pvNugs;

namespace pvNugsMediatorNc9.it.PvNugs;

public class PvMainNotificationHandler(IConsoleLoggerService logger) : 
    IPvNugsMediatorNotificationHandler<PvNotification>
{
    public async Task HandleAsync(
        PvNotification notification, 
        CancellationToken cancellationToken = default)
    {
        await logger.LogAsync($"AlternateNotificationHandler is publishing '{notification.Message}'");
    }
}
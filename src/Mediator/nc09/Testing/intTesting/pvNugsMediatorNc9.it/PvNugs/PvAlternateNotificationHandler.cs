using pvNugsLoggerNc9Abstractions;
using pvNugsMediatorNc9Abstractions.pvNugs;

namespace pvNugsMediatorNc9.it.PvNugs;

public class PvAlternateNotificationHandler(IConsoleLoggerService logger) : 
    IPvNugsMediatorNotificationHandler<PvNotification>
{
    public async Task Handle(PvNotification mdNotification,
        CancellationToken cancellationToken = default)
    {
        await logger.LogAsync($"AlternateNotificationHandler is publishing '{mdNotification.Message}'");
    }

    public async Task HandleAsync(
        PvNotification notification, 
        CancellationToken cancellationToken = default)
    {
        await Handle(notification, cancellationToken);
    }
}
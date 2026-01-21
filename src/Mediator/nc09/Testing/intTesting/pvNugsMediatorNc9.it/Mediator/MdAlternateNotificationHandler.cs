using pvNugsLoggerNc9Abstractions;
using pvNugsMediatorNc9Abstractions;
using pvNugsMediatorNc9Abstractions.Mediator;

namespace pvNugsMediatorNc9.it.Mediator;

[MediatorHandler(Lifetime = ServiceLifetime.Transient)]
public class MdAlternateNotificationHandler(IConsoleLoggerService logger) : 
    INotificationHandler<MdNotification>
{
    public async Task Handle(MdNotification mdNotification,
        CancellationToken cancellationToken = default)
    {
        await logger.LogAsync($"AlternateNotificationHandler is publishing '{mdNotification.Message}'");
    }
}
using pvNugsLoggerNc9Abstractions;
using pvNugsMediatorNc9Abstractions;
using pvNugsMediatorNc9Abstractions.pvNugs;

namespace pvNugsMediatorNc9.it.PvNugs;

[MediatorHandler(Lifetime = ServiceLifetime.Transient)]
public class PvUserCreationHandler(
    IConsoleLoggerService logger):
    IPvNugsMediatorRequestHandler<PvUserCreationRequest, Guid>
{
    public async Task<Guid> HandleAsync(
        PvUserCreationRequest request, 
        CancellationToken cancellationToken = default)
    {
        await logger.LogAsync(
            $"Creating user with username '{request.Username}' " +
            $"and email '{request.Email}'",
            SeverityEnu.Trace);
        
        return Guid.NewGuid();
    }
}
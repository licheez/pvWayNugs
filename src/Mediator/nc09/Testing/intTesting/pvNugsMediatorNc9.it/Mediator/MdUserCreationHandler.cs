using pvNugsLoggerNc9Abstractions;
using pvNugsMediatorNc9Abstractions;
using pvNugsMediatorNc9Abstractions.Mediator;

namespace pvNugsMediatorNc9.it.Mediator;

[MediatorHandler(Lifetime = ServiceLifetime.Transient)]
public class MdUserCreationHandler(
    IConsoleLoggerService logger): 
    IRequestHandler<MdUserCreationRequest, Guid>
{
    
    public async Task<Guid> Handle(
        MdUserCreationRequest request, 
        CancellationToken cancellationToken = default)
    {
        await logger.LogAsync(
            $"Creating user with username '{request.Username}' " +
            $"and email '{request.Email}'",
            SeverityEnu.Trace);
        
        return Guid.NewGuid();
    }
}
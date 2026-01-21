using pvNugsLoggerNc9Abstractions;
using pvNugsMediatorNc9Abstractions;
using pvNugsMediatorNc9Abstractions.Mediator;
using pvNugsMediatorNc9Abstractions.pvNugs;

namespace pvNugsMediatorNc9.it.PvNugs;

[MediatorHandler(Lifetime = ServiceLifetime.Transient)]
public class PvLoggingPipeline(IConsoleLoggerService logger): 
    IPvNugsMediatorPipelineRequestHandler<PvUserCreationRequest, Guid>
{
    public async Task<Guid> HandleAsync(
        PvUserCreationRequest request, 
        RequestHandlerDelegate<Guid> next,
        CancellationToken cancellationToken = default)
    {
        await logger.LogAsync(
            "PvLoggingPipeline: Before handling request for user " +
            $"'{request.Username}' with email '{request.Email}'", 
            SeverityEnu.Trace);
        
        var result = await next();
        
        await logger.LogAsync(
            "PvLoggingPipeline: After handling request." +
            $" Result = {result}", 
            SeverityEnu.Trace);
        
        return result;
    }
}
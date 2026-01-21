using pvNugsLoggerNc9Abstractions;
using pvNugsMediatorNc9Abstractions;
using pvNugsMediatorNc9Abstractions.Mediator;

namespace pvNugsMediatorNc9.it.Mediator;

[MediatorHandler(Lifetime = ServiceLifetime.Transient)]
public class MdValidationPipeline(IConsoleLoggerService logger): 
    IPipelineBehavior<MdUserCreationRequest, Guid>
{
    public async Task<Guid> Handle(
        MdUserCreationRequest request, 
        RequestHandlerDelegate<Guid> next, 
        CancellationToken cancellationToken = default)
    {
        await logger.LogAsync(
            "MdValidationPipeline: Before handling request for user " +
            $"'{request.Username}' with email '{request.Email}'", 
            SeverityEnu.Trace);
        
        var result = await next();
        
        await logger.LogAsync(
            "MdValidationPipeline: After handling request." +
            $" Result = {result}", 
            SeverityEnu.Trace);
        
        return result;
    }
}
using pvNugsLoggerNc9Abstractions;
using pvNugsMediatorNc9Abstractions;

namespace pvNugsMediatorNc9.it;

public class LoggingPipeline(IConsoleLoggerService logger): 
    IPvNugsPipelineMediator<UserCreationRequest, Guid>
{
    public async Task<Guid> Handle(
        UserCreationRequest request, 
        RequestHandlerDelegate<Guid> next, 
        CancellationToken cancellationToken = default)
    {
        await logger.LogAsync(
            "LoggingPipeline: Before handling request for user " +
            $"'{request.Username}' with email '{request.Email}'", 
            SeverityEnu.Trace);
        
        var result = await (Task<Guid>) next();
        
        await logger.LogAsync(
            "LoggingPipeline: After handling request." +
            $" Result = {result}", 
            SeverityEnu.Trace);
        
        return result;
    }
}
using pvNugsLoggerNc9Abstractions;
using pvNugsMediatorNc9Abstractions.Mediator;

namespace pvNugsMediatorNc9.it;

public class UserCreationRequest(string username, string email): 
    IRequest<Guid>
{
    public string? Username { get; } = username;
    public string? Email { get; } = email;
}

public class UserCreationHandler(
    IConsoleLoggerService logger): 
    IRequestHandler<IRequest<Guid>, Guid>
{
    
    public async Task<Guid> Handle(
        IRequest<Guid> request, 
        CancellationToken cancellationToken = default)
    {
        await logger.LogAsync(
            $"Creating user with username '{((UserCreationRequest)request).Username}' " +
            $"and email '{((UserCreationRequest)request).Email}'",
            SeverityEnu.Trace);
        
        return Guid.NewGuid();
    }

}
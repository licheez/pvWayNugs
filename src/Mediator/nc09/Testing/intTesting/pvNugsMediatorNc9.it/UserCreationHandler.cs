using pvNugsLoggerNc9Abstractions;
using pvNugsMediatorNc9Abstractions;

namespace pvNugsMediatorNc9.it;

public class UserCreationRequest(string username, string email): 
    IPvNugsMediatorRequest<Guid>
{
    public string? Username { get; } = username;
    public string? Email { get; } = email;
}

public class UserCreationHandler(
    IConsoleLoggerService logger): 
    IPvNugsMediatorRequestHandler<IPvNugsMediatorRequest<Guid>, Guid>
{
    
    public async Task<Guid> Handle(
        IPvNugsMediatorRequest<Guid> request, 
        CancellationToken cancellationToken = default)
    {
        await logger.LogAsync(
            $"Creating user with username '{((UserCreationRequest)request).Username}' " +
            $"and email '{((UserCreationRequest)request).Email}'",
            SeverityEnu.Trace);
        
        return Guid.NewGuid();
    }
    
}
using pvNugsMediatorNc9Abstractions.Mediator;

namespace pvNugsMediatorNc9.it.Mediator;

public class MdUserCreationRequest(string username, string email): 
    IRequest<Guid>
{
    public string? Username { get; } = username;
    public string? Email { get; } = email;
}
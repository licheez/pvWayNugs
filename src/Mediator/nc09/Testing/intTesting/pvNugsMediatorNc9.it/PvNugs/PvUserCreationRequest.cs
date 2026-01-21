using pvNugsMediatorNc9Abstractions.pvNugs;

namespace pvNugsMediatorNc9.it.PvNugs;

public class PvUserCreationRequest(string username, string email): 
    IPvNugsMediatorRequest<Guid>
{
    public string? Username { get; } = username;
    public string? Email { get; } = email;
}
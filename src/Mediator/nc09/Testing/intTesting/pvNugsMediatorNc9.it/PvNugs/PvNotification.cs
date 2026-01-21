using pvNugsMediatorNc9Abstractions.Mediator;

namespace pvNugsMediatorNc9.it.PvNugs;

public class PvNotification(string message) : INotification
{
    public string Message { get; } = message;
}
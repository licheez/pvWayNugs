using pvNugsMediatorNc9Abstractions.Mediator;

namespace pvNugsMediatorNc9.it.Mediator;

public class MdNotification(string message) : INotification
{
    public string Message { get; } = message;
}
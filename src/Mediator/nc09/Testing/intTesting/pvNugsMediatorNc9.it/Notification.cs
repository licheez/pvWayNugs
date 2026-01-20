using pvNugsMediatorNc9Abstractions.Mediator;

namespace pvNugsMediatorNc9.it;

public class Notification: INotification
{
    public string Message { get; }

    public Notification(string message)
    {
        Message = message;
    }
}
using pvNugsMediatorNc9Abstractions;

namespace pvNugsMediatorNc9.it;

public class Notification: IPvNugsMediatorNotification
{
    public string Message { get; }

    public Notification(string message)
    {
        Message = message;
    }
}
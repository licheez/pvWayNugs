namespace pvNugsMessagingNc10Kafka;

public class PvNugsMessagingException: Exception
{
    public PvNugsMessagingException(string message) : base(
        $"pvNugsMessagingException: {message}")
    {
    }

    public PvNugsMessagingException(Exception e) : base(
        $"pvNugsMessagingException: {e.Message}", e)
    {
    }
}
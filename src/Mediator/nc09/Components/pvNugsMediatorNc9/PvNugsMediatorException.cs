namespace pvNugsMediatorNc9;

public class PvNugsMediatorException: Exception
{
    public PvNugsMediatorException(string message) : 
        base($"pvNugsMediator Exception: {message}")
    {
    }

    public PvNugsMediatorException(Exception e):
        base($"pvNugsMediator Exception: {e.Message}", e)
    {
    }
    
}
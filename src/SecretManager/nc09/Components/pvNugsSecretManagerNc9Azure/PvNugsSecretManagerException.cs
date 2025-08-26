namespace pvNugsSecretManagerNc9Azure;

public class PvNugsSecretManagerException : 
    Exception
{
    public PvNugsSecretManagerException(string message):
        base($"PvNugsSecretManagerException: {message}")
    {
    }
    
    public PvNugsSecretManagerException(Exception e) : 
        base($"PvNugsSecretManagerException: {e.Message}", e)
    {
    }
}
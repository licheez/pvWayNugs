namespace pvNugsSecretManagerNc9Azure;

public class PvNugsStaticSecretManagerException : 
    Exception
{
    public PvNugsStaticSecretManagerException(string message):
        base($"PvNugsStaticSecretManagerException: {message}")
    {
    }
    
    public PvNugsStaticSecretManagerException(Exception e) : 
        base($"PvNugsStaticSecretManagerException: {e.Message}", e)
    {
    }
}
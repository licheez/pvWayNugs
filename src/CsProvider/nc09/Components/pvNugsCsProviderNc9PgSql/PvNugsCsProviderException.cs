namespace pvNugsCsProviderNc9PgSql;

public class PvNugsCsProviderException : 
    Exception
{
    public PvNugsCsProviderException(string message):
        base($"PvNugsCsProviderException: {message}")
    {
        
    }
    
    public PvNugsCsProviderException(Exception e) : 
        base($"PvNugsCsProviderException: {e.Message}", e)
    {
    }
}
namespace pvNugsCsProviderNc9PgSql;

public class PvNugsCsProviderPgSqlConfig
{
    public static string Section = nameof(PvNugsCsProviderPgSqlConfig);
    
    public CsProviderModeEnu Mode { get; set; }
    public string Server { get; set; } = null!;
    public string Schema { get; set; }= null!;
    public string Database { get; set; }= null!;
    public int? Port { get; set; }
    public string? Timezone { get; set; }
    public int? TimeoutInSeconds { get; set; }
    
    public string? Username { get; set; }
    public string? Password { get; set; }
    
    public string? SecretName { get; set; }
}
namespace pvNugsSecretManagerNc10;

public class PvNugsSecretManagerConfig
{
    public const string Section = nameof(PvNugsSecretManagerConfig);
    
    public string CacheKeyPrefix { get; set; } = "PvNugsSecretManagerNc10";
    public TimeSpan CacheTimeToLive { get; set; } = TimeSpan.FromDays(5);
}
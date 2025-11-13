namespace pvNugsCacheNc6Local;

public class PvNugsCacheConfig
{
    public const string Section = nameof(PvNugsCacheConfig);
    
    // ReSharper disable once PropertyCanBeMadeInitOnly.Global
    public TimeSpan? DefaultTimeToLive { get; set; }
}
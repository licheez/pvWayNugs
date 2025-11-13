namespace pvNugsCacheNc9Local;

/// <summary>
/// Configuration settings for the pvNugs in-memory cache service.
/// This class is typically bound from the application configuration (appsettings.json).
/// </summary>
public class PvNugsCacheConfig
{
    /// <summary>
    /// The configuration section name used to bind this configuration from appsettings.json.
    /// Default value is "PvNugsCacheConfig".
    /// </summary>
    public const string Section = nameof(PvNugsCacheConfig);
    
    /// <summary>
    /// Gets or sets the default time-to-live for cached items.
    /// If null, cached items will not expire automatically.
    /// If set, this value will be used as the default expiration time for items
    /// when no explicit time-to-live is specified during cache operations.
    /// </summary>
    // ReSharper disable once PropertyCanBeMadeInitOnly.Global
    public TimeSpan? DefaultTimeToLive { get; set; }
}
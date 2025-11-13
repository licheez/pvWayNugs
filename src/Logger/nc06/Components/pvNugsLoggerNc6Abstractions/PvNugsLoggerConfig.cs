

// ReSharper disable MemberCanBePrivate.Global

using pvNugsEnumConvNc6;

namespace pvNugsLoggerNc6Abstractions;

/// <summary>
/// Configuration class for PvNugs logging system that manages minimum log level settings.
/// <list type="bullets">
/// <item>
/// <c>MinLogLevel</c>(string) minimum log level. Supported formats: 
/// <list type="bullet">
/// <item>Trace: "trace", "t", "verbose", "verb", "vrb", "v"</item>
/// <item>Debug: "debug", "dbg", "d"</item>
/// <item> Info: "inf", "info", "information", "i"</item>
/// <item>Warning: "warning", "warn", "w"</item>
/// <item>Error: "error", "err", "e"</item>
/// <item>Fatal: "fatal", "ftl", "f", "critic", "critical", "c"</item>
/// </list>
/// </item>
/// </list>
/// </summary>
public class PvNugsLoggerConfig
{
    /// <summary>
    /// The configuration section name used for binding configuration values.
    /// </summary>
    public const string Section = nameof(PvNugsLoggerConfig);

    /// <summary>
    /// Gets or sets the minimum log level as a string.
    /// Valid values include variations of trace, debug, info, warning, error, and fatal.
    /// </summary>
    public string MinLogLevel { get; set; } = null!;

    /// <summary>
    /// Gets the parsed minimum severity level from the <see cref="MinLogLevel"/> string.
    /// Converts various string representations to their corresponding <see cref="SeverityEnu"/> value.
    /// If the input string doesn't match any known level, defaults to Trace.
    /// </summary>
    /// <remarks>
    /// Supported string formats:
    /// - Trace: "trace", "t", "verbose", "verb", "vrb", "v"
    /// - Debug: "debug", "dbg", "d"
    /// - Info: "inf", "info", "information", "i"
    /// - Warning: "warning", "warn", "w"
    /// - Error: "error", "err", "e"
    /// - Fatal: "fatal", "ftl" "f", "critic", "critical", "c"
    /// </remarks>
    public SeverityEnu MinLevel => MinLogLevel.ToLowerInvariant() switch
    {
        "trace" or "t" or "verbose" or "verb" or "vrb" or "v" => SeverityEnu.Trace,
        "debug" or "dbg" or "d" => SeverityEnu.Debug,
        "inf" or "info" or "information" or "i" => SeverityEnu.Info,
        "warning" or "warn" or "w" => SeverityEnu.Warning,
        "error" or "err" or "e" => SeverityEnu.Error,
        "fatal" or "ftl" or "f" or "critic" or "critical" or "c" => SeverityEnu.Fatal,
        _ => SeverityEnu.Trace
    };

    /// <summary>
    /// Initializes a new instance of the <see cref="PvNugsLoggerConfig"/> class.
    /// </summary>
    public PvNugsLoggerConfig()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PvNugsLoggerConfig"/>
    /// class with a specified minimum log level.
    /// </summary>
    /// <param name="minLogLevel">The minimum severity level for logging.</param>
    public PvNugsLoggerConfig(SeverityEnu minLogLevel)
    {
        MinLogLevel = minLogLevel.GetCode();
    }
}
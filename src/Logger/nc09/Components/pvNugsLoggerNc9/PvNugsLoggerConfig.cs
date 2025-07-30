using pvNugsEnumConvNc9;
using pvNugsLoggerNc9Abstractions;

namespace pvNugsLoggerNc9;

public class PvNugsLoggerConfig
{
    public const string Section = nameof(PvNugsLoggerConfig);

    public string MinLogLevel { get; set; } = null!;

    public SeverityEnu MinLevel => MinLogLevel.ToLowerInvariant() switch
    {
        "trace" or "t" or "verbose" or "verb" or "vrb" or "v" => SeverityEnu.Trace,
        "debug" or "dbg" or "d" => SeverityEnu.Debug,
        "inf" or "info" or "information" or "i" => SeverityEnu.Info,
        "warning" or "warn" or "w" => SeverityEnu.Warning,
        "error" or "err" or "e" => SeverityEnu.Error,
        "fatal" or "f" or "critic" or "critical" or "c" => SeverityEnu.Fatal,
        _ => SeverityEnu.Trace
    };

    public PvNugsLoggerConfig()
    {
    }

    public PvNugsLoggerConfig(SeverityEnu minLogLevel)
    {
        MinLogLevel = minLogLevel.GetCode();
    }
}
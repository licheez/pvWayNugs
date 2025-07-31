namespace pvNugsLoggerNc9Abstractions;

/// <summary>
/// Represents logging severity levels for the application.
/// </summary>
public enum SeverityEnu
{
    /// <summary>
    /// Indicates a normal, successful operation. Maps to LogLevel.None.
    /// </summary>
    [System.ComponentModel.Description("O")]
    Ok,
    
    /// <summary>
    /// Contains the most detailed messages. Maps to LogLevel.Trace.
    /// </summary>
    [System.ComponentModel.Description("T")]
    Trace,
    
    /// <summary>
    /// Contains information useful for debugging. Maps to LogLevel.Debug.
    /// </summary>
    [System.ComponentModel.Description("D")]
    Debug,
    
    /// <summary>
    /// Represents normal application flow messages. Maps to LogLevel.Information.
    /// </summary>
    [System.ComponentModel.Description("I")]
    Info,
    
    /// <summary>
    /// Highlights an abnormal or unexpected event. Maps to LogLevel.Warning.
    /// </summary>
    [System.ComponentModel.Description("W")]
    Warning,
    
    /// <summary>
    /// Indicates a failure in the application. Maps to LogLevel.Error.
    /// </summary>
    [System.ComponentModel.Description("E")]
    Error,
    
    /// <summary>
    /// Represents a critical error causing complete failure. Maps to LogLevel.Critical.
    /// </summary>
    [System.ComponentModel.Description("F")]
    Fatal
}
namespace pvNugsLoggerNc9Abstractions;

/// <summary>
/// Defines the available SQL database roles for accessing the logging system.
/// </summary>
public enum SqlRoleEnu
{
    /// <summary>
    /// Represents a role with full administrative privileges to the logging database.
    /// This role has complete control over log data and schema modifications.
    /// </summary>
    Owner,

    /// <summary>
    /// Represents a role with standard application-level access to the logging database.
    /// This role has permissions to write logs and perform routine operations.
    /// </summary>
    Application,

    /// <summary>
    /// Represents a role with read-only access to the logging database.
    /// This role can only view and query log entries without modification rights.
    /// </summary>
    Reader
}
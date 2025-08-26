namespace pvNugsCsProviderNc9Abstractions;

/// <summary>
/// Specifies the SQL role levels for database access control.
/// </summary>
public enum SqlRoleEnu
{
    /// <summary>
    /// Represents the highest privilege level with full database ownership and control capabilities.
    /// </summary>
    Owner,

    /// <summary>
    /// Represents application-level privileges, typically used for normal application operations
    /// with write access.
    /// </summary>
    Application,

    /// <summary>
    /// Represents read-only access privileges, typically used for query operations
    /// without modification capabilities.
    /// </summary>
    Reader
}
namespace pvNugsCsProviderNc9Abstractions;

/// <summary>
/// Defines a contract for providing Postgres-specific connection string functionality,
/// extending the base connection string provider with Postgres-specific properties.
/// </summary>
public interface IPvNugsPgSqlCsProvider : IPvNugsCsProvider
{
    /// <summary>
    /// Gets the SQL role associated with the current connection.
    /// </summary>
    SqlRoleEnu Role { get; }

    /// <summary>
    /// Gets a value indicating whether the provider uses dynamic credentials
    /// for database connections.
    /// </summary>
    bool UseDynamicCredentials { get; }

    /// <summary>
    /// Gets the username used for database authentication.
    /// </summary>
    string UserName { get; }

    /// <summary>
    /// Gets the Postgres schema name used for database operations.
    /// </summary>
    string Schema { get; }
}
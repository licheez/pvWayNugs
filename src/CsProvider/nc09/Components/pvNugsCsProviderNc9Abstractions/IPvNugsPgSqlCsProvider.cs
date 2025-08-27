namespace pvNugsCsProviderNc9Abstractions;

/// <summary>
/// Defines a contract for providing Postgres-specific connection string functionality,
/// extending the base connection string provider with Postgres-specific properties.
/// </summary>
public interface IPvNugsPgSqlCsProvider : IPvNugsCsProvider
{
    /// <summary>
    /// Gets a value indicating whether the provider uses dynamic credentials
    /// for database connections.
    /// </summary>
    bool UseDynamicCredentials { get; }

    /// <summary>
    /// Retrieves the username associated with the specified SQL role.
    /// </summary>
    /// <param name="role">The SQL role for which the username is requested.</param>
    /// <returns>The username associated with the specified SQL role.
    /// Returns an empty string if the role does not exist.</returns>
    string GetUsername(SqlRoleEnu role);

    /// <summary>
    /// Gets the Postgres schema name used for database operations.
    /// </summary>
    string Schema { get; }
}
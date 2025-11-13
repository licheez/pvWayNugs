namespace pvNugsCsProviderNc6Abstractions;

/// <summary>
/// Defines a contract for providing MsSQL-specific connection string functionality,
/// extending the base connection string provider with MsSql-specific properties.
/// </summary>
public interface IPvNugsMsSqlCsProvider: IPvNugsCsProvider
{
    /// <summary>
    /// Gets a value indicating whether the default database configuration uses Active Directory Trusted Connection.
    /// </summary>
    bool UseTrustedConnection { get; }

    /// <summary>
    /// Determines whether the specified database configuration uses Active Directory Trusted Connection.
    /// </summary>
    /// <param name="connectionStringName">The unique name of the database configuration to check.</param>
    /// <returns>True if the specified configuration uses trusted connection; otherwise, false.</returns>
    bool IsTrustedConnection(string connectionStringName);

    /// <summary>
    /// Gets a value indicating whether the default database configuration uses dynamic credentials.
    /// </summary>
    bool UseDynamicCredentials { get; }

    /// <summary>
    /// Determines whether the specified database configuration uses dynamic credentials.
    /// </summary>
    /// <param name="connectionStringName">The unique name of the database configuration to check.</param>
    /// <returns>True if the specified configuration uses dynamic credentials; otherwise, false.</returns>
    bool IsDynamicCredentials(string connectionStringName);

    /// <summary>
    /// Retrieves the username associated with the specified SQL role for the default database configuration.
    /// </summary>
    /// <param name="role">The SQL role for which the username is requested.</param>
    /// <returns>The username associated with the specified SQL role. Returns an empty string if the role does not exist.</returns>
    string GetUsername(SqlRoleEnu role);

    /// <summary>
    /// Retrieves the username associated with the specified SQL role and database configuration.
    /// </summary>
    /// <param name="connectionStringName">The unique name of the database configuration to use.</param>
    /// <param name="role">The SQL role for which the username is requested.</param>
    /// <returns>The username associated with the specified SQL role and database configuration. Returns an empty string if the role or configuration does not exist.</returns>
    string GetUsername(string connectionStringName, SqlRoleEnu role);
}
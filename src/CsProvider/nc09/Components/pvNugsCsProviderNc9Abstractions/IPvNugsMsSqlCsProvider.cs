namespace pvNugsCsProviderNc9Abstractions;

/// <summary>
/// Defines a contract for providing MsSQL-specific connection string functionality,
/// extending the base connection string provider with MsSql-specific properties.
/// </summary>
public interface IPvNugsMsSqlCsProvider: IPvNugsCsProvider
{
    /// <summary>
    /// Uses Active Directory Trusted Connection
    /// </summary>
    bool UseTrustedConnection { get; }
    
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
}
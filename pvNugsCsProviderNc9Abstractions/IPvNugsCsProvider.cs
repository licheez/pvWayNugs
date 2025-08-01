namespace pvNugsCsProviderNc9Abstractions;

/// <summary>
/// Defines a contract for providing connection strings with role-based access control.
/// </summary>
public interface IPvNugsCsProvider
{
    /// <summary>
    /// Asynchronously retrieves a connection string based on the specified SQL role.
    /// </summary>
    /// <param name="role">The SQL role that determines the access level for the connection string. 
    /// Defaults to SqlRoleEnu.Reader if not specified.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains 
    /// the connection string for the specified role.</returns>
    Task<string> GetConnectionStringAsync(SqlRoleEnu? role = SqlRoleEnu.Reader);
}
namespace pvNugsCsProviderNc9Abstractions;

/// <summary>
/// Defines a contract for providing connection strings with role-based access control and multi-database support.
/// </summary>
public interface IPvNugsCsProvider
{
    /// <summary>
    /// Asynchronously retrieves a connection string for a specific database and SQL role.
    /// </summary>
    /// <param name="connectionStringName">The unique name of the database configuration to use.</param>
    /// <param name="role">The SQL role that determines the access level for the connection string. Defaults to <see cref="SqlRoleEnu.Reader"/> if not specified.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the connection string for the specified database and role.</returns>
    /// <remarks>
    /// <para>This method supports multi-database scenarios. The <paramref name="connectionStringName"/> parameter should match the unique name of a configured database (e.g., "MainDb", "AuditDb").</para>
    /// <para>The <paramref name="role"/> parameter enables role-based access control, returning a connection string with the appropriate privileges (e.g., Owner, Application, Reader).</para>
    /// <para>If <paramref name="connectionStringName"/> is not found, an <see cref="ArgumentException"/> should be thrown by the implementation.</para>
    /// </remarks>
    Task<string> GetConnectionStringAsync(
        string connectionStringName,
        SqlRoleEnu role = SqlRoleEnu.Reader,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Asynchronously retrieves a connection string for a specific SQL role from the default database configuration.
    /// </summary>
    /// <param name="role">The SQL role that determines the access level for the connection string. Defaults to <see cref="SqlRoleEnu.Reader"/> if not specified.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the connection string for the default database and specified role.</returns>
    /// <remarks>
    /// <para>This overload is provided for backward compatibility. It retrieves the connection string from the default database configuration.</para>
    /// </remarks>
    Task<string> GetConnectionStringAsync(
        SqlRoleEnu role = SqlRoleEnu.Reader,
        CancellationToken cancellationToken = default);
}
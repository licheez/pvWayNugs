using pvNugsCsProviderNc9Abstractions;
using pvNugsLoggerNc9Abstractions;
using SqlRoleEnu = pvNugsCsProviderNc9Abstractions.SqlRoleEnu;

namespace pvNugsCsProviderNc9ByFunc;

/// <summary>
/// Provides a function-based implementation of <see cref="IPvNugsCsProvider"/> that delegates
/// connection string retrieval to a supplied async function.
/// </summary>
/// <param name="getCsAsync">The async function that retrieves connection strings based on SQL roles.
/// This function accepts a nullable SQL role and returns the corresponding connection string.</param>
internal class CsProvider(
    IConsoleLoggerService logger,
    Func<SqlRoleEnu?, Task<string>> getCsAsync) : IPvNugsCsProvider
{
    /// <inheritdoc/>
    public async Task<string> GetConnectionStringAsync(SqlRoleEnu? role = SqlRoleEnu.Reader)
    {
        try
        {
            return await getCsAsync(role);
        }
        catch (Exception e)
        {
            await logger.LogAsync(e);
            throw new PvNugsCsProviderException(e);
        }
    }
}
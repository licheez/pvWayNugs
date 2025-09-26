using pvNugsCsProviderNc9Abstractions;
using pvNugsLoggerNc9Abstractions;
using SqlRoleEnu = pvNugsCsProviderNc9Abstractions.SqlRoleEnu;

namespace pvNugsCsProviderNc9ByFunc;

/// <summary>
/// Provides a function-based implementation of <see cref="IPvNugsCsProvider"/> that delegates
/// connection string retrieval to a supplied async function.
/// </summary>
/// <remarks>
/// This implementation does not support multiple named database configurations. The <c>connectionStringName</c>
/// parameter is ignored and only a single logical database is supported.
/// </remarks>
internal class CsProvider(
    IConsoleLoggerService logger,
    Func<SqlRoleEnu, CancellationToken, Task<string>> getCsAsync) : IPvNugsCsProvider
{
    /// <summary>
    /// Secondary constructor supporting a function that takes the connectionStringName as first parameter.
    /// </summary>
    /// <param name="logger">The logger for error reporting.</param>
    /// <param name="getCsAsync">A function that takes a connectionStringName, role, and cancellation token, and returns a connection string.</param>
    public CsProvider(
        IConsoleLoggerService logger,
        Func<string, SqlRoleEnu, CancellationToken, Task<string>> getCsAsync)
        : this(logger, (role, cancellationToken) => 
            getCsAsync("Default", role, cancellationToken))
    {
        _getCsByNameAsync = getCsAsync;
    }

    private readonly Func<string, SqlRoleEnu, CancellationToken, Task<string>>? _getCsByNameAsync;

    /// <inheritdoc/>
    public async Task<string> GetConnectionStringAsync(
        SqlRoleEnu role = SqlRoleEnu.Reader,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await getCsAsync(role, cancellationToken);
        }
        catch (Exception e)
        {
            await logger.LogAsync(e);
            throw new PvNugsCsProviderException(e);
        }
    }

    /// <inheritdoc/>
    /// <remarks>
    /// The <paramref name="connectionStringName"/> parameter is ignored in this implementation.
    /// </remarks>
    public async Task<string> GetConnectionStringAsync(
        string connectionStringName,
        SqlRoleEnu role = SqlRoleEnu.Reader,
        CancellationToken cancellationToken = default)
    {
        if (_getCsByNameAsync is not null)
        {
            try
            {
                return await _getCsByNameAsync(connectionStringName, role, cancellationToken);
            }
            catch (Exception e)
            {
                await logger.LogAsync(e);
                throw new PvNugsCsProviderException(e);
            }
        }
        // Fallback to single-db logic
        try
        {
            return await getCsAsync(role, cancellationToken);
        }
        catch (Exception e)
        {
            await logger.LogAsync(e);
            throw new PvNugsCsProviderException(e);
        }
    }

}
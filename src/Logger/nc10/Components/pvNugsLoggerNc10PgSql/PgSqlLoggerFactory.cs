using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using pvNugsLoggerNc10Abstractions;

namespace pvNugsLoggerNc10PgSql;

/// <summary>
/// Creates PostgreSQL-backed logger instances for the Microsoft logging infrastructure.
/// </summary>
/// <remarks>
/// The factory resolves the effective minimum level from <see cref="PvNugsLoggerConfig"/>
/// and reuses the configured <see cref="IPgSqlLogWriter"/> for log persistence.
/// </remarks>
internal sealed class PgSqlLoggerFactory(
    IOptions<PvNugsLoggerConfig> options,
    IPgSqlLogWriter logWriter)
    : ILoggerFactory
{
    private readonly Lock _syncRoot = new();
    private readonly List<ILoggerProvider> _providers = [];
    private readonly IOptions<PvNugsLoggerConfig> _options =
        options ?? throw new ArgumentNullException(nameof(options));
    private readonly IPgSqlLogWriter _logWriter = 
        logWriter ?? throw new ArgumentNullException(nameof(logWriter));

    /// <summary>
    /// Disposes all registered providers tracked by this factory.
    /// </summary>
    public void Dispose()
    {
        lock (_syncRoot)
        {
            foreach (var provider in _providers)
            {
                provider.Dispose();
            }

            _providers.Clear();
        }
    }

    /// <summary>
    /// Creates a logger for the requested category.
    /// </summary>
    /// <param name="categoryName">The category associated with the logger.</param>
    /// <returns>A <see cref="PgSqlLoggerService"/> instance.</returns>
    public ILogger CreateLogger(string categoryName)
    {
        ArgumentException.ThrowIfNullOrEmpty(categoryName);

        return new PgSqlLoggerService(_options.Value.MinLevel, _logWriter);
    }

    /// <summary>
    /// Adds a provider to the internal provider list for lifecycle management.
    /// </summary>
    /// <param name="provider">The provider to add.</param>
    public void AddProvider(ILoggerProvider provider)
    {
        ArgumentNullException.ThrowIfNull(provider);

        lock (_syncRoot)
        {
            if (!_providers.Contains(provider))
            {
                _providers.Add(provider);
            }
        }
    }
}
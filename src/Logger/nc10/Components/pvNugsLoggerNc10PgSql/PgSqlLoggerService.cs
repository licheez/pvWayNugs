using Microsoft.Extensions.Options;
using pvNugsLoggerNc10Abstractions;

namespace pvNugsLoggerNc10PgSql;

/// <summary>
/// PostgreSQL logger service implementation backed by <see cref="PgSqlLogWriter"/>.
/// </summary>
/// <remarks>
/// This service bridges the generic logger abstraction and the PostgreSQL writer implementation.
/// Write operations are handled by <see cref="BaseLoggerService"/> and purge operations are delegated
/// to the injected <see cref="IPgSqlLogWriter"/>.
/// </remarks>
internal class PgSqlLoggerService(
    SeverityEnu minLevel,
    IPgSqlLogWriter logWriter)
    : BaseLoggerService(minLevel, logWriter), 
        IPgSqlLoggerService
{
    /// <summary>
    /// Creates a logger service using configured minimum level and PostgreSQL writer.
    /// </summary>
    /// <param name="options">Logger configuration options.</param>
    /// <param name="logWriter">PostgreSQL log writer implementation.</param>
    public PgSqlLoggerService(
        IOptions<PvNugsLoggerConfig> options,
        IPgSqlLogWriter logWriter)
        : this(options.Value.MinLevel, logWriter)
    {
    }

    /// <summary>
    /// Purges log rows using either the supplied retention policy or the writer defaults.
    /// </summary>
    /// <param name="retainDic">
    /// Optional retention dictionary keyed by <see cref="SeverityEnu"/>.
    /// When <see langword="null"/>, the writer applies its configured default retention periods.
    /// </param>
    /// <returns>The number of deleted rows across all purged severities.</returns>
    /// <remarks>
    /// This method delegates to
    /// <see cref="ISqlLogWriter.PurgeLogsAsync(IDictionary{SeverityEnu, TimeSpan}?, CancellationToken)"/>.
    /// </remarks>
    public async Task<int> PurgeLogsAsync(IDictionary<SeverityEnu, TimeSpan>? retainDic = null)
    {
        return await logWriter.PurgeLogsAsync(retainDic);
    }
}
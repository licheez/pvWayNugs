using pvNugsLoggerNc10Abstractions;

namespace pvNugsLoggerNc10PgSql;

internal class PgSqlLoggerService(
    SeverityEnu minLevel, params ILogWriter[] logWriters)
    : BaseLoggerService(minLevel, logWriters), IPgSqlLoggerService
{
    public Task<int> PurgeLogsAsync(IDictionary<SeverityEnu, TimeSpan>? retainDic = null)
    {
        throw new NotImplementedException();
    }
}
using Microsoft.Extensions.Options;
using Npgsql;
using NpgsqlTypes;
using pvNugsCsProviderNc10Abstractions;
using pvNugsEnumConvNc10;
using pvNugsLoggerNc10Abstractions;

namespace pvNugsLoggerNc10PgSql;

/// <summary>
/// Writes application logs to PostgreSQL and optionally creates and checks the log table.
/// </summary>
/// <remarks>
/// The CreateDateUtc column uses PostgreSQL timestamp without the time zone for compatibility
/// with the existing schema. Its values are UTC by convention. Callers should provide UTC
/// dates; unspecified DateTime values are also interpreted as UTC.
/// </remarks>
public sealed class PgSqlLogWriter : IPgSqlLogWriter
{
    private const string SqlVarChar = "character varying";
    private const string SqlChar = "character";
    private const string SqlUtcDateTime = "timestamp without time zone";

    private readonly IConsoleLoggerService? _logger;
    private readonly IPvNugsPgSqlCsProvider _csp;
    private readonly PvNugsPgSqlLogWriterConfig _config;
    private readonly SemaphoreSlim _initSemaphore = new(1, 1);
    private volatile bool _isInitialized;

    private readonly string _schemaName;
    private readonly string _tableName;
    private readonly string _userIdColumnName;
    private int _userIdLength = 50;
    private readonly string _companyIdColumnName;
    private int _companyIdLength = 128;
    private readonly string _topicColumnName;
    private int _topicLength = 128;
    private readonly string _machineNameColumnName;
    private int _machineNameLength = 128;
    private readonly string _severityCodeColumnName;
    private readonly string _contextColumnName;
    private int _contextLength = 4096;
    private readonly string _messageColumnName;
    private int _messageLength = 4096;
    private readonly string _createDateColumnName;

    /// <summary>Creates a writer that uses the console for diagnostic output.</summary>
    /// <param name="csp">Provider of role-specific PostgreSQL connection strings.</param>
    /// <param name="options">Log writer configuration.</param>
    public PgSqlLogWriter(
        IPvNugsPgSqlCsProvider csp,
        IOptions<PvNugsPgSqlLogWriterConfig> options)
    {
        _csp = csp ?? throw new ArgumentNullException(nameof(csp));
        _config = (options ?? throw new ArgumentNullException(nameof(options))).Value;
        _schemaName = csp.Schema;
        _tableName = _config.TableName;
        _userIdColumnName = _config.UserIdColumnName;
        _companyIdColumnName = _config.CompanyIdColumnName;
        _topicColumnName = _config.TopicColumnName;
        _machineNameColumnName = _config.MachineNameColumnName;
        _severityCodeColumnName = _config.SeverityCodeColumnName;
        _contextColumnName = _config.ContextColumnName;
        _messageColumnName = _config.MessageColumnName;
        _createDateColumnName = _config.CreateDateUtcColumnName;

        // Fail early for missing or invalid configured SQL identifiers.
        _ = QuoteIdentifier(_schemaName);
        _ = QuoteIdentifier(_tableName);
        _ = QuoteIdentifier(_userIdColumnName);
        _ = QuoteIdentifier(_companyIdColumnName);
        _ = QuoteIdentifier(_topicColumnName);
        _ = QuoteIdentifier(_machineNameColumnName);
        _ = QuoteIdentifier(_severityCodeColumnName);
        _ = QuoteIdentifier(_contextColumnName);
        _ = QuoteIdentifier(_messageColumnName);
        _ = QuoteIdentifier(_createDateColumnName);
    }

    /// <summary>Creates a writer that sends diagnostic output to a logger.</summary>
    /// <param name="csp">Provider of role-specific PostgreSQL connection strings.</param>
    /// <param name="options">Log writer configuration.</param>
    /// <param name="logger">Logger for diagnostic output.</param>
    public PgSqlLogWriter(
        IPvNugsPgSqlCsProvider csp,
        IOptions<PvNugsPgSqlLogWriterConfig> options,
        IConsoleLoggerService logger) : this(csp, options)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>Releases resources used to synchronize initialization.</summary>
    public void Dispose() => _initSemaphore.Dispose();

    /// <summary>Releases resources used to synchronize initialization.</summary>
    /// <returns>A completed value task.</returns>
    public ValueTask DisposeAsync()
    {
        Dispose();
        return ValueTask.CompletedTask;
    }

    /// <summary>Writes one log entry to the configured PostgreSQL table.</summary>
    /// <param name="userId">Optional user identifier.</param>
    /// <param name="companyId">Optional company identifier.</param>
    /// <param name="topic">Optional log topic.</param>
    /// <param name="severity">Log severity.</param>
    /// <param name="machineName">Machine name, or an empty value to use the current machine.</param>
    /// <param name="memberName">Calling member name.</param>
    /// <param name="filePath">source file path.</param>
    /// <param name="lineNumber">source line number.</param>
    /// <param name="message">Log message.</param>
    /// <param name="dateUtc">Log timestamp in UTC.</param>
    /// <param name="cancellationToken">Token used to cancel asynchronous operations.</param>
    /// <returns>A task representing the write operation.</returns>
    public async Task WriteLogAsync(
        string? userId, string? companyId,
        string? topic, SeverityEnu severity, string machineName,
        string memberName, string filePath, int lineNumber,
        string message, DateTime dateUtc,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(message))
        {
            await LogExceptionAsync(new ArgumentException(
                "Message cannot be null or empty", nameof(message)), cancellationToken);
            return;
        }

        if (lineNumber < 0)
        {
            await LogExceptionAsync(new ArgumentOutOfRangeException(
                nameof(lineNumber), "Line number cannot be negative"), cancellationToken);
            return;
        }

        memberName = string.IsNullOrWhiteSpace(memberName) ? "<unknown>" : memberName;
        filePath = string.IsNullOrWhiteSpace(filePath) ? "<unknown>" : filePath;
        machineName = string.IsNullOrEmpty(machineName)
            ? Environment.MachineName
            : machineName;

        try
        {
            await EnsureInitializedAsync(cancellationToken);
            var appCs = await _csp.GetConnectionStringAsync(
                _config.ConnectionStringName,
                CsProviderSqlRoleEnu.Application,
                cancellationToken);

            await using var cn = new NpgsqlConnection(appCs);
            await cn.OpenAsync(cancellationToken);

            var cmdText = $"INSERT INTO {QualifiedTableName} (" +
                          $"{QuoteIdentifier(_userIdColumnName)}, " +
                          $"{QuoteIdentifier(_companyIdColumnName)}, " +
                          $"{QuoteIdentifier(_severityCodeColumnName)}, " +
                          $"{QuoteIdentifier(_machineNameColumnName)}, " +
                          $"{QuoteIdentifier(_topicColumnName)}, " +
                          $"{QuoteIdentifier(_contextColumnName)}, " +
                          $"{QuoteIdentifier(_messageColumnName)}, " +
                          $"{QuoteIdentifier(_createDateColumnName)}) " +
                          "VALUES (@userId, @companyId, @severity, @machineName, " +
                          "@topic, @context, @message, @date)";

            await using var cmd = new NpgsqlCommand(cmdText, cn);
            cmd.Parameters.Add("@userId", NpgsqlDbType.Varchar).Value =
                (object?)TruncateString(userId, _userIdLength) ?? DBNull.Value;
            cmd.Parameters.Add("@companyId", NpgsqlDbType.Varchar).Value =
                (object?)TruncateString(companyId, _companyIdLength) ?? DBNull.Value;
            cmd.Parameters.Add("@severity", NpgsqlDbType.Char).Value = GetSeverityCode(severity);
            cmd.Parameters.Add("@machineName", NpgsqlDbType.Varchar).Value =
                TruncateString(machineName, _machineNameLength)!;
            cmd.Parameters.Add("@topic", NpgsqlDbType.Varchar).Value =
                (object?)TruncateString(topic, _topicLength) ?? DBNull.Value;
            cmd.Parameters.Add("@context", NpgsqlDbType.Varchar).Value =
                TruncateString($"{memberName} # {filePath} # {lineNumber}", _contextLength)!;
            cmd.Parameters.Add("@message", NpgsqlDbType.Varchar).Value =
                TruncateString(message, _messageLength)!;
            cmd.Parameters.Add("@date", NpgsqlDbType.Timestamp).Value =
                AsUtcWithoutTimeZone(dateUtc);

            await cmd.ExecuteNonQueryAsync(cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception e)
        {
            await LogExceptionAsync(e, cancellationToken);
        }
    }

    /// <summary>Synchronously writes one log entry.</summary>
    /// <inheritdoc cref="WriteLogAsync" path="/param[@name='userId']|/param[@name='companyId']|/param[@name='topic']|/param[@name='severity']|/param[@name='machineName']|/param[@name='memberName']|/param[@name='filePath']|/param[@name='lineNumber']|/param[@name='message']|/param[@name='dateUtc']" />
    public void WriteLog(
        string? userId, string? companyId,
        string? topic, SeverityEnu severity, string machineName,
        string memberName, string filePath, int lineNumber,
        string message, DateTime dateUtc)
    {
        WriteLogAsync(userId, companyId, topic, severity, machineName,
                memberName, filePath, lineNumber, message, dateUtc)
            .GetAwaiter().GetResult();
    }

    /// <summary>Deletes entries older than the retention period for each severity.</summary>
    /// <param name="retainDic">Optional retention periods; defaults to configured periods.</param>
    /// <param name="cancellationToken">Token used to cancel asynchronous operations.</param>
    /// <returns>The total number of deleted rows.</returns>
    public async Task<int> PurgeLogsAsync(
        IDictionary<SeverityEnu, TimeSpan>? retainDic = null,
        CancellationToken cancellationToken = default)
    {
        retainDic ??= new Dictionary<SeverityEnu, TimeSpan>
        {
            { SeverityEnu.Fatal, _config.DefaultRetentionPeriodForFatal },
            { SeverityEnu.Error, _config.DefaultRetentionPeriodForError },
            { SeverityEnu.Warning, _config.DefaultRetentionPeriodForWarning },
            { SeverityEnu.Info, _config.DefaultRetentionPeriodForInfo },
            { SeverityEnu.Debug, _config.DefaultRetentionPeriodForDebug },
            { SeverityEnu.Trace, _config.DefaultRetentionPeriodForTrace }
        };

        try
        {
            await EnsureInitializedAsync(cancellationToken);
            var cs = await _csp.GetConnectionStringAsync(
                _config.ConnectionStringName,
                CsProviderSqlRoleEnu.Application,
                cancellationToken);
            await using var cn = new NpgsqlConnection(cs);
            await cn.OpenAsync(cancellationToken);

            var cmdText = $"DELETE FROM {QualifiedTableName} " +
                          $"WHERE {QuoteIdentifier(_severityCodeColumnName)} = @severity " +
                          $"AND {QuoteIdentifier(_createDateColumnName)} < @cutoffDate";

            var totalRows = 0;
            foreach (var (severity, keep) in retainDic)
            {
                await using var cmd = new NpgsqlCommand(cmdText, cn);
                cmd.Parameters.Add("@severity", NpgsqlDbType.Char).Value =
                    GetSeverityCode(severity);
                cmd.Parameters.Add("@cutoffDate", NpgsqlDbType.Timestamp).Value =
                    AsUtcWithoutTimeZone(DateTime.UtcNow - keep);
                totalRows += await cmd.ExecuteNonQueryAsync(cancellationToken);
            }

            return totalRows;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception e)
        {
            await LogExceptionAsync(e, cancellationToken);
            throw new PgSqlLogWriterException(e);
        }
    }

    /// <summary>Gets the first character of the configured severity code.</summary>
    /// <param name="severity">Severity to encode.</param>
    /// <returns>A one-character code, or D if no code is available.</returns>
    private static string GetSeverityCode(SeverityEnu severity)
    {
        var code = severity.GetCode();
        return string.IsNullOrEmpty(code) ? "D" : code[..1];
    }

    /// <summary>Shortens a value to the column limit, using an ellipsis when possible.</summary>
    /// <param name="value">Value to shorten.</param>
    /// <param name="maxLength">Maximum number of characters allowed by the column.</param>
    /// <returns>The shortened value, or the original null or empty value.</returns>
    private static string? TruncateString(string? value, int maxLength)
    {
        if (string.IsNullOrEmpty(value)) return value;
        if (maxLength < 1) throw new ArgumentOutOfRangeException(nameof(maxLength));
        if (value.Length <= maxLength) return value;
        return maxLength > 3 ? value[..(maxLength - 3)] + "..." : value[..maxLength];
    }

    /// <summary>Converts an instant to a UTC wall-clock value for the existing timestamp column.</summary>
    /// <param name="value">UTC or unspecified value; a local value is converted to UTC.</param>
    /// <returns>A DateTime with the same UTC clock value and unspecified kind.</returns>
    private static DateTime AsUtcWithoutTimeZone(DateTime value)
    {
        var utc = value.Kind == DateTimeKind.Local ? value.ToUniversalTime() : value;
        return DateTime.SpecifyKind(utc, DateTimeKind.Unspecified);
    }

    /// <summary>Quotes an SQL identifier, including any embedded double quotes.</summary>
    /// <param name="identifier">Schema, table, column, or index name.</param>
    /// <returns>A safely delimited PostgreSQL identifier.</returns>
    private static string QuoteIdentifier(string identifier)
    {
        if (string.IsNullOrWhiteSpace(identifier) || identifier.IndexOf('\0') >= 0)
            throw new ArgumentException("SQL identifier cannot be empty or contain NUL.",
                nameof(identifier));
        return "\"" + identifier.Replace("\"", "\"\"") + "\"";
    }

    /// <summary>Gets the SQL-qualified name of the configured log table.</summary>
    private string QualifiedTableName =>
        $"{QuoteIdentifier(_schemaName)}.{QuoteIdentifier(_tableName)}";

    /// <summary>Reads the table columns and validates the expected schema.</summary>
    /// <param name="cancellationToken">Token used to cancel database operations.</param>
    /// <returns>A task representing the validation.</returns>
    private async Task CheckTable(CancellationToken cancellationToken)
    {
        var cs = await _csp.GetConnectionStringAsync(
            _config.ConnectionStringName, CsProviderSqlRoleEnu.Reader,
            cancellationToken);
        await using var cn = new NpgsqlConnection(cs);
        await cn.OpenAsync(cancellationToken);

        const string cmdText =
            "SELECT column_name, data_type, is_nullable, character_maximum_length " +
            "FROM information_schema.columns " +
            "WHERE table_schema = @schemaName AND table_name = @tableName";

        await using var cmd = new NpgsqlCommand(cmdText, cn);
        cmd.Parameters.Add("@schemaName", NpgsqlDbType.Varchar).Value = _schemaName;
        cmd.Parameters.Add("@tableName", NpgsqlDbType.Varchar).Value = _tableName;

        var columns = new Dictionary<string, ColumnInfo>(StringComparer.Ordinal);
        await using (var reader = await cmd.ExecuteReaderAsync(cancellationToken))
        {
            while (await reader.ReadAsync(cancellationToken))
            {
                var column = new ColumnInfo(reader);
                columns.Add(column.ColumnName, column);
            }
        }

        var errors = new List<string>();
        if (columns.Count == 0)
        {
            errors.Add($"table {_schemaName}.{_tableName} not found or not visible");
        }
        else
        {
            CheckColumn(errors, columns, _userIdColumnName, SqlVarChar, true,
                out _userIdLength);
            CheckColumn(errors, columns, _companyIdColumnName, SqlVarChar, true,
                out _companyIdLength);
            CheckColumn(errors, columns, _severityCodeColumnName, SqlChar, false,
                out var severityLength);
            if (severityLength != 0 && severityLength != 1)
                errors.Add($"'{_severityCodeColumnName}' must have length 1");
            CheckColumn(errors, columns, _machineNameColumnName, SqlVarChar, false,
                out _machineNameLength);
            CheckColumn(errors, columns, _topicColumnName, SqlVarChar, true,
                out _topicLength);
            CheckColumn(errors, columns, _contextColumnName, SqlVarChar, false,
                out _contextLength);
            CheckColumn(errors, columns, _messageColumnName, SqlVarChar, false,
                out _messageLength);
            CheckColumn(errors, columns, _createDateColumnName, SqlUtcDateTime, false,
                out _);
        }

        if (errors.Count != 0)
            throw new PgSqlLogWriterException(string.Join(Environment.NewLine, errors));
    }

    /// <summary>Validates a column and returns its character limit when applicable.</summary>
    /// <param name="errors">Collection receiving validation failures.</param>
    /// <param name="columns">Actual columns, indexed by name.</param>
    /// <param name="columnName">Expected column name.</param>
    /// <param name="expectedType">Expected PostgreSQL data type.</param>
    /// <param name="isNullable">Whether the column should allow nulls.</param>
    /// <param name="length">Actual character limit, or zero if unavailable.</param>
    private static void CheckColumn(
        List<string> errors, IDictionary<string, ColumnInfo> columns,
        string columnName, string expectedType, bool isNullable, out int length)
    {
        length = 0;
        if (!columns.TryGetValue(columnName, out var info))
        {
            errors.Add($"{columnName} not found in log table");
            return;
        }

        if (!string.Equals(info.Type, expectedType, StringComparison.OrdinalIgnoreCase))
        {
            errors.Add($"{columnName} expected type is {expectedType} " +
                       $"but actual type is {info.Type}");
            return;
        }

        length = info.Length ?? 0;
        if (expectedType != SqlUtcDateTime && length < 1)
            errors.Add($"{columnName} has no valid character length");
        if (isNullable != info.IsNullable)
            errors.Add($"{columnName} should {(isNullable ? "" : "not ")}be nullable");
    }

    /// <summary>Creates the log table and its indexes if the table is absent.</summary>
    /// <param name="cancellationToken">Token used to cancel database operations.</param>
    /// <returns>A task representing table initialization.</returns>
    private async Task CreateTableIfNotExistsAsync(CancellationToken cancellationToken)
    {
        await LogActivityAsync($"Checking table '{_tableName}' existence", cancellationToken);
        var cs = await _csp.GetConnectionStringAsync(
            _config.ConnectionStringName, CsProviderSqlRoleEnu.Reader,
            cancellationToken);
        await using var readerCn = new NpgsqlConnection(cs);
        await readerCn.OpenAsync(cancellationToken);

        const string existsSql = "SELECT 1 FROM information_schema.tables " +
                                 "WHERE table_schema = @schemaName " +
                                 "AND table_name = @tableName";
        await using var existsCmd = new NpgsqlCommand(existsSql, readerCn);
        existsCmd.Parameters.Add("@schemaName", NpgsqlDbType.Varchar).Value = _schemaName;
        existsCmd.Parameters.Add("@tableName", NpgsqlDbType.Varchar).Value = _tableName;
        if (await existsCmd.ExecuteScalarAsync(cancellationToken) != null) return;

        try
        {
            var ownerCs = await _csp.GetConnectionStringAsync(
                _config.ConnectionStringName, CsProviderSqlRoleEnu.Owner,
                cancellationToken);
            await using var ownerCn = new NpgsqlConnection(ownerCs);
            await ownerCn.OpenAsync(cancellationToken);
            await CreateTableAsync(ownerCn, cancellationToken);
            await CreateIndexesAsync(ownerCn, cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception e)
        {
            await LogExceptionAsync(e, cancellationToken);
            throw new PgSqlLogWriterException(e);
        }
    }

    /// <summary>Runs the optional table creation and validation once per writer instance.</summary>
    /// <param name="cancellationToken">Token used to cancel initialization.</param>
    /// <returns>A task representing initialization.</returns>
    private async Task EnsureInitializedAsync(CancellationToken cancellationToken)
    {
        if (_isInitialized) return;
        await _initSemaphore.WaitAsync(cancellationToken);
        try
        {
            if (_isInitialized) return;
            if (_config.CreateTableAtFirstUse)
                await CreateTableIfNotExistsAsync(cancellationToken);
            if (_config.CheckTableAtFirstUse)
                await CheckTable(cancellationToken);
            _isInitialized = true;
        }
        finally
        {
            _initSemaphore.Release();
        }
    }

    /// <summary>Creates the schema when permitted and creates the log table.</summary>
    /// <param name="cn">Open owner connection.</param>
    /// <param name="cancellationToken">Token used to cancel database operations.</param>
    /// <returns>A task representing table creation.</returns>
    private async Task CreateTableAsync(NpgsqlConnection cn, CancellationToken cancellationToken)
    {
        await LogActivityAsync($"Creating schema {_schemaName} if needed", cancellationToken);
        await using var schemaCmd = cn.CreateCommand();
        schemaCmd.CommandText = $"CREATE SCHEMA IF NOT EXISTS {QuoteIdentifier(_schemaName)}";
        await schemaCmd.ExecuteNonQueryAsync(cancellationToken);

        await LogActivityAsync($"Creating table {_schemaName}.{_tableName}", cancellationToken);
        var createSql = $"CREATE TABLE IF NOT EXISTS {QualifiedTableName} (" +
                        "\"Id\" integer GENERATED BY DEFAULT AS IDENTITY, " +
                        $"{QuoteIdentifier(_userIdColumnName)} character varying(50) NULL, " +
                        $"{QuoteIdentifier(_companyIdColumnName)} character varying(128) NULL, " +
                        $"{QuoteIdentifier(_severityCodeColumnName)} character(1) NOT NULL, " +
                        $"{QuoteIdentifier(_machineNameColumnName)} character varying(128) NOT NULL, " +
                        $"{QuoteIdentifier(_topicColumnName)} character varying(128) NULL, " +
                        $"{QuoteIdentifier(_contextColumnName)} character varying(4096) NOT NULL, " +
                        $"{QuoteIdentifier(_messageColumnName)} character varying(4096) NOT NULL, " +
                        $"{QuoteIdentifier(_createDateColumnName)} timestamp without time zone NOT NULL)";
        await using var tableCmd = cn.CreateCommand();
        tableCmd.CommandText = createSql;
        await tableCmd.ExecuteNonQueryAsync(cancellationToken);
    }

    /// <summary>Creates the configured indexes on a new log table.</summary>
    /// <param name="connection">Open owner connection.</param>
    /// <param name="cancellationToken">Token used to cancel database operations.</param>
    /// <returns>A task representing index creation.</returns>
    private async Task CreateIndexesAsync(
        NpgsqlConnection connection, CancellationToken cancellationToken)
    {
        var indexes = new List<string>();
        if (_config.IncludeDateIndex)
            indexes.Add($"CREATE INDEX IF NOT EXISTS " +
                        $"{QuoteIdentifier($"IX_{_tableName}_{_createDateColumnName}")} " +
                        $"ON {QualifiedTableName} ({QuoteIdentifier(_createDateColumnName)} DESC)");

        if (_config.IncludePurgeIndex)
            indexes.Add($"CREATE INDEX IF NOT EXISTS {QuoteIdentifier($"IX_{_tableName}_Purge")} " +
                        $"ON {QualifiedTableName} " +
                        $"({QuoteIdentifier(_severityCodeColumnName)}, {QuoteIdentifier(_createDateColumnName)})");

        if (_config.IncludeUserIndex)
            indexes.Add($"CREATE INDEX IF NOT EXISTS {QuoteIdentifier($"IX_{_tableName}_User")} " +
                        $"ON {QualifiedTableName} " +
                        $"({QuoteIdentifier(_userIdColumnName)}, {QuoteIdentifier(_createDateColumnName)} DESC) " +
                        $"WHERE {QuoteIdentifier(_userIdColumnName)} IS NOT NULL");

        if (_config.IncludeTopicIndex)
            indexes.Add($"CREATE INDEX IF NOT EXISTS {QuoteIdentifier($"IX_{_tableName}_Topic")} " +
                        $"ON {QualifiedTableName} " +
                        $"({QuoteIdentifier(_topicColumnName)}, {QuoteIdentifier(_createDateColumnName)} DESC) " +
                        $"WHERE {QuoteIdentifier(_topicColumnName)} IS NOT NULL");

        foreach (var sql in indexes)
        {
            await using var cmd = connection.CreateCommand();
            cmd.CommandText = sql;
            await cmd.ExecuteNonQueryAsync(cancellationToken);
        }
    }

    /// <summary>Reports an initialization activity to the diagnostic logger.</summary>
    /// <param name="activity">Description of the activity.</param>
    /// <param name="cancellationToken">Token used to cancel logging.</param>
    /// <returns>A task representing the diagnostic writing.</returns>
    private async Task LogActivityAsync(string activity, CancellationToken cancellationToken)
    {
        if (_logger == null) Console.WriteLine(activity);
        else await _logger.LogAsync(activity, SeverityEnu.Trace, cancellationToken);
    }

    /// <summary>Reports an exception to the diagnostic logger.</summary>
    /// <param name="exception">Exception to report.</param>
    /// <param name="cancellationToken">Token used to cancel logging.</param>
    /// <returns>A task representing the diagnostic writing.</returns>
    private async Task LogExceptionAsync(Exception exception, CancellationToken cancellationToken)
    {
        if (_logger == null) Console.WriteLine(exception);
        else await _logger.LogAsync(exception, SeverityEnu.Fatal, cancellationToken);
    }
}
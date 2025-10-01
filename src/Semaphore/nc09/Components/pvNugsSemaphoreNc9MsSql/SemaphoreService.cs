using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;
using pvNugsCsProviderNc9Abstractions;
using pvNugsLoggerNc9Abstractions;
using pvNugsSemaphoreNc9Abstractions;
using SqlRoleEnu = pvNugsCsProviderNc9Abstractions.SqlRoleEnu;

namespace pvNugsSemaphoreNc9MsSql;

/// <summary>
/// Provides a distributed semaphore (mutex) implementation using Microsoft SQL Server as the backend.
/// This service leverages atomic database operations to coordinate exclusive access to named resources
/// across distributed systems or within a single process.
/// </summary>
internal sealed class SemaphoreService(
    ILoggerService logger,
    IPvNugsMsSqlCsProvider csp,
    IOptions<PvNugsMsSqlSemaphoreConfig> options) : IPvNugsSemaphoreService
{
    private const string SqlVarChar = "varchar";
    private const string SqlInt = "int";
    private const string SqlDateTime = "datetime";

    private const string NameField = "Name";
    private const string OwnerField = "Owner";
    private const string TimeoutInSecondsField = "TimeoutInSeconds";
    private const string CreateDateUtcField = "CreateDateUtc";
    private const string UpdateDateUtcField = "UpdateDateUtc";

    private readonly PvNugsMsSqlSemaphoreConfig _config = options.Value;
    private readonly string _connectionStringName = options.Value.ConnectionStringName;
    private readonly string _tableName = options.Value.TableName;
    private readonly string _schemaName = options.Value.SchemaName;

    private readonly SemaphoreSlim _initSemaphore = new(1, 1);
    private bool _isInitialized;

    /// <summary>
    /// Attempts to acquire a named semaphore for the specified requester and timeout period.
    /// If the semaphore is already held and not expired, returns its current status and owner.
    /// If the semaphore is expired, attempts to atomically steal the lock and returns <see cref="SemaphoreStatusEnu.ForcedAcquired"/>.
    /// </summary>
    /// <param name="name">The unique name of the semaphore (mutex).</param>
    /// <param name="requester">The identifier of the process or entity requesting the semaphore.</param>
    /// <param name="timeout">The duration for which the lock should remain valid.</param>
    /// <param name="ct">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>Information about the semaphore, including its status and ownership. Status can be <see cref="SemaphoreStatusEnu.Acquired"/>, <see cref="SemaphoreStatusEnu.OwnedBySomeoneElse"/>, or <see cref="SemaphoreStatusEnu.ForcedAcquired"/>.</returns>
    public async Task<IPvNugsSemaphoreInfo> AcquireSemaphoreAsync(
        string name, string requester, TimeSpan timeout, CancellationToken ct = default)
    {
        await EnsureInitializedAsync();

        var now = DateTime.UtcNow;
        var expiresAt = now + timeout;
        var cs = await csp.GetConnectionStringAsync(
            _connectionStringName, SqlRoleEnu.Application, ct);
        await using var cn = new SqlConnection(cs);
        await cn.OpenAsync(ct);
        await using var tx = await cn.BeginTransactionAsync(ct);
        try
        {
            // Try to insert (acquire lock)
            var insertCmd = cn.CreateCommand();
            insertCmd.Transaction = (SqlTransaction)tx;
            insertCmd.CommandText = $"INSERT INTO [{_schemaName}].[{_tableName}] (" +
                                    $"[{NameField}], " +
                                    $"[{OwnerField}], " +
                                    $"[{TimeoutInSecondsField}], " +
                                    $"[{CreateDateUtcField}], " +
                                    $"[{UpdateDateUtcField}]) " +
                                    "VALUES (" +
                                    "@Name, @Owner, @TimeoutInSeconds, " +
                                    "@CreateDateUtc, @UpdateDateUtc )";
            insertCmd.Parameters.Add("@Name", SqlDbType.VarChar, 128).Value = name;
            insertCmd.Parameters.Add("@Owner", SqlDbType.VarChar, 128).Value = requester;
            insertCmd.Parameters.Add("@TimeoutInSeconds", SqlDbType.Int).Value = (int)timeout.TotalSeconds;
            insertCmd.Parameters.Add("@CreateDateUtc", SqlDbType.DateTime).Value = now;
            insertCmd.Parameters.Add("@UpdateDateUtc", SqlDbType.DateTime).Value = now;
            try
            {
                await insertCmd.ExecuteNonQueryAsync(ct);
                await tx.CommitAsync(ct);
                return new SemaphoreInfo(
                    SemaphoreStatusEnu.Acquired, name,
                    requester, timeout, expiresAt,
                    now, now);
            }
            catch (Exception ex) when (ex is SqlException { Number: 2627 })
            {
                // Already exists, check if expired
                var selectCmd = cn.CreateCommand();
                selectCmd.Transaction = (SqlTransaction)tx;
                selectCmd.CommandText = $"SELECT " +
                                        $"[{OwnerField}], " +
                                        $"[{TimeoutInSecondsField}], " +
                                        $"[{UpdateDateUtcField}], " +
                                        $"[{CreateDateUtcField}] " +
                                        $"FROM " +
                                        $"[{_schemaName}].[{_tableName}] " +
                                        $"WHERE [{NameField}] = @Name ";
                selectCmd.Parameters.Add("@Name", SqlDbType.VarChar, 128).Value = name;
                await using var reader = await selectCmd.ExecuteReaderAsync(ct);
                if (!await reader.ReadAsync(ct))
                {
                    // Row disappeared, try again
                    await tx.RollbackAsync(ct);
                    return await AcquireSemaphoreAsync(name, requester, timeout, ct);
                }

                var rowOwner = reader.GetString(0);
                var rowTimeoutInSeconds = reader.GetInt32(1);
                var rowUpdateDateUtc = reader.GetDateTime(2);
                var rowCreateDateUtc = reader.GetDateTime(3);
                await reader.CloseAsync();
                var rowTimeout = TimeSpan.FromSeconds(rowTimeoutInSeconds);
                var rowExpiresAtUtc = rowUpdateDateUtc + rowTimeout;
                var rowExpired = rowExpiresAtUtc < now;

                if (rowExpired)
                {
                    // Try to steal (atomic update)
                    var updateCmd = cn.CreateCommand();
                    updateCmd.Transaction = (SqlTransaction)tx;
                    updateCmd.CommandText = $"UPDATE [{_schemaName}].[{_tableName}] " +
                                            $"SET " +
                                            $"[{OwnerField}]=@Owner, " +
                                            $"[{TimeoutInSecondsField}]=@TimeoutInSeconds, " +
                                            $"[{CreateDateUtcField}]=@CreateDateUtc, " +
                                            $"[{UpdateDateUtcField}]=@UpdateDateUtc " +
                                            $"WHERE [{NameField}]=@Name " +
                                            $"AND [{UpdateDateUtcField}]=@OldUpdateDateUtc";
                    updateCmd.Parameters.Add("@Owner", SqlDbType.VarChar, 128).Value = requester;
                    updateCmd.Parameters.Add("@TimeoutInSeconds", SqlDbType.Int).Value = (int)timeout.TotalSeconds;
                    updateCmd.Parameters.Add("@CreateDateUtc", SqlDbType.DateTime).Value = now;
                    updateCmd.Parameters.Add("@UpdateDateUtc", SqlDbType.DateTime).Value = now;
                    updateCmd.Parameters.Add("@Name", SqlDbType.VarChar, 128).Value = name;
                    updateCmd.Parameters.Add("@OldUpdateDateUtc", SqlDbType.DateTime).Value = rowUpdateDateUtc;
                    var rows = await updateCmd.ExecuteNonQueryAsync(ct);
                    if (rows == 1)
                    {
                        await tx.CommitAsync(ct);
                        return new SemaphoreInfo(
                            SemaphoreStatusEnu.ForcedAcquired,
                            name, requester, timeout,
                            expiresAt, now, now);
                    }

                    // Someone else updated, try again
                    await tx.RollbackAsync(ct);
                    return await AcquireSemaphoreAsync(name, requester, timeout, ct);
                }

                // Not expired, owned by someone else
                await tx.CommitAsync(ct);
                return new SemaphoreInfo(
                    SemaphoreStatusEnu.OwnedBySomeoneElse, name, rowOwner,
                    timeout, rowExpiresAtUtc, rowCreateDateUtc, rowUpdateDateUtc);
            }
        }
        catch (Exception ex)
        {
            await tx.RollbackAsync(ct);
            await logger.LogAsync(ex);
            throw new PvNugsMsSqlSemaphoreException(ex);
        }
    }

    /// <summary>
    /// Updates the last activity timestamp of a semaphore, extending its validity period.
    /// </summary>
    /// <param name="name">The name of the semaphore to touch.</param>
    /// <param name="ct">A cancellation token to observe while waiting for the task to complete.</param>
    public async Task TouchSemaphoreAsync(
        string name, CancellationToken ct = default)
    {
        await EnsureInitializedAsync();

        try
        {
            var now = DateTime.UtcNow;
            var cs = await csp.GetConnectionStringAsync(
                _connectionStringName, SqlRoleEnu.Application, ct);
            await using var cn = new SqlConnection(cs);
            await cn.OpenAsync(ct);
            var cmd = cn.CreateCommand();
            cmd.CommandText = $"UPDATE [{_schemaName}].[{_tableName}] " +
                              $"SET [{UpdateDateUtcField}]=@UpdateDateUtc " +
                              $"WHERE [{NameField}]=@Name";
            cmd.Parameters.Add("@UpdateDateUtc", SqlDbType.DateTime).Value = now;
            cmd.Parameters.Add("@Name", SqlDbType.VarChar, 128).Value = name;
            await cmd.ExecuteNonQueryAsync(ct);
        }
        catch (Exception e)
        {
            await logger.LogAsync(e);
            throw new PvNugsMsSqlSemaphoreException(e);
        }
    }

    /// <summary>
    /// Releases a previously acquired semaphore, making it available for other requesters.
    /// </summary>
    /// <param name="name">The name of the semaphore to release.</param>
    /// <param name="ct">A cancellation token to observe while waiting for the task to complete.</param>
    public async Task ReleaseSemaphoreAsync(string name, CancellationToken ct = default)
    {
        await EnsureInitializedAsync();

        try
        {
            var cs = await csp.GetConnectionStringAsync(
                _connectionStringName, SqlRoleEnu.Application, ct);
            await using var cn = new SqlConnection(cs);
            await cn.OpenAsync(ct);
            var cmd = cn.CreateCommand();
            cmd.CommandText = $"DELETE FROM [{_schemaName}].[{_tableName}] " +
                              $"WHERE [{NameField}]=@Name ";
            cmd.Parameters.Add("@Name", SqlDbType.VarChar, 128).Value = name;
            await cmd.ExecuteNonQueryAsync(ct);
        }
        catch (Exception e)
        {
            await logger.LogAsync(e);
            throw new PvNugsMsSqlSemaphoreException(e);
        }
    }

    /// <summary>
    /// Retrieves information about a specific semaphore for a given requester.
    /// </summary>
    /// <param name="name">The name of the semaphore.</param>
    /// <param name="requester">The identifier of the requester.</param>
    /// <param name="ct">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>Information about the semaphore if found; otherwise, null.</returns>
    public async Task<IPvNugsSemaphoreInfo?> GetSemaphoreAsync(string name, string requester,
        CancellationToken ct = default)
    {
        try
        {
            await EnsureInitializedAsync();

            var cs = await csp.GetConnectionStringAsync(
                _connectionStringName, SqlRoleEnu.Reader, ct);
            await using var cn = new SqlConnection(cs);
            await cn.OpenAsync(ct);
            var cmd = cn.CreateCommand();
            cmd.CommandText = $"SELECT " +
                              $"[{OwnerField}], " +
                              $"[{TimeoutInSecondsField}], " +
                              $"[{UpdateDateUtcField}], " +
                              $"[{CreateDateUtcField}] " +
                              $"FROM " +
                              $"[{_schemaName}].[{_tableName}] " +
                              $"WHERE [{NameField}] = @Name ";
            cmd.Parameters.Add("@Name", SqlDbType.VarChar, 128).Value = name;
            await using var reader = await cmd.ExecuteReaderAsync(ct);
            if (!await reader.ReadAsync(ct)) return null;
        
            var rowOwner = reader.GetString(0);
            var rowTimeoutInSeconds = reader.GetInt32(1);
            var rowUpdateDateUtc = reader.GetDateTime(2);
            var rowCreateDateUtc = reader.GetDateTime(3);
            await reader.CloseAsync();
            var rowTimeout = TimeSpan.FromSeconds(rowTimeoutInSeconds);
            var rowExpiresAtUtc = rowUpdateDateUtc + rowTimeout;

            var status = string.Compare(rowOwner,requester, StringComparison.InvariantCultureIgnoreCase) == 0 
                ? SemaphoreStatusEnu.Acquired : SemaphoreStatusEnu.OwnedBySomeoneElse;
            return new SemaphoreInfo(
                status, name, rowOwner, rowTimeout, rowExpiresAtUtc, rowCreateDateUtc, rowUpdateDateUtc);
        }
        catch (Exception e)
        {
            await logger.LogAsync(e);
            throw new PvNugsMsSqlSemaphoreException(e);
        }
    }

    /// <summary>
    /// Executes a function in a semaphore-protected context, ensuring exclusive access for the duration of the work.
    /// Retries acquisition if the semaphore is currently held by another requester.
    /// </summary>
    /// <typeparam name="T">The return type of the asynchronous work function.</typeparam>
    /// <param name="semaphoreName">The name of the semaphore (mutex) to acquire.</param>
    /// <param name="requester">The identifier of the requester (e.g., machine name).</param>
    /// <param name="timeout">The validity period for the lock.</param>
    /// <param name="workAsync">The asynchronous function to execute within the protected context.</param>
    /// <param name="notify">Optional callback for status or sleep notifications.</param>
    /// <param name="sleepBetweenAttemptsInSeconds">Seconds to wait between acquisition attempts if the semaphore is unavailable.</param>
    /// <param name="ct">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>The result of the executed work function.</returns>
    public async Task<T> IsolateWorkAsync<T>(string semaphoreName, string requester, TimeSpan timeout,
        Func<Task<T>> workAsync,
        Action<string>? notify = null, int sleepBetweenAttemptsInSeconds = 15, CancellationToken ct = default)
    {
        await EnsureInitializedAsync();
        
        while (true)
        {
            var info = await AcquireSemaphoreAsync(semaphoreName, requester, timeout, ct);
            if (info.Status is SemaphoreStatusEnu.Acquired or SemaphoreStatusEnu.ForcedAcquired)
            {
                try
                {
                    var result = await workAsync();
                    return result;
                }
                finally
                {
                    await ReleaseSemaphoreAsync(semaphoreName, ct);
                }
            }

            notify?.Invoke(
                $"Semaphore '{semaphoreName}' is owned by '{info.Owner}'. Retrying in {sleepBetweenAttemptsInSeconds}s...");
            await Task.Delay(TimeSpan.FromSeconds(sleepBetweenAttemptsInSeconds), ct);
        }
    }

    /// <summary>
    /// Executes an asynchronous action in a semaphore-protected context, ensuring exclusive access for the duration of the work.
    /// Retries acquisition if the semaphore is currently held by another requester.
    /// </summary>
    /// <param name="semaphoreName">The name of the semaphore (mutex) to acquire.</param>
    /// <param name="requester">The identifier of the requester (e.g., machine name).</param>
    /// <param name="timeout">The validity period for the lock.</param>
    /// <param name="workAsync">The asynchronous action to execute within the protected context.</param>
    /// <param name="notify">Optional callback for status or sleep notifications.</param>
    /// <param name="sleepBetweenAttemptsInSeconds">Seconds to wait between acquisition attempts if the semaphore is unavailable.</param>
    /// <param name="ct">A cancellation token to observe while waiting for the task to complete.</param>
    public async Task IsolateWorkAsync(string semaphoreName, string requester, TimeSpan timeout, Func<Task> workAsync,
        Action<string>? notify = null,
        int sleepBetweenAttemptsInSeconds = 15, CancellationToken ct = default)
    {
        await IsolateWorkAsync<object?>(semaphoreName, requester, timeout, async () =>
        {
            await workAsync();
            return null;
        }, notify, sleepBetweenAttemptsInSeconds, ct);
    }

    /// <summary>
    /// Ensures the semaphore table exists in the database, creating it if necessary.
    /// This method is thread-safe and only runs once per service lifetime.
    /// </summary>
    private async Task EnsureInitializedAsync()
    {
        if (_isInitialized) return;

        await _initSemaphore.WaitAsync();
        try
        {
            if (_isInitialized) return; // Double-check pattern

            if (!_config.CreateTableAtFirstUse)
            {
                _isInitialized = true;
                return;
            }

            if (_config.CreateTableAtFirstUse)
                await CreateTableIfNotExistsAsync();

            _isInitialized = true;
        }
        finally
        {
            _initSemaphore.Release();
        }
    }

    /// <summary>
    /// Checks for the existence of the semaphore table and creates it if it does not exist.
    /// </summary>
    private async Task CreateTableIfNotExistsAsync()
    {
        try
        {
            await logger.LogAsync($"Checking table '{_tableName}' existence", SeverityEnu.Trace);
            var readerCs =
                await csp.GetConnectionStringAsync(_connectionStringName);
            await using var readerCn = new SqlConnection(readerCs);
            await readerCn.OpenAsync();
            // Use parameters for table/schema names in queries where possible
            const string existsCommandText = "SELECT 1 FROM sys.tables t " +
                                             "INNER JOIN sys.schemas s ON t.schema_id = s.schema_id " +
                                             "WHERE t.name = @tableName AND s.name = @schemaName";

            await using var cmd = new SqlCommand(existsCommandText, readerCn);
            cmd.Parameters.Add("@tableName", SqlDbType.NVarChar, 128).Value = _tableName;
            cmd.Parameters.Add("@schemaName", SqlDbType.NVarChar, 128).Value = _schemaName;

            var tableExists = await cmd.ExecuteScalarAsync() != null;

            await readerCn.CloseAsync();

            if (tableExists) return;
        }
        catch (Exception e)
        {
            await logger.LogAsync(e);
        }

        try
        {
            await logger.LogAsync($"creating table {_schemaName}.{_tableName}", SeverityEnu.Info);

            var ownerCs = await csp.GetConnectionStringAsync(
                _connectionStringName, SqlRoleEnu.Owner);
            await using var ownerCn = new SqlConnection(ownerCs);
            await ownerCn.OpenAsync();

            // Create the table
            await CreateTableAsync(ownerCn);
        }
        catch (Exception e)
        {
            await logger.LogAsync(e);
            throw new PvNugsMsSqlSemaphoreException(e);
        }
    }

    /// <summary>
    /// Creates the semaphore table structure in the database.
    /// </summary>
    /// <param name="connection">The SQL connection to use for table creation.</param>
    private async Task CreateTableAsync(SqlConnection connection)
    {
        // Note: Table/column names can't be parameterized in DDL, but they come from config, not user input
        var createCommandText =
            $"CREATE TABLE [{_schemaName}].[{_tableName}] " +
            $"(" +
            $"[{NameField}] {SqlVarChar}(128) NOT NULL PRIMARY KEY, " +
            $"[{OwnerField}] {SqlVarChar}(128) NOT NULL, " +
            $"[{TimeoutInSecondsField}] {SqlInt} NOT NULL, " +
            $"[{CreateDateUtcField}] {SqlDateTime} NOT NULL, " +
            $"[{UpdateDateUtcField}] {SqlDateTime} NOT NULL " +
            $")";

        await using var createCmd = new SqlCommand(createCommandText, connection);
        await createCmd.ExecuteNonQueryAsync();

        await logger.LogAsync(
            $"Created table structure for {_schemaName}.{_tableName}", SeverityEnu.Info);
    }
}


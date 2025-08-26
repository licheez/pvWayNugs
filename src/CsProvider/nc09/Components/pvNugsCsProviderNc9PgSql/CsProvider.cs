using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Options;
using pvNugsCsProviderNc9Abstractions;
using pvNugsLoggerNc9Abstractions;
using pvNugsSecretManagerNc9Abstractions;
using SqlRoleEnu = pvNugsCsProviderNc9Abstractions.SqlRoleEnu;

namespace pvNugsCsProviderNc9PgSql;

public class CsProvider(
    IConsoleLoggerService logger,
    IOptions<PvNugsCsProviderPgSqlConfig> options) : IPvNugsPgSqlCsProvider
{
    private sealed class CsEntry(
        string username,
        string connectionString,
        DateTime? expirationDateUtc)
    {
        public string UserName { get; } = username;
        public string ConnectionString { get; } = connectionString;
        private DateTime? ExpirationDateUtc { get; } = expirationDateUtc;

        public bool IsExpired
        {
            get
            {
                if (!ExpirationDateUtc.HasValue) return false;
                return ExpirationDateUtc.Value < DateTime.UtcNow;
            }
        }
    }

    private readonly PvNugsCsProviderPgSqlConfig _config = options.Value;
    private readonly IPvNugsSecretManager? _secretManager;

    private readonly ConcurrentDictionary<SqlRoleEnu, CsEntry> _csEntries = new();
    private readonly ConcurrentDictionary<SqlRoleEnu, SemaphoreSlim> _locks = new();

    public SqlRoleEnu Role { get; private set; }

    public bool UseDynamicCredentials => _config.Mode == CsProviderModeEnu.DynamicSecret;

    public string GetUsername(SqlRoleEnu role)
    {
        var exists = _csEntries.TryGetValue(role, out var csEntry);
        return exists ? csEntry!.UserName : string.Empty;
    }

    public string Schema => _config.Schema;

    public CsProvider(
        IConsoleLoggerService logger,
        IOptions<PvNugsCsProviderPgSqlConfig> options,
        IPvNugsSecretManager secretManager) : this(logger, options)
    {
        _secretManager = secretManager;
    }

    public async Task<string> GetConnectionStringAsync(
        SqlRoleEnu role = SqlRoleEnu.Reader,
        CancellationToken cancellationToken = default)
    {
        var entry = _csEntries.GetValueOrDefault(role);
        if (entry is not null && !entry.IsExpired) return entry.ConnectionString;

        var gate = _locks.GetOrAdd(
            role, _ => new SemaphoreSlim(1, 1));
        await gate.WaitAsync(cancellationToken)
            .ConfigureAwait(false);
        try
        {
            entry = _csEntries.GetValueOrDefault(role);
            if (entry is not null && !entry.IsExpired) return entry.ConnectionString;

            var fresh = await FetchCredentials(role, cancellationToken)
                .ConfigureAwait(false);
            _csEntries[role] = fresh;
            return fresh.ConnectionString;
        }
        catch (Exception e)
        {
            await logger.LogAsync(e);
            throw new PvNugsCsProviderException(e);
        }
        finally
        {
            gate.Release();
        }
    }

    private async Task<CsEntry> FetchCredentials(
        SqlRoleEnu role,
        CancellationToken cancellationToken = default)
    {
        string cs;
        DateTime? csExpirationDateUtc;
        Role = role;
        string username;
        switch (_config.Mode)
        {
            case CsProviderModeEnu.Config:
                if (string.IsNullOrEmpty(_config.Username))
                {
                    const string err = "Username not found in configuration";
                    await logger.LogAsync(err, SeverityEnu.Error);
                    throw new PvNugsCsProviderException(err);
                }
                username = _config.Username;
                cs = BuildConnectionString(
                    _config.Server, _config.Database,
                    _config.Port, username, _config.Password,
                    _config.Timezone, _config.TimeoutInSeconds);
                csExpirationDateUtc = null;
                break;

            case CsProviderModeEnu.StaticSecret:
                if (string.IsNullOrEmpty(_config.Username))
                {
                    const string err = "Username not found in configuration";
                    await logger.LogAsync(err, SeverityEnu.Error);
                    throw new PvNugsCsProviderException(err);
                }
                username = _config.Username;
                try
                {
                    var password = await
                        _secretManager!.GetStaticSecretAsync(
                            _config.SecretName!, cancellationToken);
                    if (password == null)
                    {
                        var err = $"password not found for {_config.SecretName}";
                        await logger.LogAsync(err, SeverityEnu.Error);
                        throw new PvNugsCsProviderException(err);
                    }
                    cs = BuildConnectionString(
                        _config.Server, _config.Database,
                        _config.Port, username, password,
                        _config.Timezone, _config.TimeoutInSeconds);
                    csExpirationDateUtc = null;
                }
                catch (Exception e)
                {
                    await logger.LogAsync(e);
                    throw new PvNugsCsProviderException(e);
                }

                break;

            case CsProviderModeEnu.DynamicSecret:
                try
                {
                    var dbSecret = await
                        _secretManager!.GetDynamicSecretAsync(
                            _config.SecretName!, cancellationToken);
                    if (dbSecret == null)
                    {
                        var err = $"dbSecret not found for {_config.SecretName}";
                        await logger.LogAsync(err, SeverityEnu.Error);
                        throw new PvNugsCsProviderException(err);
                    }
                    username = dbSecret.Username;
                    cs = BuildConnectionString(
                        _config.Server, _config.Database,
                        _config.Port, username, dbSecret.Password,
                        _config.Timezone, _config.TimeoutInSeconds);
                    csExpirationDateUtc = dbSecret.ExpirationDateUtc;
                }
                catch (Exception e)
                {
                    await logger.LogAsync(e);
                    throw new PvNugsCsProviderException(e);
                }

                break;
            default:
                throw new SwitchExpressionException();
        }

        cs += $"Search Path={Schema};";

        return new CsEntry(
            username, cs, csExpirationDateUtc);
    }

    private static string BuildConnectionString(
        string server,
        string database,
        int? port,
        string username,
        string? password,
        string? timezone,
        int? timeoutInSeconds)
    {
        var cs = $"Server={server};" +
                 $"Database={database};" +
                 $"User Id={username};" +
                 "Include Error Detail=true";
        if (port.HasValue)
            cs += $"Port={port.Value};";
        if (!string.IsNullOrEmpty(password))
            cs += $"Password={password};";
        if (!string.IsNullOrEmpty(timezone))
            cs += $"TimeZone={timezone};";
        if (timeoutInSeconds.HasValue)
            cs += $"CommandTimeout={timeoutInSeconds.Value};";

        return cs;
    }
}
using pvNugsCsProviderNc10Abstractions;

namespace pvNugsLoggerNc10PgSql.it;

public class TestBenchCsProvider: IPvNugsPgSqlCsProvider
{
    public Task<string> GetConnectionStringAsync(
        string connectionStringName, 
        CsProviderSqlRoleEnu role = CsProviderSqlRoleEnu.Reader,
        CancellationToken cancellationToken = default)
    {
        return GetConnectionStringAsync(role, cancellationToken);
    }

    public async Task<string> GetConnectionStringAsync(
        CsProviderSqlRoleEnu role = CsProviderSqlRoleEnu.Reader,
        CancellationToken cancellationToken = default)
    {
        return await Task.FromResult(
            "Host=localhost;" +
            "Port=5432;" +
            $"Username={GetUsername(role)};" +
            "Password=dev-only-password;" +
            "Database=postgres");
    }

    public bool IsDynamicCredentials(string connectionStringName)
    {
        return false;
    }

    public string GetUsername(CsProviderSqlRoleEnu role)
    {
        return "postgres";
    }

    public string GetUsername(
        string connectionStringName, CsProviderSqlRoleEnu role)
    {
        return GetUsername(role);
    }

    public string GetSchema(string connectionStringName)
    {
        return Schema;
    }

    public bool UseDynamicCredentials => false;
    public string Schema => "int-testing";
}
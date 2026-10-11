using pvNugsLoggerNc10Abstractions;
using pvNugsSecretManagerNc10Abstractions;

namespace pvNugsCsProviderNc10PgSql.it;

public class TestSecretManager(IConsoleLoggerService logger): IPvNugsSecretManager
{
    public async Task<IReadOnlyDictionary<string, string>> GetStaticSecretsAsync(
        IReadOnlyDictionary<string, string> parameters,
        CancellationToken cancellationToken = default)  
    {
        await logger.LogAsync(
            "GetStaticSecretsAsync called with parameters: " + StringifyParameters(parameters),
            SeverityEnu.Trace, cancellationToken);
        var dic = new Dictionary<string, string>
        {
            { "StaticSecret1", "Value1" },
            { "StaticSecret2", "Value2" }
        };
        return dic;
    }
    
    public async Task<string?> GetStaticSecretAsync(
        IReadOnlyDictionary<string, string> parameters,
        CancellationToken cancellationToken = default)
    {
        await logger.LogAsync(
            "GetStaticSecretAsync called with parameters: " + StringifyParameters(parameters),
            SeverityEnu.Trace, cancellationToken);
        return "StaticSecretValue";
    }

    public async Task<IPvNugsDynamicCredential?> GetDynamicSecretAsync(
        IReadOnlyDictionary<string, string> parameters,
        CancellationToken cancellationToken = default)
    {
        await logger.LogAsync(
            "GetDynamicSecretAsync called with parameters: " + StringifyParameters(parameters),
            SeverityEnu.Trace, cancellationToken);
        return new TestDynamicCredential();
    }
    
    private static string StringifyParameters(IReadOnlyDictionary<string, string> parameters)
    {
        return string.Join(", ", parameters.Select(kv => $"{kv.Key}={kv.Value}"));
    }


    public bool SupportsDatabaseSecrets => true;
}
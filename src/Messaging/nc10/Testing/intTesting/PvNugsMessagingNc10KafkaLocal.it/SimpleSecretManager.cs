using pvNugsSecretManagerNc10Abstractions;

namespace PvNugsMessagingNc10KafkaLocal.it;

public class SimpleSecretManager: IPvNugsSecretManager
{
    public Task<IReadOnlyDictionary<string, string>> GetStaticSecretsAsync(IReadOnlyDictionary<string, string> parameters,
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<string?> GetStaticSecretAsync(IReadOnlyDictionary<string, string> parameters,
        CancellationToken cancellationToken = default)
    {
        var secretName = parameters.TryGetValue("secretName", out var name) ? name
            : throw new ArgumentException("Parameter 'secretName' is required.", nameof(parameters));
        return secretName switch
        {
            "kafka-username" => Task.FromResult<string?>("pvway"),
            "kafka-password" => Task.FromResult<string?>("pvway-secret"),
            _ => throw new ArgumentException($"Unknown secret name: {secretName}", nameof(parameters))
        };
    }

    public Task<IPvNugsDynamicCredential?> GetDynamicSecretAsync(IReadOnlyDictionary<string, string> parameters,
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public bool SupportsDatabaseSecrets => false;
}
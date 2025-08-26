namespace pvNugsSecretManagerNc9Abstractions;

public interface IPvNugsSecretManager
{
    Task<string?> GetStaticSecretAsync(
        string secretName, CancellationToken cancellationToken = default);
    Task<IPvNugsDynamicCredential?> GetDynamicSecretAsync(
        string secretName, CancellationToken cancellationToken = default);
}
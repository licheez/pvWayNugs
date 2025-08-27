namespace pvNugsSecretManagerNc9Abstractions;

public interface IPvNugsStaticSecretManager
{
    Task<string?> GetStaticSecretAsync(
        string secretName, CancellationToken cancellationToken = default);
}

public interface IPvNugsDynamicSecretManager : IPvNugsStaticSecretManager
{
    Task<IPvNugsDynamicCredential?> GetDynamicSecretAsync(
        string secretName, CancellationToken cancellationToken = default);
}
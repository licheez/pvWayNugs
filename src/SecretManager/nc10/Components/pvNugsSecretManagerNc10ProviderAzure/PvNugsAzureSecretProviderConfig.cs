// ReSharper disable PropertyCanBeMadeInitOnly.Global

using Azure.Identity;

namespace pvNugsSecretManagerNc10ProviderAzure;

/// <summary>
/// Azure Key Vault configuration used by <see cref="AzureSecretProvider"/>.
/// </summary>
public class PvNugsAzureSecretProviderConfig
{
    /// <summary>
    /// Configuration section name.
    /// </summary>
    public const string Section = nameof(PvNugsAzureSecretProviderConfig);

    /// <summary>
    /// The Azure Key Vault URL.
    /// </summary>
    public string KeyVaultUrl { get; set; } = null!;

    /// <summary>
    /// Optional service principal credentials; when <see langword="null"/>, managed identity or
    /// <see cref="DefaultAzureCredential"/> is used.
    /// </summary>
    public PvNugsAzureServicePrincipalCredential? Credential { get; set; }
}
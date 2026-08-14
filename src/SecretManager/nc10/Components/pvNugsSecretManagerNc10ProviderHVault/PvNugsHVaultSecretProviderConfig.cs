namespace pvNugsSecretManagerNc10ProviderHVault;

/// <summary>
/// Configuration settings for the HashiCorp Vault secret provider.
/// These settings should be configured in the application's configuration file under the section specified by <see cref="Section"/>.
/// </summary>
public class PvNugsHVaultSecretProviderConfig
{
    /// <summary>
    /// The configuration section name for Vault provider settings.
    /// </summary>
    public const string Section = nameof(PvNugsHVaultSecretProviderConfig);
    
    /// <summary>
    /// Gets or sets the authentication method to use when connecting to Vault.
    /// Defaults to <see cref="PvNugsHVaultSecretProviderAuthEnu.TokenAuth"/>.
    /// </summary>
    public PvNugsHVaultSecretProviderAuthEnu AuthMethod { get; set; } = PvNugsHVaultSecretProviderAuthEnu.TokenAuth;
    
    /// <summary>
    /// Gets or sets the file path to the Vault token or Kubernetes service account token.
    /// Required for both authentication methods.
    /// </summary>
    public string TokenFilePath { get; set; } = null!;
    
    /// <summary>
    /// Gets or sets the Vault server URL (e.g., "http://localhost:8200" or "https://vault.example.com").
    /// Required for all authentication methods.
    /// </summary>
    public string ServerUrl { get; set; } = null!;
    
    /// <summary>
    /// Gets or sets the Vault token. If not provided, it will be read from <see cref="TokenFilePath"/>.
    /// Optional - can be set programmatically instead of reading from file.
    /// </summary>
    public string? Token { get; set; }
    
    /// <summary>
    /// Gets or sets the Kubernetes authentication mount point in Vault (e.g., "kubernetes").
    /// Required when using <see cref="PvNugsHVaultSecretProviderAuthEnu.Kubernetes"/> authentication.
    /// </summary>
    public string KubeMountPoint { get; set; } = null!;
    
    /// <summary>
    /// Gets or sets the Kubernetes role name configured in Vault.
    /// Required when using <see cref="PvNugsHVaultSecretProviderAuthEnu.Kubernetes"/> authentication.
    /// </summary>
    public string KubeRoleName { get; set; } = null!;
    
    /// <summary>
    /// Gets or sets the Vault namespace for Kubernetes authentication.
    /// Required when using <see cref="PvNugsHVaultSecretProviderAuthEnu.Kubernetes"/> authentication in namespaced Vault instances.
    /// </summary>
    public string KubeNameSpace { get; set; } = null!;
    
}
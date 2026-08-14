namespace pvNugsSecretManagerNc10ProviderHVault;

/// <summary>
/// Defines the authentication methods supported by the HashiCorp Vault secret provider.
/// </summary>
public enum PvNugsHVaultSecretProviderAuthEnu
{
    /// <summary>
    /// Kubernetes-based authentication using a service account token.
    /// Typically used when running inside a Kubernetes cluster.
    /// </summary>
    Kubernetes,
    
    /// <summary>
    /// Token-based authentication using a Vault token.
    /// Can be used for development or when direct token access is available.
    /// </summary>
    TokenAuth
}
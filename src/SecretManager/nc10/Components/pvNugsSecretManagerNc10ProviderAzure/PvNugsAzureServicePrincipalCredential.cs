namespace pvNugsSecretManagerNc10ProviderAzure;

/// <summary>
/// Azure service principal credentials used by <see cref="AzureSecretProvider"/>.
/// </summary>
public class PvNugsAzureServicePrincipalCredential
{
    /// <summary>
    /// Azure tenant ID.
    /// </summary>
    public string TenantId { get; set; } = null!;

    /// <summary>
    /// Azure client ID (application ID).
    /// </summary>
    public string ClientId { get; set; } = null!;

    /// <summary>
    /// Azure client secret.
    /// </summary>
    public string ClientSecret { get; set; } = null!;
}
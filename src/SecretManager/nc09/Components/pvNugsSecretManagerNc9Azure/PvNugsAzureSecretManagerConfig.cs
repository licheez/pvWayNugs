namespace pvNugsSecretManagerNc9Azure;

public class PvNugsAzureSecretManagerConfig
{
    public const string Section = nameof(PvNugsAzureSecretManagerConfig);
    
    public string KeyVaultUrl { get; set; } = null!;
    public PvNugsAzureSecretManagerSecretClientCredential? Credential { get; set; } = null!;
    
}

// ReSharper disable once ClassNeverInstantiated.Global
public class PvNugsAzureSecretManagerSecretClientCredential
{
    public string TenantId { get; set; } = null!;
    public string ClientId { get; set; } = null!;
    public string ClientSecret { get; set; } = null!;
}
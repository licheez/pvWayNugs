namespace pvNugsSecretManagerNc10ProviderHVault;

/// <summary>
/// Represents errors that occur during HashiCorp Vault secret provider operations.
/// Wraps underlying exceptions from Vault client operations or configuration issues.
/// </summary>
public class PvNugsHVaultException(Exception innerException)
    : Exception("PvNugsHVaultException occurred.", innerException);
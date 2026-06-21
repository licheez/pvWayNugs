namespace pvNugsSecretManagerNc10ProviderAzure;

/// <summary>
/// Canonical parameter keys used by <see cref="AzureSecretProvider"/>.
/// </summary>
public static class PvNugsAzureSecretProviderParameters
{
    /// <summary>
    /// The required key for single secret retrieval.
    /// </summary>
    public const string SecretName = "secretName";
    
    /// <summary>
    /// Creates the Azure provider parameter dictionary for single-secret retrieval.
    /// </summary>
    /// <param name="secretName">
    /// The Azure Key Vault secret name to assign to the <see cref="SecretName"/> key.
    /// </param>
    /// <returns>
    /// A read-only dictionary containing the required <see cref="SecretName"/> parameter.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="secretName"/> is <see langword="null"/> or empty.
    /// </exception>
    public static IReadOnlyDictionary<string, string> CreateParameters(
        string secretName)
    {
        if (string.IsNullOrEmpty(secretName))
        {
            throw new ArgumentNullException(nameof(secretName));
        }
        return new Dictionary<string, string>
        {
            { SecretName, secretName }
        };
    }
}
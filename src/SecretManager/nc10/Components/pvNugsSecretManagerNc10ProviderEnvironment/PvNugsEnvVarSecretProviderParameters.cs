namespace pvNugsSecretManagerNc10ProviderEnvironment;

/// <summary>
/// Provides parameter constants and helper methods for configuring the Environment Variable secret provider.
/// </summary>
/// <remarks>
/// <para>
/// This class defines the required parameters for retrieving secrets from environment variables 
/// or configuration sources through the Microsoft.Extensions.Configuration system.
/// </para>
/// <para>The provider supports retrieval of static secrets only and requires the <see cref="SecretName"/> 
/// parameter to identify which configuration value to retrieve.</para>
/// </remarks>
/// <example>
/// <para><b>Creating parameters for secret retrieval:</b></para>
/// <code>
/// // Create parameters using the helper method
/// var parameters = PvNugsEnvVarSecretProviderParameters.CreateParameters("DatabasePassword");
/// 
/// // Retrieve the secret using the secret manager
/// var secretManager = serviceProvider.GetService&lt;IStaticSecretManager&gt;();
/// var secret = await secretManager.GetStaticSecretAsync(parameters);
/// 
/// // Or manually create the parameter dictionary
/// var manualParams = new Dictionary&lt;string, string&gt;
/// {
///     { PvNugsEnvVarSecretProviderParameters.SecretName, "ApiKey" }
/// };
/// var apiKey = await secretManager.GetStaticSecretAsync(manualParams);
/// </code>
/// </example>
public class PvNugsEnvVarSecretProviderParameters
{
    /// <summary>
    /// The required key for single secret retrieval from environment variables or configuration sources.
    /// </summary>
    /// <remarks>
    /// This constant defines the key name that must be present in the parameter dictionary 
    /// when calling <c>GetStaticSecretAsync</c>. The corresponding value should be the 
    /// configuration key name to retrieve (e.g., "DatabasePassword", "ApiKey").
    /// </remarks>
    public const string SecretName = "secretName";
    
    /// <summary>
    /// Creates the environment variable provider parameter dictionary for single-secret retrieval.
    /// </summary>
    /// <param name="secretName">
    /// The configuration key name to retrieve from environment variables or configuration sources.
    /// This should match the key name in your configuration (e.g., environment variable name,
    /// appsettings.json key, etc.).
    /// </param>
    /// <returns>
    /// A read-only dictionary containing the required <see cref="SecretName"/> parameter.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="secretName"/> is <see langword="null"/> or empty.
    /// </exception>
    /// <remarks>
    /// <para>This helper method simplifies parameter creation and ensures the correct parameter 
    /// structure for the environment variable provider.</para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Retrieve a database password from configuration
    /// var parameters = PvNugsEnvVarSecretProviderParameters.CreateParameters("DatabasePassword");
    /// var secretManager = serviceProvider.GetService&lt;IStaticSecretManager&gt;();
    /// var dbPassword = await secretManager.GetStaticSecretAsync(parameters);
    /// 
    /// // With a prefix configured as "MyApp", this would look for:
    /// // - Environment variable: MyApp__DatabasePassword
    /// // - Or in appsettings.json: { "MyApp": { "DatabasePassword": "value" } }
    /// </code>
    /// </example>
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
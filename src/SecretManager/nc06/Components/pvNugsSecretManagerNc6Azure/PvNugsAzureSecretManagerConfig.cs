// ReSharper disable PropertyCanBeMadeInitOnly.Global

using pvNugsSecretManagerNc6Abstractions;

namespace pvNugsSecretManagerNc6Azure;

/// <summary>
/// Configuration model for Azure Key Vault secret management integration.
/// This class defines the configuration settings required to establish connection and authentication
/// with Azure Key Vault for secure secret retrieval operations.
/// </summary>
/// <remarks>
/// <para><c>Purpose and Usage:</c></para>
/// <para>This configuration class is designed to work with the .NET configuration system and provides
/// the necessary settings for Azure Key Vault integration. It supports both managed identity and 
/// service principal authentication methods through the optional <see cref="Credential"/> property.</para>
/// 
/// <para><c>Configuration Binding:</c></para>
/// <para>This class is intended to be bound from application configuration sources such as appsettings.json,
/// environment variables, Azure App Configuration, or other IConfiguration providers. The <see cref="Section"/>
/// constant defines the configuration section name for proper binding.</para>
/// 
/// <para><c>Authentication Methods:</c></para>
/// <list type="bullet">
/// <item><description><c>Managed Identity:</c> When <see cref="Credential"/> is null, the implementation should use Azure Managed Identity for authentication</description></item>
/// <item><description><c>Service Principal:</c> When <see cref="Credential"/> is provided, uses client credentials flow with the specified tenant, client ID, and client secret</description></item>
/// </list>
/// 
/// <para><c>Security Considerations:</c></para>
/// <list type="bullet">
/// <item><description>The <see cref="KeyVaultUrl"/> should always use HTTPS protocol for secure communication</description></item>
/// <item><description>When using service principal authentication, ensure client secrets are stored securely and rotated regularly</description></item>
/// <item><description>Consider using Azure Managed Identity in production environments for enhanced security</description></item>
/// <item><description>Validate that the Key Vault URL is accessible from the application's network context</description></item>
/// </list>
/// 
/// <para><c>Configuration Validation:</c></para>
/// <para>Implementations should validate that the <see cref="KeyVaultUrl"/> is a valid Azure Key Vault URL
/// and that authentication credentials (if provided) are properly formatted. The URL should follow the format:
/// https://[vault-name].vault.azure.net/</para>
/// </remarks>
/// <example>
/// <para>Configuration in appSettings.json with managed identity:</para>
/// <code>
/// {
///   "PvNugsAzureSecretManagerConfig": {
///     "KeyVaultUrl": "https://myvault.vault.azure.net/",
///     "Credential": null
///   }
/// }
/// </code>
/// 
/// <para>Configuration in appSettings.json with service principal:</para>
/// <code>
/// {
///   "PvNugsAzureSecretManagerConfig": {
///     "KeyVaultUrl": "https://myvault.vault.azure.net/",
///     "Credential": {
///       "TenantId": "12345678-1234-1234-1234-123456789012",
///       "ClientId": "87654321-4321-4321-4321-210987654321",
///       "ClientSecret": "your-client-secret-value"
///     }
///   }
/// }
/// </code>
/// 
/// <para>Dependency injection registration:</para>
/// <code>
/// // In Program.cs or Startup.cs
/// services.Configure&lt;PvNugsAzureSecretManagerConfig&gt;(
///     configuration.GetSection(PvNugsAzureSecretManagerConfig.Section));
/// 
/// // Usage in service
/// public class MyService
/// {
///     private readonly PvNugsAzureSecretManagerConfig _config;
///     
///     public MyService(IOptions&lt;PvNugsAzureSecretManagerConfig&gt; options)
///     {
///         _config = options.Value;
///     }
/// }
/// </code>
/// 
/// <para>Manual configuration binding:</para>
/// <code>
/// var config = new PvNugsAzureSecretManagerConfig();
/// configuration.GetSection(PvNugsAzureSecretManagerConfig.Section).Bind(config);
/// 
/// // Validate configuration
/// if (string.IsNullOrWhiteSpace(config.KeyVaultUrl))
/// {
///     throw new InvalidOperationException("Key Vault URL is required");
/// }
/// 
/// if (!Uri.IsWellFormedUriString(config.KeyVaultUrl, UriKind.Absolute))
/// {
///     throw new ArgumentException("Key Vault URL must be a valid absolute URL");
/// }
/// </code>
/// </example>
/// <seealso cref="PvNugsAzureServicePrincipalCredential"/>
/// <seealso cref="IPvNugsStaticSecretManager"/>
public class PvNugsAzureSecretManagerConfig
{
    /// <summary>
    /// The configuration section name used for binding this configuration class from IConfiguration sources.
    /// This constant provides a centralized way to reference the configuration section and ensures consistency
    /// across the application when working with configuration providers.
    /// </summary>
    /// <value>
    /// Returns the name of this class (<c>"PvNugsAzureSecretManagerConfig"</c>) which serves as the configuration section identifier.
    /// </value>
    /// <remarks>
    /// <para><c>Usage Pattern:</c></para>
    /// <para>This constant is typically used with the Options pattern and IConfiguration binding to automatically
    /// map configuration values from various sources (appsettings.json, environment variables, etc.) to this
    /// configuration class instance.</para>
    /// 
    /// <para><c>Configuration Sources:</c></para>
    /// <para>The section name can be used across different configuration sources:</para>
    /// <list type="bullet">
    /// <item><description>JSON configuration files (appsettings.json, appsettings.{Environment}.json)</description></item>
    /// <item><description>Environment variables (using the section name as prefix)</description></item>
    /// <item><description>Azure App Configuration service</description></item>
    /// <item><description>Command line arguments</description></item>
    /// <item><description>In-memory configuration providers</description></item>
    /// </list>
    /// 
    /// <para><c>Best Practices:</c></para>
    /// <list type="bullet">
    /// <item><description>Use this constant instead of hardcoding the section name to avoid typos and maintain consistency</description></item>
    /// <item><description>The section name matches the class name by convention, making it self-documenting</description></item>
    /// <item><description>This approach supports refactoring scenarios where the class name might change</description></item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <para>Using the Section constant for configuration binding:</para>
    /// <code>
    /// // In dependency injection setup
    /// services.Configure&lt;PvNugsAzureSecretManagerConfig&gt;(
    ///     configuration.GetSection(PvNugsAzureSecretManagerConfig.Section));
    /// 
    /// // Direct binding
    /// var azureConfig = configuration
    ///     .GetSection(PvNugsAzureSecretManagerConfig.Section)
    ///     .Get&lt;PvNugsAzureSecretManagerConfig&gt;();
    /// 
    /// // Validation with configuration
    /// services.Configure&lt;PvNugsAzureSecretManagerConfig&gt;(
    ///     configuration.GetSection(PvNugsAzureSecretManagerConfig.Section))
    ///     .ValidateDataAnnotations();
    /// </code>
    /// 
    /// <para>Environment variable naming convention:</para>
    /// <code>
    /// // Environment variables would be named:
    /// // PvNugsAzureSecretManagerConfig__KeyVaultUrl
    /// // PvNugsAzureSecretManagerConfig__Credential__TenantId
    /// // PvNugsAzureSecretManagerConfig__Credential__ClientId
    /// // PvNugsAzureSecretManagerConfig__Credential__ClientSecret
    /// </code>
    /// </example>
    public const string Section = nameof(PvNugsAzureSecretManagerConfig);
    
    /// <summary>
    /// Gets or sets the Azure Key Vault URL used for secret retrieval operations.
    /// This URL specifies the endpoint for the specific Azure Key Vault instance that contains
    /// the secrets to be accessed by the secret management implementation.
    /// </summary>
    /// <value>
    /// The fully qualified URL of the Azure Key Vault instance. Must be a valid HTTPS URL
    /// following the Azure Key Vault naming convention: https://[vault-name].vault.azure.net/
    /// </value>
    /// <remarks>
    /// <para><c>URL Format Requirements:</c></para>
    /// <list type="bullet">
    /// <item><description>Must use HTTPS protocol for secure communication</description></item>
    /// <item><description>Must follow Azure Key Vault URL format: https://[vault-name].vault.azure.net/</description></item>
    /// <item><description>The vault name must be globally unique across Azure</description></item>
    /// <item><description>The vault name can contain only alphanumeric characters and hyphens</description></item>
    /// <item><description>The vault name must be between 3-24 characters long</description></item>
    /// </list>
    /// 
    /// <para><c>Security and Access:</c></para>
    /// <list type="bullet">
    /// <item><description>The Key Vault must be accessible from the application's network context</description></item>
    /// <item><description>Appropriate access policies or RBAC permissions must be configured for the authentication principal</description></item>
    /// <item><description>Consider network restrictions and firewall rules when configuring Key Vault access</description></item>
    /// <item><description>The URL itself is not sensitive information, but the vault contents are highly sensitive</description></item>
    /// </list>
    /// 
    /// <para><c>Configuration Sources:</c></para>
    /// <para>This property is typically configured through:</para>
    /// <list type="bullet">
    /// <item><description>Application configuration files (appsettings.json)</description></item>
    /// <item><description>Environment variables (especially in containerized deployments)</description></item>
    /// <item><description>Azure App Configuration for centralized configuration management</description></item>
    /// <item><description>Infrastructure as Code templates (ARM, Terraform, Bicep)</description></item>
    /// </list>
    /// 
    /// <para><c>Environment-Specific Configuration:</c></para>
    /// <para>Different environments typically have separate Key Vault instances for security isolation:</para>
    /// <list type="bullet">
    /// <item><description>Development: https://myapp-dev-kv.vault.azure.net/</description></item>
    /// <item><description>Staging: https://myapp-staging-kv.vault.azure.net/</description></item>
    /// <item><description>Production: https://myapp-prod-kv.vault.azure.net/</description></item>
    /// </list>
    /// 
    /// <para><c>Validation Considerations:</c></para>
    /// <para>Implementations should validate this URL to ensure:</para>
    /// <list type="bullet">
    /// <item><description>The URL is well-formed and uses HTTPS protocol</description></item>
    /// <item><description>The URL is reachable from the application's network context</description></item>
    /// <item><description>The vault exists and is accessible with the configured credentials</description></item>
    /// <item><description>The URL ends with the proper Azure Key Vault domain suffix</description></item>
    /// </list>
    /// </remarks>
    /// <exception cref="System.ArgumentNullException">
    /// May be thrown by implementations when the URL is null during configuration validation.
    /// </exception>
    /// <exception cref="System.ArgumentException">
    /// May be thrown by implementations when the URL format is invalid or doesn't meet Azure Key Vault URL requirements.
    /// </exception>
    /// <exception cref="System.UriFormatException">
    /// May be thrown by implementations when the URL is not a valid URI format.
    /// </exception>
    /// <example>
    /// <para>Valid Azure Key Vault URLs:</para>
    /// <code>
    /// // Production Key Vault
    /// config.KeyVaultUrl = "https://mycompany-prod-kv.vault.azure.net/";
    /// 
    /// // Development Key Vault  
    /// config.KeyVaultUrl = "https://mycompany-dev-keyvault.vault.azure.net/";
    /// 
    /// // Regional Key Vault (note: all Key Vaults use the same domain suffix)
    /// config.KeyVaultUrl = "https://myapp-westus2-secrets.vault.azure.net/";
    /// </code>
    /// 
    /// <para>Configuration in appsettings.json:</para>
    /// <code>
    /// {
    ///   "PvNugsAzureSecretManagerConfig": {
    ///     "KeyVaultUrl": "https://myapp-keyvault.vault.azure.net/"
    ///   }
    /// }
    /// </code>
    /// 
    /// <para>Environment variable configuration:</para>
    /// <code>
    /// // Environment variable name
    /// PvNugsAzureSecretManagerConfig__KeyVaultUrl=https://myapp-prod-kv.vault.azure.net/
    /// </code>
    /// 
    /// <para>URL validation example:</para>
    /// <code>
    /// public void ValidateKeyVaultUrl(string keyVaultUrl)
    /// {
    ///     if (string.IsNullOrWhiteSpace(keyVaultUrl))
    ///         throw new ArgumentException("Key Vault URL cannot be null or empty");
    ///         
    ///     if (!Uri.TryCreate(keyVaultUrl, UriKind.Absolute, out var uri))
    ///         throw new ArgumentException("Key Vault URL must be a valid absolute URL");
    ///         
    ///     if (uri.Scheme != "https")
    ///         throw new ArgumentException("Key Vault URL must use HTTPS protocol");
    ///         
    ///     if (!uri.Host.EndsWith(".vault.azure.net", StringComparison.OrdinalIgnoreCase))
    ///         throw new ArgumentException("Key Vault URL must be a valid Azure Key Vault endpoint");
    /// }
    /// </code>
    /// </example>
    /// <seealso cref="PvNugsAzureServicePrincipalCredential"/>
    /// <seealso cref="System.Uri"/>
    public string KeyVaultUrl { get; set; } = null!;
    
    /// <summary>
    /// Gets or sets the credential configuration for authenticating with Azure Key Vault.
    /// When specified, this property contains the service principal credentials (tenant ID, client ID, and client secret)
    /// required for client credentials flow authentication. When null, the implementation should use Azure Managed Identity
    /// for authentication, which is the recommended approach for Azure-hosted applications.
    /// </summary>
    /// <value>
    /// A <see cref="PvNugsAzureServicePrincipalCredential"/> object containing service principal authentication details,
    /// or <c>null</c> to indicate that Azure Managed Identity should be used for authentication.
    /// </value>
    /// <remarks>
    /// <para><c>Authentication Methods:</c></para>
    /// <para>This property supports two primary authentication methods for Azure Key Vault:</para>
    /// <list type="bullet">
    /// <item><description><c>Managed Identity (Recommended):</c> When this property is <c>null</c>, the implementation uses Azure Managed Identity, which provides secure, keyless authentication for Azure services</description></item>
    /// <item><description><c>Service Principal:</c> When this property contains credential details, the implementation uses the client credentials flow with the specified tenant ID, client ID, and client secret</description></item>
    /// </list>
    /// 
    /// <para><c>Security Best Practices:</c></para>
    /// <list type="bullet">
    /// <item><description><c>Prefer Managed Identity:</c> In Azure-hosted environments (App Service, Azure Functions, Virtual Machines, etc.), use managed identity to eliminate the need for storing secrets</description></item>
    /// <item><description><c>Secure Secret Storage:</c> When service principal authentication is necessary, store client secrets in secure locations (environment variables, separate Key Vault, etc.)</description></item>
    /// <item><description><c>Regular Rotation:</c> Implement regular rotation of client secrets to minimize security risks</description></item>
    /// <item><description><c>Principle of Least Privilege:</c> Grant only the minimum required permissions to the service principal or managed identity</description></item>
    /// <item><description><c>Environment Separation:</c> Use different service principals for different environments (dev, staging, production)</description></item>
    /// </list>
    /// 
    /// <para><c>Configuration Scenarios:</c></para>
    /// <list type="bullet">
    /// <item><description><c>Azure App Service:</c> Use managed identity by setting this property to null</description></item>
    /// <item><description><c>Azure Functions:</c> Use managed identity by setting this property to null</description></item>
    /// <item><description><c>Local Development:</c> Use service principal credentials for local testing and debugging</description></item>
    /// <item><description><c>On-Premises:</c> Use service principal credentials when running outside of Azure</description></item>
    /// <item><description><c>Third-Party Cloud:</c> Use service principal credentials when running on non-Azure infrastructure</description></item>
    /// </list>
    /// 
    /// <para><c>Permission Requirements:</c></para>
    /// <para>The authentication principal (managed identity or service principal) must have appropriate permissions:</para>
    /// <list type="bullet">
    /// <item><description><c>Key Vault Access Policy:</c> Grant "Get" permission for secrets, and optionally "List" if the application needs to enumerate secrets</description></item>
    /// <item><description><c>RBAC Permissions:</c> Assign "Key Vault Secrets User" role or custom roles with appropriate permissions</description></item>
    /// <item><description><c>Network Access:</c> Ensure the Key Vault's network access policies allow connections from the application's network context</description></item>
    /// </list>
    /// 
    /// <para><c>Environment-Specific Configuration:</c></para>
    /// <para>Different environments may require different authentication approaches:</para>
    /// <list type="bullet">
    /// <item><description><c>Development:</c> Service principal for local development, managed identity for dev environment</description></item>
    /// <item><description><c>Staging:</c> Managed identity with staging-specific permissions</description></item>
    /// <item><description><c>Production:</c> Managed identity with production-specific permissions and network restrictions</description></item>
    /// </list>
    /// </remarks>
    /// <exception cref="System.Security.Authentication.AuthenticationException">
    /// May be thrown by implementations when authentication fails due to invalid credentials or configuration.
    /// </exception>
    /// <exception cref="System.UnauthorizedAccessException">
    /// May be thrown by implementations when the authenticated principal lacks sufficient permissions to access Key Vault.
    /// </exception>
    /// <example>
    /// <para>Configuration for managed identity (recommended for Azure environments):</para>
    /// <code>
    /// {
    ///   "PvNugsAzureSecretManagerConfig": {
    ///     "KeyVaultUrl": "https://myapp-prod-kv.vault.azure.net/",
    ///     "Credential": null
    ///   }
    /// }
    /// </code>
    /// 
    /// <para>Configuration for service principal authentication:</para>
    /// <code>
    /// {
    ///   "PvNugsAzureSecretManagerConfig": {
    ///     "KeyVaultUrl": "https://myapp-dev-kv.vault.azure.net/",
    ///     "Credential": {
    ///       "TenantId": "12345678-1234-1234-1234-123456789012",
    ///       "ClientId": "87654321-4321-4321-4321-210987654321",
    ///       "ClientSecret": "your-client-secret-value"
    ///     }
    ///   }
    /// }
    /// </code>
    /// 
    /// <para>Environment variable configuration for service principal:</para>
    /// <code>
    /// PvNugsAzureSecretManagerConfig__Credential__TenantId=12345678-1234-1234-1234-123456789012
    /// PvNugsAzureSecretManagerConfig__Credential__ClientId=87654321-4321-4321-4321-210987654321
    /// PvNugsAzureSecretManagerConfig__Credential__ClientSecret=your-client-secret-value
    /// </code>
    /// 
    /// <para>Conditional configuration based on environment:</para>
    /// <code>
    /// // In Program.cs - different configuration per environment
    /// if (builder.Environment.IsDevelopment())
    /// {
    ///     // Use service principal for local development
    ///     services.Configure&lt;PvNugsAzureSecretManagerConfig&gt;(options =&gt;
    ///     {
    ///         options.KeyVaultUrl = "https://myapp-dev-kv.vault.azure.net/";
    ///         options.Credential = new PvNugsAzureServicePrincipalCredential
    ///         {
    ///             TenantId = builder.Configuration["AzureAd:TenantId"],
    ///             ClientId = builder.Configuration["AzureAd:ClientId"], 
    ///             ClientSecret = builder.Configuration["AzureAd:ClientSecret"]
    ///         };
    ///     });
    /// }
    /// else
    /// {
    ///     // Use managed identity for Azure environments
    ///     services.Configure&lt;PvNugsAzureSecretManagerConfig&gt;(options =&gt;
    ///     {
    ///         options.KeyVaultUrl = builder.Configuration["KeyVault:Url"];
    ///         options.Credential = null; // Use managed identity
    ///     });
    /// }
    /// </code>
    /// 
    /// <para>Authentication validation example:</para>
    /// <code>
    /// public void ValidateCredentialConfiguration(PvNugsAzureSecretManagerConfig config)
    /// {
    ///     if (config.Credential != null)
    ///     {
    ///         // Validate service principal configuration
    ///         if (string.IsNullOrWhiteSpace(config.Credential.TenantId))
    ///             throw new ArgumentException("TenantId is required when using service principal authentication");
    ///             
    ///         if (string.IsNullOrWhiteSpace(config.Credential.ClientId))
    ///             throw new ArgumentException("ClientId is required when using service principal authentication");
    ///             
    ///         if (string.IsNullOrWhiteSpace(config.Credential.ClientSecret))
    ///             throw new ArgumentException("ClientSecret is required when using service principal authentication");
    ///             
    ///         // Validate GUID formats
    ///         if (!Guid.TryParse(config.Credential.TenantId, out _))
    ///             throw new ArgumentException("TenantId must be a valid GUID");
    ///             
    ///         if (!Guid.TryParse(config.Credential.ClientId, out _))
    ///             throw new ArgumentException("ClientId must be a valid GUID");
    ///     }
    ///     // When Credential is null, managed identity will be used (no additional validation needed)
    /// }
    /// </code>
    /// </example>
    /// <seealso cref="PvNugsAzureServicePrincipalCredential"/>
    /// <seealso href="https://docs.microsoft.com/en-us/azure/active-directory/managed-identities-azure-resources/">Azure Managed Identities</seealso>
    /// <seealso href="https://docs.microsoft.com/en-us/azure/key-vault/general/authentication">Azure Key Vault Authentication</seealso>
    public PvNugsAzureServicePrincipalCredential? Credential { get; set; }
    
}
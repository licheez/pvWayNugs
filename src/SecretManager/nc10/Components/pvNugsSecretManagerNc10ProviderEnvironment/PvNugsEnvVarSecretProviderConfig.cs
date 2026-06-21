namespace pvNugsSecretManagerNc10ProviderEnvironment;

/// <summary>
/// Provides configuration settings for the pvNugs Secret Manager environment variables implementation.
/// This configuration class defines options for organizing and accessing secrets stored in 
/// environment variables or other configuration sources within the Microsoft Configuration system.
/// </summary>
/// <remarks>
/// <para><b>Configuration Sources:</b></para>
/// <para>This configuration class is designed to work with the Microsoft.Extensions.Configuration
/// system and supports binding from various configuration sources including:</para>
/// <list type="bullet">
/// <item><description><b>Environment variables:</b> Standard and prefixed environment variable patterns</description></item>
/// <item><description><b>JSON files:</b> appsettings.json and environment-specific configuration files</description></item>
/// <item><description><b>Azure Key Vault:</b> When configured as a configuration provider (not for secret generation)</description></item>
/// <item><description><b>Command line arguments:</b> Runtime configuration overrides</description></item>
/// <item><description><b>In-memory collections:</b> Programmatic configuration for testing scenarios</description></item>
/// <item><description><b>Custom providers:</b> Any IConfiguration provider implementation</description></item>
/// </list>
/// 
/// <para><b>Configuration Registration:</b></para>
/// <para>The configuration is typically registered during application startup using the
/// Options pattern with dependency injection, allowing for strongly-typed access
/// to configuration values throughout the application lifecycle.</para>
/// 
/// <para><b>Section Binding:</b></para>
/// <para>This class expects to be bound to a configuration section named 
/// "PvNugsSecretManagerEnvVariablesConfig". The section name is defined by
/// the <see cref="Section"/> constant to ensure consistency and prevent configuration errors.</para>
/// 
/// <para><b>Thread Safety:</b></para>
/// <para>This configuration class is thread-safe for read operations once configured during application startup.
/// The properties are typically set once during the configuration binding phase and then
/// accessed read-only throughout the application lifecycle.</para>
/// 
/// <para><b>Validation:</b></para>
/// <para>No built-in validation is performed on configuration values. Applications should implement
/// appropriate validation logic if needed, particularly for the <see cref="Prefix"/> property.</para>
/// </remarks>
/// <example>
/// <para><b>Dependency Injection Registration:</b></para>
/// <code>
/// // Register configuration in Program.cs or Startup.cs
/// builder.Services.Configure&lt;PvNugsSecretManagerEnvVariablesConfig&gt;(
///     builder.Configuration.GetSection(PvNugsSecretManagerEnvVariablesConfig.Section));
/// 
/// // Alternative registration with inline configuration
/// builder.Services.Configure&lt;PvNugsSecretManagerEnvVariablesConfig&gt;(options =&gt;
/// {
///     options.Prefix = "MyApplication";
/// });
/// </code>
/// 
/// <para><b>Configuration Examples:</b></para>
/// <code>
/// // appsettings.json configuration:
/// {
///   "PvNugsSecretManagerEnvVariablesConfig": {
///     "Prefix": "MyApp"
///   }
/// }
/// 
/// // Environment variable configuration:
/// // PvNugsSecretManagerEnvVariablesConfig__Prefix=MyApp
/// 
/// // Development override in appsettings.Development.json:
/// {
///   "PvNugsSecretManagerEnvVariablesConfig": {
///     "Prefix": "MyApp_Dev"
///   }
/// }
/// </code>
/// 
/// <para><b>Usage in Service Classes:</b></para>
/// <code>
/// public class SecretService
/// {
///     private readonly PvNugsSecretManagerEnvVariablesConfig _config;
///     private readonly ILogger&lt;SecretService&gt; _logger;
///     
///     public SecretService(
///         IOptions&lt;PvNugsSecretManagerEnvVariablesConfig&gt; options,
///         ILogger&lt;SecretService&gt; logger)
///     {
///         _config = options.Value;
///         _logger = logger;
///     }
///     
///     public void LogConfiguration()
///     {
///         var prefix = _config.Prefix ?? "No prefix configured";
///         _logger.LogInformation("Secret manager using prefix: {Prefix}", prefix);
///     }
///     
///     public string GetEffectiveSecretPath(string secretName)
///     {
///         if (string.IsNullOrEmpty(_config.Prefix))
///         {
///             return secretName;
///         }
///         return $"{_config.Prefix}__{secretName}";
///     }
/// }
/// </code>
/// </example>
public class PvNugsEnvVarSecretProviderConfig
{
    /// <summary>
    /// Defines the configuration section name used to bind this configuration class from configuration sources.
    /// This constant ensures consistency and provides compile-time safety when referencing the section name.
    /// </summary>
    /// <value>
    /// The string "PvNugsSecretManagerEnvVariablesConfig", which corresponds to the class name
    /// and serves as the root section key in configuration hierarchies.
    /// </value>
    /// <remarks>
    /// <para><b>Consistency Benefits:</b></para>
    /// <para>This constant provides several advantages:</para>
    /// <list type="bullet">
    /// <item><description><b>Compile-time Safety:</b> Using <see langword="nameof"/> ensures automatic updates if the class is renamed</description></item>
    /// <item><description><b>Refactoring Support:</b> IDE refactoring operations will automatically update all references</description></item>
    /// <item><description><b>Typo Prevention:</b> Eliminates string literal typos in configuration binding code</description></item>
    /// <item><description><b>Documentation Alignment:</b> Keeps documentation examples consistent with actual usage</description></item>
    /// </list>
    /// 
    /// <para><b>Usage Scenarios:</b></para>
    /// <para>The section name is used in various configuration scenarios:</para>
    /// <list type="bullet">
    /// <item><description>Binding configuration from appsettings.json hierarchical sections</description></item>
    /// <item><description>Reading environment variables with double-underscore hierarchical keys</description></item>
    /// <item><description>Configuring options in dependency injection containers</description></item>
    /// <item><description>Validating configuration sections during application startup</description></item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Use the constant for consistent section binding
    /// services.Configure&lt;PvNugsSecretManagerEnvVariablesConfig&gt;(
    ///     configuration.GetSection(PvNugsSecretManagerEnvVariablesConfig.Section));
    /// 
    /// // This is equivalent to, but safer than:
    /// services.Configure&lt;PvNugsSecretManagerEnvVariablesConfig&gt;(
    ///     configuration.GetSection("PvNugsSecretManagerEnvVariablesConfig"));
    /// 
    /// // Configuration validation using the constant
    /// if (!configuration.GetSection(PvNugsSecretManagerEnvVariablesConfig.Section).Exists())
    /// {
    ///     throw new InvalidOperationException("Required configuration section missing");
    /// }
    /// </code>
    /// </example>
    public const string Section = nameof(PvNugsEnvVarSecretProviderConfig);
    
    /// <summary>
    /// Gets or sets the optional prefix used to organize secrets within the configuration hierarchy.
    /// When specified, this prefix creates a logical namespace for grouping related secrets,
    /// enabling better organization in multi-environment or multi-tenant scenarios.
    /// </summary>
    /// <value>
    /// A string representing the prefix name, or <c>null</c> if no prefix is configured.
    /// When <c>null</c> or empty, secrets are accessed directly from the root configuration level.
    /// </value>
    /// <remarks>
    /// <para><b>Organizational Benefits:</b></para>
    /// <para>The prefix serves as a namespace mechanism that provides several organizational benefits:</para>
    /// <list type="bullet">
    /// <item><description><b>Multi-Environment Support:</b> Separate secrets for development, staging, and production environments</description></item>
    /// <item><description><b>Multi-Tenant Applications:</b> Isolate secrets by tenant or customer organization</description></item>
    /// <item><description><b>Feature-Based Organization:</b> Group secrets by application feature or microservice</description></item>
    /// <item><description><b>Team-Based Isolation:</b> Separate secrets by development team or application component</description></item>
    /// <item><description><b>Container Deployment:</b> Environment-specific secret injection in containerized deployments</description></item>
    /// </list>
    /// 
    /// <para><b>Configuration Resolution:</b></para>
    /// <list type="bullet">
    /// <item><description><b>Without prefix (null/empty):</b> Secrets retrieved directly from root configuration (e.g., "DatabasePassword")</description></item>
    /// <item><description><b>With prefix specified:</b> Secrets retrieved from prefixed section (e.g., "MyApp" section containing "DatabasePassword")</description></item>
    /// </list>
    /// 
    /// <para><b>Environment Variable Patterns:</b></para>
    /// <list type="bullet">
    /// <item><description><b>No prefix:</b> <c>DatabasePassword=secretValue</c></description></item>
    /// <item><description><b>With prefix "MyApp":</b> <c>MyApp__DatabasePassword=secretValue</c></description></item>
    /// <item><description><b>Dynamic credentials with prefix:</b> <c>MyApp__ServiceName__username=user123</c></description></item>
    /// </list>
    /// 
    /// <para><b>JSON Configuration Patterns:</b></para>
    /// <list type="bullet">
    /// <item><description><b>No prefix:</b> <c>{ "DatabasePassword": "secretValue" }</c></description></item>
    /// <item><description><b>With prefix:</b> <c>{ "MyApp": { "DatabasePassword": "secretValue" } }</c></description></item>
    /// </list>
    /// 
    /// <para><b>Validation and Constraints:</b></para>
    /// <para>The prefix value is not validated by this configuration class. Consider the following guidelines:</para>
    /// <list type="bullet">
    /// <item><description><b>Recommended Characters:</b> Use alphanumeric characters, underscores, and hyphens for broad compatibility</description></item>
    /// <item><description><b>Avoid Special Characters:</b> Some configuration providers may have restrictions on certain characters</description></item>
    /// <item><description><b>Case Sensitivity:</b> Be aware that some configuration sources are case-sensitive while others are not</description></item>
    /// <item><description><b>Length Considerations:</b> Very long prefixes may hit path length limitations in some environments</description></item>
    /// </list>
    /// 
    /// <para><b>Best Practices:</b></para>
    /// <list type="bullet">
    /// <item><description><b>Consistency:</b> Use the same prefix format across all environments for a given application</description></item>
    /// <item><description><b>Descriptive Names:</b> Choose prefixes that clearly indicate the application, environment, or tenant</description></item>
    /// <item><description><b>Hierarchical Organization:</b> Consider using hierarchical prefixes like "MyApp_Prod" or "Tenant1_Services"</description></item>
    /// <item><description><b>Documentation:</b> Document prefix conventions for development teams</description></item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <para><b>Configuration without prefix:</b></para>
    /// <code>
    /// // Configuration instance
    /// var config = new PvNugsSecretManagerEnvVariablesConfig 
    /// { 
    ///     Prefix = null 
    /// };
    /// 
    /// // Environment variables (direct access):
    /// // DatabasePassword=mySecret123
    /// // ApiKey=abc-def-ghi
    /// 
    /// // JSON configuration (root level):
    /// {
    ///   "DatabasePassword": "mySecret123",
    ///   "ApiKey": "abc-def-ghi"
    /// }
    /// 
    /// // Retrieval pattern: configuration["DatabasePassword"]
    /// </code>
    /// 
    /// <para><b>Configuration with prefix:</b></para>
    /// <code>
    /// // Configuration instance
    /// var config = new PvNugsSecretManagerEnvVariablesConfig 
    /// { 
    ///     Prefix = "Production" 
    /// };
    /// 
    /// // Environment variables (prefixed access):
    /// // Production__DatabasePassword=mySecret123
    /// // Production__ApiKey=abc-def-ghi
    /// 
    /// // JSON configuration (sectioned):
    /// {
    ///   "Production": {
    ///     "DatabasePassword": "mySecret123",
    ///     "ApiKey": "abc-def-ghi"
    ///   }
    /// }
    /// 
    /// // Retrieval pattern: configuration.GetSection("Production")["DatabasePassword"]
    /// </code>
    /// 
    /// <para><b>Multi-environment example:</b></para>
    /// <code>
    /// // Development environment
    /// {
    ///   "PvNugsSecretManagerEnvVariablesConfig": {
    ///     "Prefix": "MyApp_Dev"
    ///   },
    ///   "MyApp_Dev": {
    ///     "DatabasePassword": "dev_password_123",
    ///     "ExternalApiKey": "dev_api_key_456"
    ///   }
    /// }
    /// 
    /// // Production environment  
    /// {
    ///   "PvNugsSecretManagerEnvVariablesConfig": {
    ///     "Prefix": "MyApp_Prod"
    ///   },
    ///   "MyApp_Prod": {
    ///     "DatabasePassword": "prod_password_xyz",
    ///     "ExternalApiKey": "prod_api_key_789"
    ///   }
    /// }
    /// 
    /// // Usage remains the same - only configuration changes
    /// var secretManager = serviceProvider.GetService&lt;IStaticSecretManager&gt;();
    /// var dbPassword = await secretManager.GetStaticSecretAsync("DatabasePassword");
    /// </code>
    /// 
    /// <para><b>Dynamic credential organization:</b></para>
    /// <code>
    /// // Environment variables for dynamic credentials with prefix "MyService":
    /// // MyService__DatabasePrimary__username=temp_user_abc
    /// // MyService__DatabasePrimary__password=temp_pass_xyz
    /// // MyService__DatabasePrimary__expirationDateUtc=2024-12-31T23:59:59Z
    /// // MyService__CacheService__username=cache_user_def
    /// // MyService__CacheService__password=cache_pass_uvw
    /// // MyService__CacheService__expirationDateUtc=2024-06-30T23:59:59Z
    /// 
    /// // Configuration enables organized secret retrieval:
    /// var config = new PvNugsSecretManagerEnvVariablesConfig { Prefix = "MyService" };
    /// var dynamicSecretManager = new DynamicSecretManager(logger, Options.Create(config), configuration);
    /// 
    /// var dbCredential = await dynamicSecretManager.GetDynamicSecretAsync("DatabasePrimary");
    /// var cacheCredential = await dynamicSecretManager.GetDynamicSecretAsync("CacheService");
    /// </code>
    /// </example>
    public string? Prefix { get; set; }
}
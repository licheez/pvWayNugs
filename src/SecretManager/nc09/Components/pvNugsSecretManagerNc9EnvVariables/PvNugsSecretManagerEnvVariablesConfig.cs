namespace pvNugsSecretManagerNc9EnvVariables;

/// <summary>
/// Provides configuration settings for the pvNugs Secret Manager environment variables implementation.
/// This configuration class defines options for organizing and accessing secrets stored in 
/// environment variables or other configuration sources within the Microsoft Configuration system.
/// </summary>
/// <remarks>
/// <para><strong>Configuration Sources:</strong></para>
/// <para>This configuration class is designed to work with the Microsoft.Extensions.Configuration
/// system and supports binding from various configuration sources including:</para>
/// <list type="bullet">
/// <item><description><strong>Environment variables:</strong> Standard and prefixed environment variable patterns</description></item>
/// <item><description><strong>JSON files:</strong> appsettings.json and environment-specific configuration files</description></item>
/// <item><description><strong>Azure Key Vault:</strong> When configured as a configuration provider (not for secret generation)</description></item>
/// <item><description><strong>Command line arguments:</strong> Runtime configuration overrides</description></item>
/// <item><description><strong>In-memory collections:</strong> Programmatic configuration for testing scenarios</description></item>
/// <item><description><strong>Custom providers:</strong> Any IConfiguration provider implementation</description></item>
/// </list>
/// 
/// <para><strong>Configuration Registration:</strong></para>
/// <para>The configuration is typically registered during application startup using the
/// Options pattern with dependency injection, allowing for strongly-typed access
/// to configuration values throughout the application lifecycle.</para>
/// 
/// <para><strong>Section Binding:</strong></para>
/// <para>This class expects to be bound to a configuration section named 
/// "PvNugsSecretManagerEnvVariablesConfig". The section name is defined by
/// the <see cref="Section"/> constant to ensure consistency and prevent configuration errors.</para>
/// 
/// <para><strong>Thread Safety:</strong></para>
/// <para>This configuration class is thread-safe for read operations once configured during application startup.
/// The properties are typically set once during the configuration binding phase and then
/// accessed read-only throughout the application lifecycle.</para>
/// 
/// <para><strong>Validation:</strong></para>
/// <para>No built-in validation is performed on configuration values. Applications should implement
/// appropriate validation logic if needed, particularly for the <see cref="Prefix"/> property.</para>
/// </remarks>
/// <example>
/// <para><strong>Dependency Injection Registration:</strong></para>
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
/// <para><strong>Configuration Examples:</strong></para>
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
/// <para><strong>Usage in Service Classes:</strong></para>
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
public class PvNugsSecretManagerEnvVariablesConfig
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
    /// <para><strong>Consistency Benefits:</strong></para>
    /// <para>This constant provides several advantages:</para>
    /// <list type="bullet">
    /// <item><description><strong>Compile-time Safety:</strong> Using <see langword="nameof"/> ensures automatic updates if the class is renamed</description></item>
    /// <item><description><strong>Refactoring Support:</strong> IDE refactoring operations will automatically update all references</description></item>
    /// <item><description><strong>Typo Prevention:</strong> Eliminates string literal typos in configuration binding code</description></item>
    /// <item><description><strong>Documentation Alignment:</strong> Keeps documentation examples consistent with actual usage</description></item>
    /// </list>
    /// 
    /// <para><strong>Usage Scenarios:</strong></para>
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
    public const string Section = nameof(PvNugsSecretManagerEnvVariablesConfig);
    
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
    /// <para><strong>Organizational Benefits:</strong></para>
    /// <para>The prefix serves as a namespace mechanism that provides several organizational benefits:</para>
    /// <list type="bullet">
    /// <item><description><strong>Multi-Environment Support:</strong> Separate secrets for development, staging, and production environments</description></item>
    /// <item><description><strong>Multi-Tenant Applications:</strong> Isolate secrets by tenant or customer organization</description></item>
    /// <item><description><strong>Feature-Based Organization:</strong> Group secrets by application feature or microservice</description></item>
    /// <item><description><strong>Team-Based Isolation:</strong> Separate secrets by development team or application component</description></item>
    /// <item><description><strong>Container Deployment:</strong> Environment-specific secret injection in containerized deployments</description></item>
    /// </list>
    /// 
    /// <para><strong>Configuration Resolution:</strong></para>
    /// <list type="bullet">
    /// <item><description><strong>Without prefix (null/empty):</strong> Secrets retrieved directly from root configuration (e.g., "DatabasePassword")</description></item>
    /// <item><description><strong>With prefix specified:</strong> Secrets retrieved from prefixed section (e.g., "MyApp" section containing "DatabasePassword")</description></item>
    /// </list>
    /// 
    /// <para><strong>Environment Variable Patterns:</strong></para>
    /// <list type="bullet">
    /// <item><description><strong>No prefix:</strong> <c>DatabasePassword=secretValue</c></description></item>
    /// <item><description><strong>With prefix "MyApp":</strong> <c>MyApp__DatabasePassword=secretValue</c></description></item>
    /// <item><description><strong>Dynamic credentials with prefix:</strong> <c>MyApp__ServiceName__username=user123</c></description></item>
    /// </list>
    /// 
    /// <para><strong>JSON Configuration Patterns:</strong></para>
    /// <list type="bullet">
    /// <item><description><strong>No prefix:</strong> <c>{ "DatabasePassword": "secretValue" }</c></description></item>
    /// <item><description><strong>With prefix:</strong> <c>{ "MyApp": { "DatabasePassword": "secretValue" } }</c></description></item>
    /// </list>
    /// 
    /// <para><strong>Validation and Constraints:</strong></para>
    /// <para>The prefix value is not validated by this configuration class. Consider the following guidelines:</para>
    /// <list type="bullet">
    /// <item><description><strong>Recommended Characters:</strong> Use alphanumeric characters, underscores, and hyphens for broad compatibility</description></item>
    /// <item><description><strong>Avoid Special Characters:</strong> Some configuration providers may have restrictions on certain characters</description></item>
    /// <item><description><strong>Case Sensitivity:</strong> Be aware that some configuration sources are case-sensitive while others are not</description></item>
    /// <item><description><strong>Length Considerations:</strong> Very long prefixes may hit path length limitations in some environments</description></item>
    /// </list>
    /// 
    /// <para><strong>Best Practices:</strong></para>
    /// <list type="bullet">
    /// <item><description><strong>Consistency:</strong> Use the same prefix format across all environments for a given application</description></item>
    /// <item><description><strong>Descriptive Names:</strong> Choose prefixes that clearly indicate the application, environment, or tenant</description></item>
    /// <item><description><strong>Hierarchical Organization:</strong> Consider using hierarchical prefixes like "MyApp_Prod" or "Tenant1_Services"</description></item>
    /// <item><description><strong>Documentation:</strong> Document prefix conventions for development teams</description></item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <para><strong>Configuration without prefix:</strong></para>
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
    /// <para><strong>Configuration with prefix:</strong></para>
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
    /// <para><strong>Multi-environment example:</strong></para>
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
    /// <para><strong>Dynamic credential organization:</strong></para>
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
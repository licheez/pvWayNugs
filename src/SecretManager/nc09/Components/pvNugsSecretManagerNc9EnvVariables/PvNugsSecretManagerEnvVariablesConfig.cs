namespace pvNugsSecretManagerNc9EnvVariables;

/// <summary>
/// Provides configuration settings for the pvNugs Secret Manager environment variables implementation.
/// This configuration class defines options for organizing and accessing secrets stored in 
/// environment variables or other configuration sources.
/// </summary>
/// <remarks>
/// <para>
/// This configuration class is designed to work with the Microsoft.Extensions.Configuration
/// system and supports binding from various configuration sources including:
/// </para>
/// <list type="bullet">
/// <item>Environment variables</item>
/// <item>appsettings.json files</item>
/// <item>Azure Key Vault (when configured as a configuration provider)</item>
/// <item>Command line arguments</item>
/// <item>In-memory collections</item>
/// <item>Other IConfiguration providers</item>
/// </list>
/// <para>
/// The configuration is typically registered during application startup using the
/// Options pattern with dependency injection, allowing for strongly-typed access
/// to configuration values throughout the application.
/// </para>
/// <para>
/// <strong>Configuration Section:</strong>
/// </para>
/// <para>
/// This class expects to be bound to a configuration section named 
/// "PvNugsSecretManagerEnvVariablesConfig". The section name is defined by
/// the <see cref="Section"/> constant to ensure consistency and prevent typos.
/// </para>
/// <para>
/// <strong>Thread Safety:</strong>
/// </para>
/// <para>
/// This configuration class is thread-safe for read operations once configured.
/// The properties are typically set once during application startup and then
/// accessed read-only throughout the application lifecycle.
/// </para>
/// </remarks>
/// <example>
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
/// 
/// // Example appsettings.json configuration:
/// // {
/// //   "PvNugsSecretManagerEnvVariablesConfig": {
/// //     "Prefix": "MyApp"
/// //   }
/// // }
/// 
/// // Example environment variable configuration:
/// // PvNugsSecretManagerEnvVariablesConfig__Prefix=MyApp
/// 
/// // Usage in a service class
/// public class MyService
/// {
///     private readonly PvNugsSecretManagerEnvVariablesConfig _config;
///     
///     public MyService(IOptions&lt;PvNugsSecretManagerEnvVariablesConfig&gt; options)
///     {
///         _config = options.Value;
///     }
///     
///     public void DoSomething()
///     {
///         var prefix = _config.Prefix;
///         if (!string.IsNullOrEmpty(prefix))
///         {
///             // Use the configured prefix for secret organization
///             Console.WriteLine($"Using prefix: {prefix}");
///         }
///     }
/// }
/// </code>
/// </example>
public class PvNugsSecretManagerEnvVariablesConfig
{
    /// <summary>
    /// Defines the configuration section name used to bind this configuration class.
    /// </summary>
    /// <value>
    /// The string "PvNugsSecretManagerEnvVariablesConfig", which corresponds to the class name.
    /// </value>
    /// <remarks>
    /// <para>
    /// This constant is used to ensure consistency when binding the configuration class
    /// to a specific section in the configuration hierarchy. Using <see langword="nameof"/>
    /// provides compile-time safety and automatic refactoring support.
    /// </para>
    /// <para>
    /// The section name is used in scenarios such as:
    /// </para>
    /// <list type="bullet">
    /// <item>Binding configuration from appsettings.json</item>
    /// <item>Reading environment variables with hierarchical keys</item>
    /// <item>Configuring options in dependency injection</item>
    /// <item>Validating configuration sections</item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Use the constant for consistent section binding
    /// services.Configure&lt;PvNugsSecretManagerEnvVariablesConfig&gt;(
    ///     configuration.GetSection(PvNugsSecretManagerEnvVariablesConfig.Section));
    /// 
    /// // Equivalent to:
    /// services.Configure&lt;PvNugsSecretManagerEnvVariablesConfig&gt;(
    ///     configuration.GetSection("PvNugsSecretManagerEnvVariablesConfig"));
    /// </code>
    /// </example>
    public const string Section = nameof(PvNugsSecretManagerEnvVariablesConfig);
    
    /// <summary>
    /// Gets or sets the optional prefix used to organize secrets within the configuration hierarchy.
    /// When specified, this prefix is used to create configuration sections for logical grouping of related secrets.
    /// </summary>
    /// <value>
    /// A string representing the prefix name, or <c>null</c> if no prefix is configured.
    /// When <c>null</c> or empty, secrets are accessed directly from the root configuration.
    /// </value>
    /// <remarks>
    /// <para>
    /// The prefix serves as a namespace mechanism for organizing secrets, which is particularly
    /// useful in scenarios such as:
    /// </para>
    /// <list type="bullet">
    /// <item>Multi-tenant applications where secrets need organization by tenant</item>
    /// <item>Applications with multiple environments (dev, staging, production)</item>
    /// <item>Large applications with secrets organized by feature or module</item>
    /// <item>Containerized deployments with environment-specific secret injection</item>
    /// </list>
    /// <para>
    /// <strong>How the prefix works:</strong>
    /// </para>
    /// <list type="bullet">
    /// <item>When prefix is null or empty: Secrets are retrieved directly (e.g., "DatabasePassword")</item>
    /// <item>When prefix is specified: Secrets are retrieved from a section (e.g., "MyApp" section containing "DatabasePassword")</item>
    /// </list>
    /// <para>
    /// <strong>Configuration Examples:</strong>
    /// </para>
    /// <para>
    /// <strong>Without prefix (Prefix = null):</strong>
    /// </para>
    /// <list type="bullet">
    /// <item>Environment variable: <c>DatabasePassword=secret123</c></item>
    /// <item>appsettings.json: <c>{ "DatabasePassword": "secret123" }</c></item>
    /// </list>
    /// <para>
    /// <strong>With prefix (Prefix = "MyApp"):</strong>
    /// </para>
    /// <list type="bullet">
    /// <item>Environment variable: <c>MyApp__DatabasePassword=secret123</c></item>
    /// <item>appsettings.json: <c>{ "MyApp": { "DatabasePassword": "secret123" } }</c></item>
    /// </list>
    /// <para>
    /// <strong>Validation:</strong>
    /// </para>
    /// <para>
    /// No validation is performed on the prefix value. It can contain any characters
    /// valid for configuration keys in the underlying configuration provider.
    /// However, it's recommended to use alphanumeric characters and avoid special
    /// characters that might have meaning in specific configuration providers.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Configuration without prefix - secrets at root level
    /// var config1 = new PvNugsSecretManagerEnvVariablesConfig 
    /// { 
    ///     Prefix = null 
    /// };
    /// // Retrieves: configuration["DatabasePassword"]
    /// 
    /// // Configuration with prefix - secrets organized in sections
    /// var config2 = new PvNugsSecretManagerEnvVariablesConfig 
    /// { 
    ///     Prefix = "Production" 
    /// };
    /// // Retrieves: configuration.GetSection("Production")["DatabasePassword"]
    /// 
    /// // Environment variable examples:
    /// // Without prefix: DatabasePassword=mySecret
    /// // With prefix: Production__DatabasePassword=mySecret
    /// 
    /// // appsettings.json examples:
    /// // Without prefix:
    /// // {
    /// //   "DatabasePassword": "mySecret"
    /// // }
    /// // 
    /// // With prefix:
    /// // {
    /// //   "Production": {
    /// //     "DatabasePassword": "mySecret"
    /// //   }
    /// // }
    /// </code>
    /// </example>
    public string? Prefix { get; set; }
}
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using pvNugsSecretManagerNc6Abstractions;

namespace pvNugsSecretManagerNc6Azure;

/// <summary>
/// Provides dependency injection extension methods for registering Azure Key Vault-based secret management services
/// in the application's service collection. This static class encapsulates the registration logic for the
/// pvNugsSecretManagerNc9Azure package, enabling seamless integration with .NET's dependency injection container.
/// </summary>
/// <remarks>
/// <para><c>Purpose and Design:</c></para>
/// <para>This class follows the standard .NET extension method pattern for service registration, providing a clean
/// and discoverable API for configuring Azure Key Vault secret management services. It handles both configuration
/// binding and service registration in a single, atomic operation to ensure proper dependency setup.</para>
/// 
/// <para><c>Integration Benefits:</c></para>
/// <list type="bullet">
/// <item><description><c>Configuration Binding:</c> Automatically binds Azure Key Vault configuration from IConfiguration sources</description></item>
/// <item><description><c>Service Registration:</c> Registers the secret manager implementation with proper lifetime management</description></item>
/// <item><description><c>Dependency Resolution:</c> Ensures all required dependencies are properly configured for injection</description></item>
/// <item><description><c>Type Safety:</c> Provides compile-time safety through strongly typed configuration and service interfaces</description></item>
/// </list>
/// 
/// <para><c>Configuration Integration:</c></para>
/// <para>The extension method integrates with .NET's IConfiguration system to support multiple configuration sources:</para>
/// <list type="bullet">
/// <item><description>JSON configuration files (appsettings.json, appsettings.{Environment}.json)</description></item>
/// <item><description>Environment variables with hierarchical naming convention</description></item>
/// <item><description>Azure App Configuration service for centralized configuration management</description></item>
/// <item><description>Command line arguments for deployment-time configuration overrides</description></item>
/// <item><description>In-memory configuration providers for testing and development scenarios</description></item>
/// </list>
/// 
/// <para><c>Service Lifetime Strategy:</c></para>
/// <para>The registered services use singleton lifetime management for several important reasons:</para>
/// <list type="bullet">
/// <item><description><c>Performance Optimization:</c> Avoids repeated Azure Key Vault client initialization overhead</description></item>
/// <item><description><c>Connection Pooling:</c> Enables efficient HTTP connection reuse for Azure API calls</description></item>
/// <item><description><c>Caching Benefits:</c> Allows the secret manager to maintain internal caches across requests</description></item>
/// <item><description><c>Authentication Efficiency:</c> Reduces authentication token acquisition overhead</description></item>
/// </list>
/// 
/// <para><c>Dependency Requirements:</c></para>
/// <para>For successful operation, the following dependencies must be registered separately:</para>
/// <list type="bullet">
/// <item><description><c>ILoggerService:</c> Required for comprehensive logging of secret management operations</description></item>
/// <item><description><c>IPvNugsCache:</c> Required for caching retrieved secrets to improve performance</description></item>
/// <item><description><c>IConfiguration:</c> Automatically available in .NET applications for configuration binding</description></item>
/// </list>
/// 
/// <para><c>Thread Safety and Concurrency:</c></para>
/// <para>The registered singleton services are designed to handle concurrent access safely:</para>
/// <list type="bullet">
/// <item><description>Azure SDK clients are inherently thread-safe and support concurrent operations</description></item>
/// <item><description>The secret manager implementation uses thread-safe patterns for lazy initialization</description></item>
/// <item><description>Configuration objects are immutable after binding, ensuring safe concurrent access</description></item>
/// </list>
/// 
/// <para><c>Extension Method Patterns:</c></para>
/// <para>This class follows established .NET patterns for dependency injection extensions:</para>
/// <list type="bullet">
/// <item><description><c>TryAdd Pattern:</c> Uses TryAddSingleton to avoid duplicate registrations</description></item>
/// <item><description><c>Fluent Interface:</c> Returns IServiceCollection for method chaining</description></item>
/// <item><description><c>Configuration Binding:</c> Uses the Options pattern for strongly typed configuration</description></item>
/// <item><description><c>Descriptive Naming:</c> Method names clearly indicate the functionality being registered</description></item>
/// </list>
/// </remarks>
/// <example>
/// <para>Basic service registration in Program.cs:</para>
/// <code>
/// var builder = WebApplication.CreateBuilder(args);
/// 
/// // Register Azure Key Vault secret management services
/// builder.Services.TryAddPvNugsAzureStaticSecretManager(builder.Configuration);
/// 
/// var app = builder.Build();
/// </code>
/// 
/// <para>Complete registration with all dependencies:</para>
/// <code>
/// var builder = WebApplication.CreateBuilder(args);
/// 
/// // Register required dependencies first
/// builder.Services.TryAddPvNugsCacheNc9Local(builder.Configuration);
/// builder.Services.TryAddPvNugsLoggerSeriService(builder.Configuration);
/// 
/// // Register Azure Key Vault secret manager
/// builder.Services.TryAddPvNugsAzureStaticSecretManager(builder.Configuration);
/// 
/// var app = builder.Build();
/// </code>
/// 
/// <para>Usage in application service:</para>
/// <code>
/// public class DatabaseService
/// {
///     private readonly IPvNugsStaticSecretManager _secretManager;
///     
///     public DatabaseService(IPvNugsStaticSecretManager secretManager)
///     {
///         _secretManager = secretManager;
///     }
///     
///     public async Task&lt;string&gt; GetConnectionStringAsync()
///     {
///         var password = await _secretManager.GetStaticSecretAsync("database-password");
///         return $"Server=myserver;Database=mydb;Password={password};";
///     }
/// }
/// </code>
/// 
/// <para>Configuration file example (appsettings.json):</para>
/// <code>
/// {
///   "PvNugsAzureSecretManagerConfig": {
///     "KeyVaultUrl": "https://myapp-keyvault.vault.azure.net/",
///     "Credential": null
///   }
/// }
/// </code>
/// 
/// <para>Environment-specific registration:</para>
/// <code>
/// var builder = WebApplication.CreateBuilder(args);
/// 
/// if (builder.Environment.IsProduction())
/// {
///     // Production: Use managed identity with production Key Vault
///     builder.Services.Configure&lt;PvNugsAzureSecretManagerConfig&gt;(options =&gt;
///     {
///         options.KeyVaultUrl = builder.Configuration["KeyVault:ProductionUrl"];
///         options.Credential = null; // Use managed identity
///     });
/// }
/// 
/// builder.Services.TryAddPvNugsAzureStaticSecretManager(builder.Configuration);
/// </code>
/// 
/// <para>Registration with validation:</para>
/// <code>
/// var builder = WebApplication.CreateBuilder(args);
/// 
/// builder.Services.TryAddPvNugsAzureStaticSecretManager(builder.Configuration);
/// 
/// // Add configuration validation
/// builder.Services.Configure&lt;PvNugsAzureSecretManagerConfig&gt;(options =&gt;
/// {
///     if (string.IsNullOrWhiteSpace(options.KeyVaultUrl))
///         throw new InvalidOperationException("Key Vault URL is required");
///         
///     if (!Uri.IsWellFormedUriString(options.KeyVaultUrl, UriKind.Absolute))
///         throw new ArgumentException("Key Vault URL must be a valid absolute URL");
/// });
/// </code>
/// </example>
/// <seealso cref="IPvNugsStaticSecretManager"/>
/// <seealso cref="PvNugsStaticSecretManager"/>
/// <seealso cref="PvNugsAzureSecretManagerConfig"/>
/// <seealso cref="IServiceCollection"/>
/// <seealso href="https://docs.microsoft.com/en-us/aspnet/core/fundamentals/dependency-injection">ASP.NET Core Dependency Injection</seealso>
public static class PvNugsStaticSecretManagerAzureDi
{
    /// <summary>
    /// Registers the Azure Key Vault-based static secret manager implementation and its configuration
    /// with the dependency injection container. This method provides a complete setup for Azure Key Vault
    /// integration, including configuration binding and service registration with appropriate lifetime management.
    /// </summary>
    /// <param name="services">
    /// The <see cref="IServiceCollection"/> to which the Azure Key Vault secret management services will be added.
    /// This parameter represents the application's service container and is used to register both the configuration
    /// options and the secret manager implementation.
    /// </param>
    /// <param name="config">
    /// The <see cref="IConfiguration"/> instance containing the application's configuration data.
    /// This configuration source is used to bind the Azure Key Vault settings from various configuration providers
    /// such as appsettings.json, environment variables, Azure App Configuration, or other IConfiguration sources.
    /// </param>
    /// <returns>
    /// The same <see cref="IServiceCollection"/> instance that was passed in, enabling fluent method chaining
    /// with other service registration calls. This follows the standard builder pattern used throughout
    /// .NET dependency injection extensions.
    /// </returns>
    /// <remarks>
    /// <para><c>Registration Operations:</c></para>
    /// <para>This method performs two essential registration operations:</para>
    /// <list type="number">
    /// <item><description><c>Configuration Binding:</c> Binds the <see cref="PvNugsAzureSecretManagerConfig"/> from the configuration section specified by <see cref="PvNugsAzureSecretManagerConfig.Section"/></description></item>
    /// <item><description><c>Service Registration:</c> Registers <see cref="PvNugsStaticSecretManager"/> as a singleton implementation of <see cref="IPvNugsStaticSecretManager"/></description></item>
    /// </list>
    /// 
    /// <para><c>Configuration Section Binding:</c></para>
    /// <para>The method automatically binds configuration from the section named by <see cref="PvNugsAzureSecretManagerConfig.Section"/>.
    /// This enables the configuration to be sourced from multiple providers in the standard .NET configuration hierarchy:</para>
    /// <list type="bullet">
    /// <item><description>Base configuration files (appsettings.json)</description></item>
    /// <item><description>Environment-specific files (appsettings.Development.json, appsettings.Production.json)</description></item>
    /// <item><description>Environment variables using the double-underscore naming convention</description></item>
    /// <item><description>Azure App Configuration for centralized, feature-flagged configuration</description></item>
    /// <item><description>Command-line arguments for deployment-time overrides</description></item>
    /// </list>
    /// 
    /// <para><c>Service Lifetime Management:</c></para>
    /// <para>The secret manager is registered as a singleton for several important reasons:</para>
    /// <list type="bullet">
    /// <item><description><c>Performance:</c> Avoids the overhead of creating new Azure Key Vault clients for each request</description></item>
    /// <item><description><c>Connection Efficiency:</c> Enables HTTP connection pooling and reuse for Azure API calls</description></item>
    /// <item><description><c>Caching:</c> Allows the implementation to maintain internal caches for improved performance</description></item>
    /// <item><description><c>Authentication:</c> Reduces token acquisition overhead by reusing authentication contexts</description></item>
    /// <item><description><c>Resource Management:</c> Ensures proper disposal and cleanup of Azure SDK resources</description></item>
    /// </list>
    /// 
    /// <para><c>TryAdd Pattern Benefits:</c></para>
    /// <para>The method uses ServiceCollection.TryAddSingleton{TService,TImplementation}
    /// which provides several advantages:</para>
    /// <list type="bullet">
    /// <item><description><c>Duplicate Protection:</c> Prevents duplicate service registrations if called multiple times</description></item>
    /// <item><description><c>Override Capability:</c> Allows pre-existing registrations to take precedence (useful for testing)</description></item>
    /// <item><description><c>Composition Safety:</c> Enables safe usage in library scenarios where multiple packages might register the same services</description></item>
    /// <item><description><c>Configuration Flexibility:</c> Supports scenarios where different implementations might be registered conditionally</description></item>
    /// </list>
    /// 
    /// <para><c>Dependency Requirements:</c></para>
    /// <para>For the registered secret manager to function correctly, the following services must also be registered:</para>
    /// <list type="bullet">
    /// <item><description><c>ILoggerService:</c> Must be registered for logging secret management operations and errors</description></item>
    /// <item><description><c>IPvNugsCache:</c> Must be registered for caching retrieved secrets to improve performance</description></item>
    /// <item><description><c>IConfiguration:</c> Automatically available in .NET applications, but must contain the required configuration section</description></item>
    /// </list>
    /// 
    /// <para><c>Configuration Validation:</c></para>
    /// <para>While this method handles the basic registration, applications should consider implementing configuration validation:</para>
    /// <list type="bullet">
    /// <item><description>Validate that the Key Vault URL is properly formatted and accessible</description></item>
    /// <item><description>Ensure that authentication credentials (if using service principal) are properly configured</description></item>
    /// <item><description>Verify that the specified Key Vault exists and is accessible from the application's network context</description></item>
    /// <item><description>Check that required permissions are granted to the authentication principal</description></item>
    /// </list>
    /// 
    /// <para><c>Error Handling:</c></para>
    /// <para>Configuration binding errors may occur if:</para>
    /// <list type="bullet">
    /// <item><description>The configuration section is missing or malformed</description></item>
    /// <item><description>Required configuration properties are not provided</description></item>
    /// <item><description>Configuration values are in incorrect formats (e.g., invalid URLs or GUIDs)</description></item>
    /// <item><description>Environment variables or other configuration sources are not accessible</description></item>
    /// </list>
    /// 
    /// <para><c>Testing Considerations:</c></para>
    /// <para>When testing applications that use this registration method:</para>
    /// <list type="bullet">
    /// <item><description>Use test-specific configuration to point to test Key Vault instances or mock implementations</description></item>
    /// <item><description>Consider registering mock implementations before calling this method to override the registration</description></item>
    /// <item><description>Ensure test environments have appropriate Key Vault access or use alternative test strategies</description></item>
    /// <item><description>Validate configuration binding with test-specific configuration sources</description></item>
    /// </list>
    /// </remarks>
    /// <exception cref="System.ArgumentNullException">
    /// Thrown when <paramref name="services"/> or <paramref name="config"/> is null.
    /// </exception>
    /// <exception cref="System.InvalidOperationException">
    /// May be thrown during configuration binding if the configuration section is malformed or contains invalid values.
    /// </exception>
    /// <exception cref="System.InvalidOperationException">
    /// May be thrown if required configuration sections or values are missing, or if service registration fails.
    /// </exception>
    /// <example>
    /// <para>Standard registration in a web application:</para>
    /// <code>
    /// var builder = WebApplication.CreateBuilder(args);
    /// 
    /// // Register dependencies
    /// builder.Services.TryAddPvNugsCacheNc9Local(builder.Configuration);
    /// builder.Services.TryAddPvNugsLoggerSeriService(builder.Configuration);
    /// 
    /// // Register Azure Key Vault secret manager
    /// builder.Services.TryAddPvNugsAzureStaticSecretManager(builder.Configuration);
    /// 
    /// var app = builder.Build();
    /// </code>
    /// 
    /// <para>Registration with environment-specific configuration:</para>
    /// <code>
    /// var builder = WebApplication.CreateBuilder(args);
    /// 
    /// // Pre-configure for specific environments
    /// if (builder.Environment.IsDevelopment())
    /// {
    ///     builder.Services.Configure&lt;PvNugsAzureSecretManagerConfig&gt;(options =&gt;
    ///     {
    ///         options.KeyVaultUrl = "https://dev-keyvault.vault.azure.net/";
    ///         options.Credential = new PvNugsAzureServicePrincipalCredential
    ///         {
    ///             TenantId = builder.Configuration["AzureAd:TenantId"],
    ///             ClientId = builder.Configuration["AzureAd:ClientId"],
    ///             ClientSecret = builder.Configuration["AzureAd:ClientSecret"]
    ///         };
    ///     });
    /// }
    /// 
    /// builder.Services.TryAddPvNugsAzureStaticSecretManager(builder.Configuration);
    /// </code>
    /// 
    /// <para>Registration with configuration validation:</para>
    /// <code>
    /// var builder = WebApplication.CreateBuilder(args);
    /// 
    /// builder.Services.TryAddPvNugsAzureStaticSecretManager(builder.Configuration);
    /// 
    /// // Add post-configuration validation
    /// builder.Services.PostConfigure&lt;PvNugsAzureSecretManagerConfig&gt;(options =&gt;
    /// {
    ///     if (string.IsNullOrWhiteSpace(options.KeyVaultUrl))
    ///         throw new InvalidOperationException("Azure Key Vault URL must be configured");
    ///         
    ///     if (!Uri.IsWellFormedUriString(options.KeyVaultUrl, UriKind.Absolute))
    ///         throw new ArgumentException("Azure Key Vault URL must be a valid absolute URL");
    /// });
    /// </code>
    /// 
    /// <para>Multiple service registration with method chaining:</para>
    /// <code>
    /// var builder = WebApplication.CreateBuilder(args);
    /// 
    /// builder.Services
    ///     .TryAddPvNugsCacheNc9Local(builder.Configuration)
    ///     .TryAddPvNugsLoggerSeriService(builder.Configuration)
    ///     .TryAddPvNugsAzureStaticSecretManager(builder.Configuration);
    /// 
    /// var app = builder.Build();
    /// </code>
    /// 
    /// <para>Conditional registration based on configuration:</para>
    /// <code>
    /// var builder = WebApplication.CreateBuilder(args);
    /// 
    /// var useAzureKeyVault = builder.Configuration.GetValue&lt;bool&gt;("Features:UseAzureKeyVault");
    /// if (useAzureKeyVault)
    /// {
    ///     builder.Services.TryAddPvNugsAzureStaticSecretManager(builder.Configuration);
    /// }
    /// else
    /// {
    ///     // Register alternative secret manager implementation
    ///     builder.Services.TryAddSingleton&lt;IPvNugsStaticSecretManager, FileBasedSecretManager&gt;();
    /// }
    /// </code>
    /// </example>
    /// <seealso cref="OptionsConfigurationServiceCollectionExtensions.Configure{TOptions}(IServiceCollection, IConfiguration)"/>
    /// <seealso cref="PvNugsAzureSecretManagerConfig"/>
    /// <seealso cref="PvNugsStaticSecretManager"/>
    /// <seealso cref="IPvNugsStaticSecretManager"/>
    public static IServiceCollection TryAddPvNugsAzureStaticSecretManager(
        this IServiceCollection services,
        IConfiguration config)
    {
        services.Configure<PvNugsAzureSecretManagerConfig>(
            config.GetSection(PvNugsAzureSecretManagerConfig.Section));
        
        services.TryAddSingleton<IPvNugsStaticSecretManager, PvNugsStaticSecretManager>();
        
        return services;
    }
}
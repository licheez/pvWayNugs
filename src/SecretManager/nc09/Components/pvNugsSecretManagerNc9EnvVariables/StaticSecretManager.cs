using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using pvNugsLoggerNc9Abstractions;
using pvNugsSecretManagerNc9Abstractions;

namespace pvNugsSecretManagerNc9EnvVariables;

/// <summary>
/// Provides static secret management functionality using configuration sources such as environment variables, 
/// appsettings.json, Azure Key Vault, or any other IConfiguration provider. This implementation serves as a 
/// bridge between configuration-based secret storage and the standardized secret management interface, 
/// enabling flexible secret retrieval across diverse deployment scenarios.
/// </summary>
/// <remarks>
/// <para><strong>Core Functionality:</strong></para>
/// <para>This implementation retrieves static secrets from any IConfiguration source and wraps them in the 
/// standardized pvNugs secret management interface. It provides a consistent API regardless of the underlying 
/// configuration provider, making it suitable for development, testing, and production environments where 
/// configuration-based secret management is preferred over dynamic secret generation.</para>
/// 
/// <para><strong>Supported Configuration Sources:</strong></para>
/// <list type="bullet">
/// <item><description>Environment variables (ideal for containerized deployments)</description></item>
/// <item><description>JSON configuration files (appsettings.json, appsettings.{Environment}.json)</description></item>
/// <item><description>Azure Key Vault (via configuration provider)</description></item>
/// <item><description>AWS Parameter Store/Secrets Manager (via configuration providers)</description></item>
/// <item><description>Command line arguments</description></item>
/// <item><description>Memory-based configuration</description></item>
/// <item><description>Custom configuration providers implementing IConfigurationProvider</description></item>
/// </list>
/// 
/// <para><strong>Secret Organization with Prefix Support:</strong></para>
/// <para>The implementation supports optional prefix configuration for organizing secrets into logical groups. 
/// This feature is particularly valuable in:</para>
/// <list type="bullet">
/// <item><description>Multi-tenant applications where secrets need tenant-specific organization</description></item>
/// <item><description>Microservice architectures requiring service-specific secret namespacing</description></item>
/// <item><description>Environment-specific secret management (dev, staging, production)</description></item>
/// <item><description>Role-based secret access patterns</description></item>
/// </list>
/// 
/// <para><strong>Error Handling and Logging Strategy:</strong></para>
/// <para>The class implements comprehensive error handling with the following characteristics:</para>
/// <list type="bullet">
/// <item><description>Input validation with immediate ArgumentException for invalid usage</description></item>
/// <item><description>Asynchronous logging of all exceptions before wrapping and re-throwing</description></item>
/// <item><description>Consistent exception wrapping in PvNugsSecretManagerException for unified error handling</description></item>
/// <item><description>Detailed error messages with context for troubleshooting</description></item>
/// <item><description>Preservation of original exception details through proper exception chaining</description></item>
/// </list>
/// 
/// <para><strong>Performance Characteristics:</strong></para>
/// <list type="bullet">
/// <item><description>Fast configuration-based lookups (typically cached by configuration system)</description></item>
/// <item><description>Thread-safe read operations on immutable configuration data</description></item>
/// <item><description>Minimal memory overhead with on-demand secret retrieval</description></item>
/// <item><description>No network I/O for local configuration sources</description></item>
/// <item><description>Efficient section-based organization with lazy configuration section access</description></item>
/// </list>
/// 
/// <para><strong>Security Considerations:</strong></para>
/// <list type="bullet">
/// <item><description>Secrets remain in memory only as long as needed (no internal caching)</description></item>
/// <item><description>Relies on underlying configuration provider security (encryption at rest, secure transmission)</description></item>
/// <item><description>Supports secure configuration providers like Azure Key Vault for production scenarios</description></item>
/// <item><description>No credential persistence or logging of secret values</description></item>
/// </list>
/// 
/// <para><strong>Integration with Connection String Providers:</strong></para>
/// <para>This class is specifically designed to work with the pvNugs connection string provider ecosystem, 
/// supporting the StaticSecret mode in both SQL Server and PostgreSQL providers. The secret naming convention 
/// follows the pattern <c>{SecretName}-{Role}</c> where:</para>
/// <list type="bullet">
/// <item><description>SecretName comes from the connection string provider configuration</description></item>
/// <item><description>Role represents the database role (Owner, Application, Reader)</description></item>
/// <item><description>This enables role-based database access with appropriate permission levels</description></item>
/// </list>
/// 
/// <para><strong>Deployment Patterns:</strong></para>
/// <list type="bullet">
/// <item><description><strong>Development:</strong> Use appsettings.json or environment variables for quick setup</description></item>
/// <item><description><strong>Docker Containers:</strong> Leverage environment variables or mounted configuration files</description></item>
/// <item><description><strong>Kubernetes:</strong> Integrate with ConfigMaps, Secrets, or external secret operators</description></item>
/// <item><description><strong>Cloud Native:</strong> Connect to cloud-based secret management through configuration providers</description></item>
/// <item><description><strong>On-Premises:</strong> Use encrypted configuration files or network-based configuration services</description></item>
/// </list>
/// 
/// <para><strong>Thread Safety:</strong></para>
/// <para>This class is fully thread-safe for concurrent read operations. The underlying IConfiguration 
/// implementations are typically immutable after initial loading, making concurrent access safe without 
/// additional synchronization. Configuration changes require application restart in most scenarios.</para>
/// 
/// <para><strong>Extensibility:</strong></para>
/// <para>The protected <see cref="Logger"/> property and <see cref="GetSectionAsync"/> method enable 
/// inheritance scenarios where specialized behavior might be needed while maintaining the core 
/// configuration-based secret retrieval pattern.</para>
/// </remarks>
/// <example>
/// <para><strong>Basic Registration and Configuration:</strong></para>
/// <code>
/// // Program.cs - Dependency injection setup
/// var builder = WebApplication.CreateBuilder(args);
/// 
/// // Configure the secret manager with optional prefix
/// builder.Services.Configure&lt;PvNugsSecretManagerEnvVariablesConfig&gt;(config =&gt; 
/// {
///     config.Prefix = "MyApp"; // Optional: organize secrets under a section
/// });
/// 
/// // Register the static secret manager
/// builder.Services.AddSingleton&lt;ILoggerService, ConsoleLoggerService&gt;();
/// builder.Services.AddSingleton&lt;IPvNugsStaticSecretManager, StaticSecretManager&gt;();
/// </code>
/// 
/// <para><strong>Environment Variable Configuration (with prefix):</strong></para>
/// <code>
/// // Environment variables when prefix is "MyApp"
/// // MyApp__DatabaseConnection=Server=localhost;Database=MyDb;User Id=appuser;Password=secret123;
/// // MyApp__ApiKey__ThirdParty=abc123xyz789
/// // MyApp__CertificatePassword=mycert123
/// 
/// // Retrieval code
/// var secretManager = serviceProvider.GetRequiredService&lt;IPvNugsStaticSecretManager&gt;();
/// var dbConnection = await secretManager.GetStaticSecretAsync("DatabaseConnection");
/// var apiKey = await secretManager.GetStaticSecretAsync("ApiKey__ThirdParty");
/// var certPassword = await secretManager.GetStaticSecretAsync("CertificatePassword");
/// </code>
/// 
/// <para><strong>JSON Configuration (appsettings.json with prefix):</strong></para>
/// <code>
/// // appsettings.json
/// {
///   "MyApp": {
///     "DatabaseConnection": "Server=localhost;Database=MyDb;User Id=appuser;Password=secret123;",
///     "ApiKey": {
///       "ThirdParty": "abc123xyz789"
///     },
///     "CertificatePassword": "mycert123"
///   }
/// }
/// 
/// // Usage remains the same as environment variable example above
/// </code>
/// 
/// <para><strong>Integration with Connection String Providers:</strong></para>
/// <code>
/// // Configuration for SQL Server connection string provider using StaticSecret mode
/// {
///   "PvNugsCsProviderMsSqlConfig": {
///     "Mode": "StaticSecret",
///     "Server": "localhost",
///     "Database": "MyAppDB",
///     "Username": "myapp_user",
///     "SecretName": "MyApp-Database"
///   },
///   "MyApp": {
///     "MyApp-Database-Owner": "owner_password_123",
///     "MyApp-Database-Application": "app_password_456", 
///     "MyApp-Database-Reader": "reader_password_789"
///   }
/// }
/// 
/// // The connection string provider will automatically query for role-specific passwords
/// // using the pattern {SecretName}-{Role}
/// </code>
/// 
/// <para><strong>Advanced Error Handling:</strong></para>
/// <code>
/// public class SecretService
/// {
///     private readonly IPvNugsStaticSecretManager _secretManager;
///     private readonly ILogger&lt;SecretService&gt; _logger;
/// 
///     public SecretService(IPvNugsStaticSecretManager secretManager, ILogger&lt;SecretService&gt; logger)
///     {
///         _secretManager = secretManager;
///         _logger = logger;
///     }
/// 
///     public async Task&lt;string&gt; GetRequiredSecretAsync(string secretName)
///     {
///         try
///         {
///             var secret = await _secretManager.GetStaticSecretAsync(secretName);
///             
///             if (string.IsNullOrEmpty(secret))
///             {
///                 throw new InvalidOperationException($"Required secret '{secretName}' not found in configuration");
///             }
///             
///             return secret;
///         }
///         catch (ArgumentException ex)
///         {
///             _logger.LogError(ex, "Invalid secret name provided: {SecretName}", secretName);
///             throw; // Re-throw argument validation errors
///         }
///         catch (PvNugsSecretManagerException ex)
///         {
///             _logger.LogError(ex, "Failed to retrieve secret '{SecretName}' from configuration", secretName);
///             throw; // Re-throw wrapped configuration errors
///         }
///     }
/// 
///     public async Task&lt;string?&gt; GetOptionalSecretAsync(string secretName)
///     {
///         try
///         {
///             return await _secretManager.GetStaticSecretAsync(secretName);
///         }
///         catch (PvNugsSecretManagerException ex)
///         {
///             _logger.LogWarning(ex, "Optional secret '{SecretName}' could not be retrieved, using default", secretName);
///             return null; // Return null for optional secrets
///         }
///     }
/// }
/// </code>
/// 
/// <para><strong>Cloud Integration with Azure Key Vault:</strong></para>
/// <code>
/// // Program.cs - Azure Key Vault integration
/// var builder = WebApplication.CreateBuilder(args);
/// 
/// // Add Azure Key Vault as a configuration source
/// builder.Configuration.AddAzureKeyVault(
///     new Uri($"https://{keyVaultName}.vault.azure.net/"),
///     new DefaultAzureCredential());
/// 
/// // Secrets in Azure Key Vault automatically become available through IConfiguration
/// // Key Vault secret names like "MyApp--DatabaseConnection" become "MyApp:DatabaseConnection"
/// 
/// // Register secret manager - no changes needed, works with Key Vault transparently
/// builder.Services.Configure&lt;PvNugsSecretManagerEnvVariablesConfig&gt;(config =&gt; 
/// {
///     config.Prefix = "MyApp";
/// });
/// builder.Services.AddSingleton&lt;IPvNugsStaticSecretManager, StaticSecretManager&gt;();
/// </code>
/// 
/// <para><strong>Kubernetes ConfigMap and Secret Integration:</strong></para>
/// <code>
/// # Kubernetes ConfigMap
/// apiVersion: v1
/// kind: ConfigMap
/// metadata:
///   name: myapp-config
/// data:
///   appsettings.json: |
///     {
///       "MyApp": {
///         "DatabaseConnection": "Server=db-service;Database=MyAppDB;",
///         "ApiEndpoint": "https://api.example.com"
///       }
///     }
/// 
/// ---
/// # Kubernetes Secret
/// apiVersion: v1
/// kind: Secret
/// metadata:
///   name: myapp-secrets
/// type: Opaque
/// data:
///   MyApp__ApiKey: YWJjMTIzeHl6Nzg5  # base64 encoded
///   MyApp__DatabasePassword: c2VjcmV0MTIz  # base64 encoded
/// 
/// # Pod configuration mounts both as files and environment variables
/// # The StaticSecretManager automatically picks up both sources
/// </code>
/// 
/// <para><strong>Testing and Mocking:</strong></para>
/// <code>
/// [Test]
/// public async Task GetStaticSecretAsync_ReturnsExpectedSecret()
/// {
///     // Arrange - Create in-memory configuration for testing
///     var configBuilder = new ConfigurationBuilder();
///     configBuilder.AddInMemoryCollection(new Dictionary&lt;string, string&gt;
///     {
///         {"TestPrefix:DatabaseConnection", "test-connection-string"},
///         {"TestPrefix:ApiKey", "test-api-key"}
///     });
///     
///     var configuration = configBuilder.Build();
///     var options = Options.Create(new PvNugsSecretManagerEnvVariablesConfig 
///     { 
///         Prefix = "TestPrefix" 
///     });
///     var mockLogger = new Mock&lt;ILoggerService&gt;();
///     
///     var secretManager = new StaticSecretManager(mockLogger.Object, options, configuration);
///     
///     // Act
///     var secret = await secretManager.GetStaticSecretAsync("DatabaseConnection");
///     
///     // Assert
///     Assert.That(secret, Is.EqualTo("test-connection-string"));
/// }
/// </code>
/// </example>
/// <param name="logger">
/// The logger service for recording operations, errors, and diagnostic information. 
/// Must not be null. Used for asynchronous logging of exceptions and operational events.
/// </param>
/// <param name="options">
/// Configuration options containing prefix and other settings for secret organization. 
/// Must not be null. The options pattern enables configuration-driven behavior modification.
/// </param>
/// <param name="configuration">
/// The configuration provider to retrieve secrets from. Must not be null.
/// Can be any IConfiguration implementation including composite configurations with multiple sources.
/// </param>
/// <exception cref="ArgumentNullException">
/// Thrown when any of the constructor parameters (logger, options, or configuration) is null.
/// </exception>
/// <seealso cref="IPvNugsStaticSecretManager"/>
/// <seealso cref="PvNugsSecretManagerEnvVariablesConfig"/>
/// <seealso cref="PvNugsSecretManagerException"/>
/// <seealso href="https://docs.microsoft.com/en-us/aspnet/core/fundamentals/configuration/">ASP.NET Core Configuration</seealso>
/// <seealso href="https://docs.microsoft.com/en-us/azure/key-vault/general/vs-key-vault-add-connected-service">Azure Key Vault Configuration Provider</seealso>
internal class StaticSecretManager(
    ILoggerService logger,
    IOptions<PvNugsSecretManagerEnvVariablesConfig> options,
    IConfiguration configuration) : IPvNugsStaticSecretManager
{
    /// <summary>
    /// The optional prefix used to organize secrets within the configuration.
    /// When specified, secrets are retrieved from a configuration section with this name.
    /// When null or empty, secrets are retrieved directly from the root configuration.
    /// </summary>
    /// <remarks>
    /// This prefix allows for logical grouping of secrets, which is particularly useful in:
    /// - Multi-tenant applications where secrets need to be organized by tenant
    /// - Applications with multiple environments where secrets are grouped by environment
    /// - Large applications where secrets need to be organized by feature or module
    /// </remarks>
    private readonly string? _prefix = options.Value.Prefix;

    /// <summary>
    /// Gets the logger service instance used for recording operations, errors, and diagnostic information.
    /// </summary>
    /// <value>
    /// The <see cref="ILoggerService"/> instance provided during construction.
    /// This logger is used for asynchronous logging of exceptions and operational events.
    /// </value>
    /// <remarks>
    /// The logger is exposed as a protected property to allow derived classes to access
    /// the same logging infrastructure for consistent error reporting and diagnostics.
    /// All logging operations in this class use async methods to ensure proper async flow.
    /// </remarks>
    protected ILoggerService Logger => logger;

    /// <summary>
    /// Asynchronously retrieves a static secret by name from the configured sources.
    /// </summary>
    /// <param name="secretName">
    /// The name of the secret to retrieve. Cannot be null, empty, or consist only of whitespace characters.
    /// This name is used either directly as a configuration key or as a key within the configured prefix section.
    /// </param>
    /// <param name="cancellationToken">
    /// A token to cancel the operation if needed. Currently not used in the implementation
    /// but provided for interface compliance and future extensibility.
    /// </param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains:
    /// - The secret value as a string if found in the configuration
    /// - <c>null</c> if the secret does not exist in the configuration
    /// - The method never returns an empty string unless that is the actual configured value
    /// </returns>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="secretName"/> is null, empty, or consists only of whitespace characters.
    /// This exception is not wrapped and is thrown directly to indicate invalid method usage.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when a required configuration section (specified by prefix) does not exist.
    /// This exception is wrapped in <see cref="PvNugsSecretManagerException"/> after being logged.
    /// </exception>
    /// <exception cref="PvNugsSecretManagerException">
    /// Thrown when any error occurs during secret retrieval, except for argument validation errors.
    /// The original exception is wrapped and logged before being re-thrown to provide consistent
    /// error handling across the secret management system.
    /// </exception>
    /// <remarks>
    /// <para>
    /// The method constructs the configuration lookup strategy as follows:
    /// </para>
    /// <list type="bullet">
    /// <item>If no prefix is configured (null or empty): Uses the <paramref name="secretName"/> directly as a configuration key</item>
    /// <item>If a prefix is configured: Uses <c>GetRequiredSection(prefix)[secretName]</c> to retrieve the secret from the specified section</item>
    /// </list>
    /// <para>
    /// Error Handling Strategy:
    /// </para>
    /// <list type="number">
    /// <item>Input validation errors (ArgumentException) are thrown directly without logging or wrapping</item>
    /// <item>Configuration errors and other exceptions are logged asynchronously using the configured logger</item>
    /// <item>All logged exceptions are wrapped in PvNugsSecretManagerException before being re-thrown</item>
    /// <item>The async logging ensures that log entries are properly written before the exception propagates</item>
    /// </list>
    /// <para>
    /// Performance Considerations:
    /// - Configuration access is typically very fast as values are cached
    /// - The async nature is primarily for consistent interface compliance and proper logging
    /// - No I/O operations are performed beyond potential logging to external sinks
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Basic secret retrieval
    /// var dbConnection = await secretManager.GetStaticSecretAsync("DatabaseConnection");
    /// 
    /// // Handling missing secrets
    /// var optionalApiKey = await secretManager.GetStaticSecretAsync("OptionalApiKey");
    /// if (optionalApiKey == null)
    /// {
    ///     Console.WriteLine("API key not configured, using default behavior");
    /// }
    /// 
    /// // Error handling with specific exception types
    /// try
    /// {
    ///     var criticalSecret = await secretManager.GetStaticSecretAsync("CriticalSystemKey");
    ///     // Use the secret...
    /// }
    /// catch (ArgumentException ex)
    /// {
    ///     // Handle invalid secret name
    ///     logger.LogError(ex, "Invalid secret name provided");
    /// }
    /// catch (PvNugsSecretManagerException ex)
    /// {
    ///     // Handle configuration or system errors
    ///     logger.LogError(ex, "Failed to retrieve critical system secret");
    ///     throw; // Re-throw if the secret is mandatory
    /// }
    /// 
    /// // Using cancellation token for consistency
    /// using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
    /// var secret = await secretManager.GetStaticSecretAsync("MySecret", cts.Token);
    /// </code>
    /// </example>
    public async Task<string?> GetStaticSecretAsync(
        string secretName, CancellationToken cancellationToken = new())
    {
        if (string.IsNullOrWhiteSpace(secretName))
            throw new ArgumentException("Secret name cannot be null or empty",
                nameof(secretName));
        
        try
        {
            var section = await GetSectionAsync();
            var secret = section[secretName];
            return secret;
        }
        catch (Exception e)
        {
            await Logger.LogAsync(e);
            throw new PvNugsSecretManagerException(e);
        }
    }

    
        /// <summary>
    /// Asynchronously retrieves the appropriate configuration section for secret retrieval based on the configured prefix.
    /// This method implements the prefix-based secret organization logic and serves as the foundation for all secret lookups.
    /// </summary>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains:
    /// - The root <see cref="IConfiguration"/> instance when no prefix is configured (null or empty)
    /// - The specific configuration section identified by the prefix when a prefix is configured
    /// - The method ensures the returned configuration object is valid and exists
    /// </returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when a prefix is configured but the corresponding configuration section does not exist.
    /// This exception is logged asynchronously before being thrown to provide diagnostic information.
    /// </exception>
    /// <remarks>
    /// <para><strong>Section Resolution Logic:</strong></para>
    /// <para>This method implements a two-tier resolution strategy:</para>
    /// <list type="number">
    /// <item><description><strong>No Prefix:</strong> Returns the root configuration instance for direct key access</description></item>
    /// <item><description><strong>With Prefix:</strong> Uses <c>GetRequiredSection(prefix)</c> and validates section existence</description></item>
    /// </list>
    /// 
    /// <para><strong>Prefix-Based Organization:</strong></para>
    /// <para>When a prefix is configured, this method enables logical grouping of secrets within the configuration hierarchy.
    /// This is particularly valuable for:</para>
    /// <list type="bullet">
    /// <item><description>Multi-tenant applications requiring tenant-specific secret namespaces</description></item>
    /// <item><description>Microservice architectures with service-specific secret organization</description></item>
    /// <item><description>Environment-based secret segregation (development, staging, production)</description></item>
    /// <item><description>Role-based or feature-based secret grouping</description></item>
    /// </list>
    /// 
    /// <para><strong>Error Handling Strategy:</strong></para>
    /// <para>The method implements defensive programming practices:</para>
    /// <list type="bullet">
    /// <item><description>Validates section existence using <c>IConfigurationSection.Exists()</c></description></item>
    /// <item><description>Provides clear error messages indicating the missing section name</description></item>
    /// <item><description>Logs exceptions asynchronously before throwing for diagnostic purposes</description></item>
    /// <item><description>Throws InvalidOperationException directly (not wrapped) for configuration errors</description></item>
    /// </list>
    /// 
    /// <para><strong>Performance Considerations:</strong></para>
    /// <list type="bullet">
    /// <item><description>Configuration section access is typically fast (cached by configuration system)</description></item>
    /// <item><description>Section existence check is a lightweight operation</description></item>
    /// <item><description>No I/O operations except for potential logging to external sinks</description></item>
    /// <item><description>Results are not cached - relies on underlying configuration provider caching</description></item>
    /// </list>
    /// 
    /// <para><strong>Extensibility Design:</strong></para>
    /// <para>This method is marked <c>protected</c> to enable derived classes to:</para>
    /// <list type="bullet">
    /// <item><description>Override section resolution logic for specialized scenarios</description></item>
    /// <item><description>Implement custom validation or transformation logic</description></item>
    /// <item><description>Add additional logging or monitoring for section access</description></item>
    /// <item><description>Provide fallback behavior for missing sections</description></item>
    /// </list>
    /// 
    /// <para><strong>Thread Safety:</strong></para>
    /// <para>This method is thread-safe as it operates on immutable configuration objects and performs only read operations.
    /// Multiple threads can safely call this method concurrently without synchronization concerns.</para>
    /// 
    /// <para><strong>Configuration Section Structure:</strong></para>
    /// <para>The method supports hierarchical configuration structures. For example, with prefix "MyApp":</para>
    /// <code>
    /// // JSON structure that would be successfully resolved:
    /// {
    ///   "MyApp": {
    ///     "DatabaseConnection": "connection-string-here",
    ///     "ApiKeys": {
    ///       "ThirdParty": "api-key-here"
    ///     }
    ///   }
    /// }
    /// 
    /// // Environment variable equivalent:
    /// // MyApp__DatabaseConnection=connection-string-here
    /// // MyApp__ApiKeys__ThirdParty=api-key-here
    /// </code>
    /// </remarks>
    /// <example>
    /// <para><strong>Usage in Derived Classes:</strong></para>
    /// <code>
    /// public class CustomStaticSecretManager : StaticSecretManager
    /// {
    ///     public CustomStaticSecretManager(
    ///         ILoggerService logger,
    ///         IOptions&lt;PvNugsSecretManagerEnvVariablesConfig&gt; options,
    ///         IConfiguration configuration) 
    ///         : base(logger, options, configuration) { }
    /// 
    ///     // Override to add custom validation or fallback logic
    ///     protected override async Task&lt;IConfiguration&gt; GetSectionAsync()
    ///     {
    ///         try 
    ///         {
    ///             var section = await base.GetSectionAsync();
    ///             
    ///             // Add custom validation
    ///             await ValidateSecretSectionAsync(section);
    ///             
    ///             return section;
    ///         }
    ///         catch (InvalidOperationException ex)
    ///         {
    ///             // Provide fallback behavior for missing sections
    ///             await Logger.LogAsync($"Primary section not found, using fallback: {ex.Message}");
    ///             return GetFallbackConfiguration();
    ///         }
    ///     }
    /// 
    ///     private async Task ValidateSecretSectionAsync(IConfiguration section)
    ///     {
    ///         // Custom validation logic
    ///         var requiredKeys = new[] { "DatabaseConnection", "ApiKey" };
    ///         foreach (var key in requiredKeys)
    ///         {
    ///             if (string.IsNullOrEmpty(section[key]))
    ///             {
    ///                 await Logger.LogAsync($"Warning: Required secret '{key}' not found", SeverityEnu.Warning);
    ///             }
    ///         }
    ///     }
    /// }
    /// </code>
    /// 
    /// <para><strong>Debugging Configuration Issues:</strong></para>
    /// <code>
    /// public class DiagnosticStaticSecretManager : StaticSecretManager
    /// {
    ///     public DiagnosticStaticSecretManager(/* parameters */) : base(/* parameters */) { }
    /// 
    ///     protected override async Task&lt;IConfiguration&gt; GetSectionAsync()
    ///     {
    ///         var section = await base.GetSectionAsync();
    ///         
    ///         // Log all available keys for debugging
    ///         await Logger.LogAsync("Available configuration keys:");
    ///         foreach (var child in section.GetChildren())
    ///         {
    ///             await Logger.LogAsync($"  - {child.Key}: {(string.IsNullOrEmpty(child.Value) ? "&lt;section&gt;" : "&lt;has value&gt;")}");
    ///         }
    ///         
    ///         return section;
    ///     }
    /// }
    /// </code>
    /// 
    /// <para><strong>Error Scenarios and Troubleshooting:</strong></para>
    /// <code>
    /// // This would cause GetSectionAsync to throw InvalidOperationException:
    /// 
    /// // Configuration with prefix "MyApp" but missing section:
    /// {
    ///   "SomeOtherSection": {
    ///     "Setting": "value"
    ///   }
    ///   // Missing: "MyApp" section
    /// }
    /// 
    /// // Or environment variables without the prefix:
    /// // DatabaseConnection=value  // Should be: MyApp__DatabaseConnection=value
    /// 
    /// // Correct configuration:
    /// {
    ///   "MyApp": {  // This section must exist when prefix is "MyApp"
    ///     "DatabaseConnection": "connection-string"
    ///   }
    /// }
    /// </code>
    /// </example>
    /// <seealso cref="GetStaticSecretAsync(string, CancellationToken)"/>
    /// <seealso cref="PvNugsSecretManagerEnvVariablesConfig.Prefix"/>
    /// <seealso cref="Microsoft.Extensions.Configuration.IConfiguration"/>
    /// <seealso cref="Microsoft.Extensions.Configuration.IConfigurationSection"/>
    /// <seealso href="https://docs.microsoft.com/en-us/dotnet/api/microsoft.extensions.configuration.configurationextensions.getrequiredsection">GetRequiredSection Method</seealso>
    protected async Task<IConfiguration> GetSectionAsync()
    {
        if (string.IsNullOrEmpty(_prefix)) return configuration;

        var section = configuration.GetRequiredSection(_prefix);
        if (section.Exists()) return section;
        
        var ex = new InvalidOperationException($"Required configuration section '{_prefix}' does not exist.");
        await Logger.LogAsync(ex);
        throw ex;
    }
    
}
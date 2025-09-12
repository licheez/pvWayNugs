using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using pvNugsLoggerNc9Abstractions;
using pvNugsSecretManagerNc9Abstractions;

namespace pvNugsSecretManagerNc9EnvVariables;

/// <summary>
/// Provides dependency injection extension methods for registering the pvNugs Secret Manager 
/// environment variables implementation with the Microsoft.Extensions.DependencyInjection container.
/// This static class offers both unified and granular registration approaches for maximum flexibility
/// in different application scenarios and architectural patterns.
/// </summary>
/// <remarks>
/// <para><strong>Registration Approaches:</strong></para>
/// <para>This static class provides three distinct registration methods to accommodate different use cases:</para>
/// <list type="bullet">
/// <item><description><strong>Unified Registration:</strong> <see cref="TryAddPvNugsSecretManagerEnvVariables"/> registers both static and dynamic secret managers together</description></item>
/// <item><description><strong>Static-Only Registration:</strong> <see cref="TryAddPvNugsEnvVariablesStaticSecretManager"/> registers only the static secret manager</description></item>
/// <item><description><strong>Dynamic-Only Registration:</strong> <see cref="TryAddPvNugsEnvVariablesDynamicSecretManager"/> registers only the dynamic secret manager</description></item>
/// </list>
/// 
/// <para><strong>Services Registered:</strong></para>
/// <list type="bullet">
/// <item><description><see cref="IPvNugsStaticSecretManager"/> implemented by <see cref="StaticSecretManager"/> (singleton)</description></item>
/// <item><description><see cref="IPvNugsDynamicSecretManager"/> implemented by <see cref="DynamicSecretManager"/> (singleton)</description></item>
/// <item><description><see cref="IConfiguration"/> registration (if not already present)</description></item>
/// <item><description>Automatic configuration binding for <see cref="PvNugsSecretManagerEnvVariablesConfig"/> when using unified registration</description></item>
/// </list>
/// 
/// <para><strong>Dependency Requirements:</strong></para>
/// <para>All registration methods validate that <see cref="ILoggerService"/> is already registered in the container.
/// This ensures that secret manager services can resolve their logging dependencies at runtime.</para>
/// 
/// <para><strong>Service Lifetime Strategy:</strong></para>
/// <para>All services are registered as singletons because they are:</para>
/// <list type="bullet">
/// <item><description><strong>Stateless:</strong> Contain no mutable state that could cause thread safety issues</description></item>
/// <item><description><strong>Configuration-dependent:</strong> Rely only on immutable configuration and logging services</description></item>
/// <item><description><strong>Performance-optimized:</strong> Singleton lifetime provides optimal performance for frequent secret access</description></item>
/// <item><description><strong>Thread-safe:</strong> Designed for safe concurrent access across multiple threads</description></item>
/// </list>
/// 
/// <para><strong>Registration Safety:</strong></para>
/// <para>All methods use <c>TryAdd</c> patterns providing:</para>
/// <list type="bullet">
/// <item><description><strong>Non-destructive:</strong> Won't overwrite existing service registrations</description></item>
/// <item><description><strong>Idempotent:</strong> Safe to call multiple times without side effects</description></item>
/// <item><description><strong>Customizable:</strong> Allows custom implementations to be registered first and take precedence</description></item>
/// <item><description><strong>Composable:</strong> Methods can be mixed and matched for specific architectural needs</description></item>
/// </list>
/// 
/// <para><strong>Architecture Flexibility:</strong></para>
/// <para>The granular registration methods enable various architectural patterns:</para>
/// <list type="bullet">
/// <item><description><strong>Microservices:</strong> Register only needed secret managers per service</description></item>
/// <item><description><strong>Legacy Integration:</strong> Add dynamic capabilities to existing static-only applications</description></item>
/// <item><description><strong>Testing:</strong> Register different implementations for different test scenarios</description></item>
/// <item><description><strong>Feature Flags:</strong> Conditionally register services based on application features</description></item>
/// </list>
/// 
/// <para><strong>Error Prevention:</strong></para>
/// <para>All methods include proactive validation to prevent common integration issues:</para>
/// <list type="bullet">
/// <item><description><strong>Dependency Validation:</strong> Ensures required <see cref="ILoggerService"/> is registered</description></item>
/// <item><description><strong>Clear Error Messages:</strong> Provides actionable guidance when dependencies are missing</description></item>
/// <item><description><strong>Early Failure:</strong> Catches configuration issues at startup rather than runtime</description></item>
/// </list>
/// 
/// <para><strong>Thread Safety:</strong></para>
/// <para>Registration methods are not thread-safe and should only be called during application startup.
/// However, the registered services are designed for thread-safe concurrent use throughout the application lifecycle.</para>
/// </remarks>
/// <example>
/// <para><strong>Complete Setup with Both Managers:</strong></para>
/// <code>
/// var builder = WebApplication.CreateBuilder(args);
/// 
/// // 1. Register logger service (required dependency)
/// builder.Services.TryAddPvNugsConsoleLoggerService(SeverityEnu.Information);
/// 
/// // 2. Register both static and dynamic secret managers
/// builder.Services.TryAddPvNugsSecretManagerEnvVariables(builder.Configuration);
/// 
/// var app = builder.Build();
/// 
/// // 3. Both services are now available
/// app.MapGet("/secrets-status", async (
///     IPvNugsStaticSecretManager staticManager,
///     IPvNugsDynamicSecretManager dynamicManager) =&gt;
/// {
///     var apiKey = await staticManager.GetStaticSecretAsync("ApiKey");
///     var dbCredential = await dynamicManager.GetDynamicSecretAsync("DatabaseService");
///     
///     return new { HasApiKey = !string.IsNullOrEmpty(apiKey), HasDbCredential = dbCredential != null };
/// });
/// </code>
/// 
/// <para><strong>Selective Registration Examples:</strong></para>
/// <code>
/// // Legacy application - only needs static secrets
/// builder.Services.AddSingleton&lt;ILoggerService, FileLoggerService&gt;();
/// builder.Services.TryAddPvNugsEnvVariablesStaticSecretManager(builder.Configuration);
/// 
/// // Modern microservice - only needs dynamic credentials
/// builder.Services.AddSingleton&lt;ILoggerService, StructuredLoggerService&gt;();
/// builder.Services.TryAddPvNugsEnvVariablesDynamicSecretManager(builder.Configuration);
/// 
/// // Hybrid approach - register based on feature flags
/// builder.Services.AddSingleton&lt;ILoggerService, CloudLoggerService&gt;();
/// builder.Services.TryAddPvNugsEnvVariablesStaticSecretManager(builder.Configuration);
/// 
/// if (builder.Configuration.GetValue&lt;bool&gt;("Features:UseDynamicCredentials"))
/// {
///     builder.Services.TryAddPvNugsEnvVariablesDynamicSecretManager(builder.Configuration);
/// }
/// </code>
/// 
/// <para><strong>Configuration Examples:</strong></para>
/// <code>
/// // appsettings.json for both static and dynamic secrets:
/// {
///   "PvNugsSecretManagerEnvVariablesConfig": {
///     "Prefix": "Production"
///   },
///   "Production": {
///     "ApiKey": "static-api-key-12345",
///     "ExternalService": "static-service-token",
///     "DatabaseService__username": "dynamic_user_abc",
///     "DatabaseService__password": "dynamic_pass_xyz", 
///     "DatabaseService__expirationDateUtc": "2024-12-31T23:59:59Z"
///   }
/// }
/// </code>
/// </example>
public static class PvNugsSecretManagerEnvVariablesDi
{
    /// <summary>
    /// Registers both static and dynamic secret manager services with the dependency injection container
    /// using a unified configuration approach. This method provides the most comprehensive secret management
    /// capabilities by enabling both traditional static secrets and modern dynamic credential handling.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add the services to. Must not be null.</param>
    /// <param name="configuration">
    /// The configuration instance used to bind the <see cref="PvNugsSecretManagerEnvVariablesConfig"/>
    /// settings and provide secret values to both secret managers. Must not be null.
    /// </param>
    /// <returns>
    /// The same <see cref="IServiceCollection"/> instance for fluent method chaining,
    /// enabling additional service registrations in a single call chain.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="services"/> or <paramref name="configuration"/> is null.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when no <see cref="ILoggerService"/> implementation is found in the service collection.
    /// This validation ensures that both secret managers can resolve their logging dependencies.
    /// </exception>
    /// <remarks>
    /// <para><strong>Registration Approach:</strong></para>
    /// <para>This method implements a comprehensive registration strategy by:</para>
    /// <list type="number">
    /// <item><description><strong>Static Manager Registration:</strong> Calls <see cref="TryAddPvNugsEnvVariablesStaticSecretManager"/> for static secret handling</description></item>
    /// <item><description><strong>Dynamic Manager Registration:</strong> Calls <see cref="TryAddPvNugsEnvVariablesDynamicSecretManager"/> for dynamic credential management</description></item>
    /// <item><description><strong>Unified Configuration:</strong> Both managers share the same configuration instance and settings</description></item>
    /// </list>
    /// 
    /// <para><strong>Benefits of Unified Registration:</strong></para>
    /// <list type="bullet">
    /// <item><description><strong>Complete Functionality:</strong> Provides access to both static and dynamic secret management capabilities</description></item>
    /// <item><description><strong>Simplified Setup:</strong> Single method call handles all secret management registration needs</description></item>
    /// <item><description><strong>Consistent Configuration:</strong> Both managers use the same configuration source and prefix settings</description></item>
    /// <item><description><strong>Future-Proof:</strong> Applications can use either type of secret without additional registration</description></item>
    /// </list>
    /// 
    /// <para><strong>Use Cases:</strong></para>
    /// <list type="bullet">
    /// <item><description><strong>Full-Featured Applications:</strong> Applications that need both API keys (static) and database credentials (dynamic)</description></item>
    /// <item><description><strong>Migration Scenarios:</strong> Transitioning from static to dynamic secrets while maintaining backward compatibility</description></item>
    /// <item><description><strong>Multi-Environment:</strong> Applications that use different secret types across development, staging, and production</description></item>
    /// <item><description><strong>Service Integration:</strong> Applications integrating with multiple external services requiring different authentication approaches</description></item>
    /// </list>
    /// 
    /// <para><strong>Configuration Sharing:</strong></para>
    /// <para>Both secret managers share the same <see cref="PvNugsSecretManagerEnvVariablesConfig"/> configuration,
    /// including the prefix setting. This ensures consistent secret organization and retrieval patterns
    /// across both static and dynamic secret types.</para>
    /// 
    /// <para><strong>Performance Considerations:</strong></para>
    /// <para>While this method registers both managers, there is minimal performance overhead since:</para>
    /// <list type="bullet">
    /// <item><description>Both services are singletons and only instantiated once</description></item>
    /// <item><description>Services are only resolved when explicitly injected into consuming classes</description></item>
    /// <item><description>No background processes or resources are consumed until services are used</description></item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <para><strong>Basic Unified Registration:</strong></para>
    /// <code>
    /// var builder = WebApplication.CreateBuilder(args);
    /// 
    /// // Register required logger
    /// builder.Services.AddSingleton&lt;ILoggerService, ConsoleLoggerService&gt;();
    /// 
    /// // Register both secret managers with single call
    /// builder.Services.TryAddPvNugsSecretManagerEnvVariables(builder.Configuration);
    /// 
    /// var app = builder.Build();
    /// 
    /// // Services can be used independently or together
    /// public class MixedSecretService
    /// {
    ///     public MixedSecretService(
    ///         IPvNugsStaticSecretManager staticSecrets,
    ///         IPvNugsDynamicSecretManager dynamicSecrets) { }
    ///         
    ///     public async Task&lt;AuthResult&gt; AuthenticateAsync()
    ///     {
    ///         // Use static secret for API key
    ///         var apiKey = await staticSecrets.GetStaticSecretAsync("ExternalApiKey");
    ///         
    ///         // Use dynamic credentials for database
    ///         var dbCreds = await dynamicSecrets.GetDynamicSecretAsync("DatabaseService");
    ///         
    ///         return AuthResult.Success;
    ///     }
    /// }
    /// </code>
    /// 
    /// <para><strong>Configuration for Mixed Usage:</strong></para>
    /// <code>
    /// // appsettings.json supporting both secret types
    /// {
    ///   "PvNugsSecretManagerEnvVariablesConfig": {
    ///     "Prefix": "MyApp"
    ///   },
    ///   "MyApp": {
    ///     // Static secrets (simple key-value pairs)
    ///     "ExternalApiKey": "static-key-12345",
    ///     "PaymentGatewayToken": "static-token-67890",
    ///     
    ///     // Dynamic credentials (structured with expiration)
    ///     "DatabaseService__username": "temp_db_user_abc",
    ///     "DatabaseService__password": "temp_db_pass_xyz",
    ///     "DatabaseService__expirationDateUtc": "2024-06-30T23:59:59Z",
    ///     
    ///     "CacheService__username": "temp_cache_user_def",
    ///     "CacheService__password": "temp_cache_pass_uvw",
    ///     "CacheService__expirationDateUtc": "2024-07-31T23:59:59Z"
    ///   }
    /// }
    /// </code>
    /// 
    /// <para><strong>Service Usage Examples:</strong></para>
    /// <code>
    /// // Controller using both types of secrets
    /// [ApiController]
    /// public class SecureController : ControllerBase
    /// {
    ///     private readonly IPvNugsStaticSecretManager _staticSecrets;
    ///     private readonly IPvNugsDynamicSecretManager _dynamicSecrets;
    ///     
    ///     public SecureController(
    ///         IPvNugsStaticSecretManager staticSecrets,
    ///         IPvNugsDynamicSecretManager dynamicSecrets)
    ///     {
    ///         _staticSecrets = staticSecrets;
    ///         _dynamicSecrets = dynamicSecrets;
    ///     }
    ///     
    ///     [HttpGet("external-data")]
    ///     public async Task&lt;IActionResult&gt; GetExternalDataAsync()
    ///     {
    ///         // Use static API key for external service
    ///         var apiKey = await _staticSecrets.GetStaticSecretAsync("ExternalApiKey");
    ///         if (string.IsNullOrEmpty(apiKey))
    ///             return BadRequest("External API key not configured");
    ///             
    ///         // Call external service with static key
    ///         var externalData = await CallExternalServiceAsync(apiKey);
    ///         return Ok(externalData);
    ///     }
    ///     
    ///     [HttpGet("database-report")]
    ///     public async Task&lt;IActionResult&gt; GetDatabaseReportAsync()
    ///     {
    ///         // Use dynamic credentials for database
    ///         var dbCredentials = await _dynamicSecrets.GetDynamicSecretAsync("DatabaseService");
    ///         if (dbCredentials?.ExpirationDateUtc &lt;= DateTime.UtcNow)
    ///             return BadRequest("Database credentials unavailable or expired");
    ///             
    ///         // Generate database report with dynamic credentials
    ///         var report = await GenerateReportAsync(dbCredentials);
    ///         return Ok(report);
    ///     }
    /// }
    /// </code>
    /// </example>
    public static IServiceCollection TryAddPvNugsSecretManagerEnvVariables(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        return services.TryAddPvNugsEnvVariablesStaticSecretManager(configuration)
            .TryAddPvNugsEnvVariablesDynamicSecretManager(configuration);
    }

    /// <summary>
    /// Registers only the static secret manager service with the dependency injection container.
    /// This method provides focused registration for applications that only need traditional
    /// key-value secret management without dynamic credential capabilities.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add the services to. Must not be null.</param>
    /// <param name="configuration">
    /// The configuration instance used to provide secret values to the static secret manager. 
    /// Must not be null. This configuration will be registered as a singleton if not already present.
    /// </param>
    /// <returns>
    /// The same <see cref="IServiceCollection"/> instance for fluent method chaining,
    /// enabling additional service registrations.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="services"/> or <paramref name="configuration"/> is null.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when no <see cref="ILoggerService"/> implementation is found in the service collection.
    /// This validation ensures that the static secret manager can resolve its logging dependency.
    /// </exception>
    /// <remarks>
    /// <para><strong>Focused Registration:</strong></para>
    /// <para>This method provides targeted registration for scenarios where only static secret management is needed:</para>
    /// <list type="number">
    /// <item><description><strong>Dependency Validation:</strong> Ensures <see cref="ILoggerService"/> is registered</description></item>
    /// <item><description><strong>Configuration Registration:</strong> Registers <see cref="IConfiguration"/> if not already present</description></item>
    /// <item><description><strong>Service Registration:</strong> Registers <see cref="IPvNugsStaticSecretManager"/> with <see cref="StaticSecretManager"/> implementation</description></item>
    /// </list>
    /// 
    /// <para><strong>Use Cases:</strong></para>
    /// <list type="bullet">
    /// <item><description><strong>Legacy Applications:</strong> Existing applications that use simple configuration-based secrets</description></item>
    /// <item><description><strong>Simple Services:</strong> Microservices that only need API keys or connection strings</description></item>
    /// <item><description><strong>Development Environments:</strong> Local development where dynamic credentials are unnecessary</description></item>
    /// <item><description><strong>Static Infrastructure:</strong> Applications with fixed, long-lived credentials</description></item>
    /// </list>
    /// 
    /// <para><strong>Performance Benefits:</strong></para>
    /// <list type="bullet">
    /// <item><description><strong>Minimal Footprint:</strong> Only registers services that will be used</description></item>
    /// <item><description><strong>Faster Startup:</strong> Reduces service resolution overhead during container building</description></item>
    /// <item><description><strong>Clear Dependencies:</strong> Makes application secret management requirements explicit</description></item>
    /// </list>
    /// 
    /// <para><strong>Configuration Requirements:</strong></para>
    /// <para>The static secret manager expects simple key-value configuration patterns:</para>
    /// <list type="bullet">
    /// <item><description><strong>Direct Keys:</strong> Simple string values for secret storage</description></item>
    /// <item><description><strong>Prefix Support:</strong> Optional prefix-based organization using <see cref="PvNugsSecretManagerEnvVariablesConfig"/></description></item>
    /// <item><description><strong>Multiple Sources:</strong> Support for JSON, environment variables, and other configuration providers</description></item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <para><strong>Legacy Application Integration:</strong></para>
    /// <code>
    /// // Existing application adding secret management
    /// var builder = WebApplication.CreateBuilder(args);
    /// 
    /// // Existing services
    /// builder.Services.AddSingleton&lt;ILoggerService, ExistingLoggerService&gt;();
    /// builder.Services.AddScoped&lt;IUserService, UserService&gt;();
    /// 
    /// // Add only static secret management
    /// builder.Services.TryAddPvNugsEnvVariablesStaticSecretManager(builder.Configuration);
    /// 
    /// var app = builder.Build();
    /// 
    /// // Update existing services to use secret manager
    /// public class UserService
    /// {
    ///     private readonly IPvNugsStaticSecretManager _secrets;
    ///     
    ///     public UserService(IPvNugsStaticSecretManager secrets)
    ///     {
    ///         _secrets = secrets;
    ///     }
    ///     
    ///     public async Task&lt;bool&gt; ValidateApiKeyAsync(string key)
    ///     {
    ///         var validKey = await _secrets.GetStaticSecretAsync("UserValidationApiKey");
    ///         return key == validKey;
    ///     }
    /// }
    /// </code>
    /// 
    /// <para><strong>Microservice with Static Secrets:</strong></para>
    /// <code>
    /// // Simple microservice configuration
    /// var builder = Host.CreateApplicationBuilder(args);
    /// 
    /// // Minimal logging
    /// builder.Services.AddSingleton&lt;ILoggerService, ConsoleLoggerService&gt;();
    /// 
    /// // Only static secrets needed
    /// builder.Services.TryAddPvNugsEnvVariablesStaticSecretManager(builder.Configuration);
    /// 
    /// // Service implementation
    /// builder.Services.AddHostedService&lt;NotificationService&gt;();
    /// 
    /// var host = builder.Build();
    /// await host.RunAsync();
    /// 
    /// // Service using only static secrets
    /// public class NotificationService : BackgroundService
    /// {
    ///     private readonly IPvNugsStaticSecretManager _secrets;
    ///     
    ///     public NotificationService(IPvNugsStaticSecretManager secrets)
    ///     {
    ///         _secrets = secrets;
    ///     }
    ///     
    ///     protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    ///     {
    ///         var smtpPassword = await _secrets.GetStaticSecretAsync("SmtpPassword");
    ///         var apiKey = await _secrets.GetStaticSecretAsync("NotificationApiKey");
    ///         
    ///         // Use static secrets for notification service
    ///     }
    /// }
    /// </code>
    /// 
    /// <para><strong>Configuration Examples:</strong></para>
    /// <code>
    /// // appsettings.json for static-only secrets
    /// {
    ///   "PvNugsSecretManagerEnvVariablesConfig": {
    ///     "Prefix": "StaticSecrets"
    ///   },
    ///   "StaticSecrets": {
    ///     "DatabaseConnectionString": "Server=localhost;Database=MyApp;...",
    ///     "ExternalApiKey": "static-api-key-12345",
    ///     "SmtpPassword": "email-service-password",
    ///     "EncryptionKey": "base64-encryption-key"
    ///   }
    /// }
    /// 
    /// // Environment variables alternative
    /// // StaticSecrets__DatabaseConnectionString=Server=localhost;Database=MyApp;...
    /// // StaticSecrets__ExternalApiKey=static-api-key-12345
    /// // StaticSecrets__SmtpPassword=email-service-password
    /// // StaticSecrets__EncryptionKey=base64-encryption-key
    /// </code>
    /// </example>
    public static IServiceCollection TryAddPvNugsEnvVariablesStaticSecretManager(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Validate that required dependencies exist
        if (services.All(x => x.ServiceType != typeof(ILoggerService)))
        {
            throw new InvalidOperationException(
                "ILoggerService must be registered before adding PvNugs Secret Manager. " +
                "Please register a logger service implementation first.");
        }
        services.Configure<PvNugsSecretManagerEnvVariablesConfig>(
            configuration.GetSection(nameof(PvNugsSecretManagerEnvVariablesConfig)));
        
        // Register static secret manager service
        services.TryAddSingleton<IConfiguration>(
            _ => configuration);
        services.TryAddSingleton<
            IPvNugsStaticSecretManager, StaticSecretManager>();
        
        return services;
    }
    
    /// <summary>
    /// Registers only the dynamic secret manager service with the dependency injection container.
    /// This method provides focused registration for applications that exclusively need modern
    /// time-limited credential management with automatic expiration handling.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add the services to. Must not be null.</param>
    /// <param name="configuration">
    /// The configuration instance used to provide credential components (username, password, expiration) 
    /// to the dynamic secret manager. Must not be null. This configuration will be registered as a 
    /// singleton if not already present.
    /// </param>
    /// <returns>
    /// The same <see cref="IServiceCollection"/> instance for fluent method chaining,
    /// enabling additional service registrations.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="services"/> or <paramref name="configuration"/> is null.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when no <see cref="ILoggerService"/> implementation is found in the service collection.
    /// This validation ensures that the dynamic secret manager can resolve its logging dependency.
    /// </exception>
    /// <remarks>
    /// <para><strong>Specialized Registration:</strong></para>
    /// <para>This method provides targeted registration for advanced secret management scenarios:</para>
    /// <list type="number">
    /// <item><description><strong>Dependency Validation:</strong> Ensures <see cref="ILoggerService"/> is registered</description></item>
    /// <item><description><strong>Configuration Registration:</strong> Registers <see cref="IConfiguration"/> if not already present</description></item>
    /// <item><description><strong>Service Registration:</strong> Registers <see cref="IPvNugsDynamicSecretManager"/> with <see cref="DynamicSecretManager"/> implementation</description></item>
    /// </list>
    /// 
    /// <para><strong>Use Cases:</strong></para>
    /// <list type="bullet">
    /// <item><description><strong>Modern Applications:</strong> New applications built with security-first principles</description></item>
    /// <item><description><strong>Database Services:</strong> Applications requiring time-limited database credentials</description></item>
    /// <item><description><strong>High-Security Environments:</strong> Applications in regulated industries requiring credential rotation</description></item>
    /// <item><description><strong>Cloud-Native Services:</strong> Microservices using dynamic secret management systems</description></item>
    /// <item><description><strong>Zero-Trust Architecture:</strong> Applications implementing zero-trust security models</description></item>
    /// </list>
    /// 
    /// <para><strong>Security Benefits:</strong></para>
    /// <list type="bullet">
    /// <item><description><strong>Time-Limited Credentials:</strong> All credentials have explicit expiration dates</description></item>
    /// <item><description><strong>Automatic Validation:</strong> Built-in expiration checking prevents use of expired credentials</description></item>
    /// <item><description><strong>Rotation Readiness:</strong> Designed to work with credential rotation systems</description></item>
    /// <item><description><strong>Audit Trail:</strong> Structured credential access with expiration tracking</description></item>
    /// </list>
    /// 
    /// <para><strong>Configuration Requirements:</strong></para>
    /// <para>The dynamic secret manager expects structured configuration with three components per credential:</para>
    /// <list type="bullet">
    /// <item><description><strong>Username Component:</strong> <c>{SecretName}__username</c></description></item>
    /// <item><description><strong>Password Component:</strong> <c>{SecretName}__password</c></description></item>
    /// <item><description><strong>Expiration Component:</strong> <c>{SecretName}__expirationDateUtc</c> in ISO 8601 format</description></item>
    /// </list>
    /// 
    /// <para><strong>Performance Characteristics:</strong></para>
    /// <list type="bullet">
    /// <item><description><strong>Optimized Footprint:</strong> Only registers dynamic secret management capabilities</description></item>
    /// <item><description><strong>Validation Overhead:</strong> Includes expiration validation but optimized for frequent access</description></item>
    /// <item><description><strong>Memory Efficient:</strong> No unnecessary static secret management overhead</description></item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <para><strong>Modern Application with Dynamic Credentials:</strong></para>
    /// <code>
    /// var builder = WebApplication.CreateBuilder(args);
    /// 
    /// // Modern logging infrastructure
    /// builder.Services.AddSingleton&lt;ILoggerService, StructuredLoggerService&gt;();
    /// 
    /// // Only dynamic credential management
    /// builder.Services.TryAddPvNugsEnvVariablesDynamicSecretManager(builder.Configuration);
    /// 
    /// // Application services
    /// builder.Services.AddScoped&lt;IDatabaseService, DatabaseService&gt;();
    /// builder.Services.AddScoped&lt;ICacheService, CacheService&gt;();
    /// 
    /// var app = builder.Build();
    /// 
    /// // Services using only dynamic credentials
    /// public class DatabaseService
    /// {
    ///     private readonly IPvNugsDynamicSecretManager _dynamicSecrets;
    ///     
    ///     public DatabaseService(IPvNugsDynamicSecretManager dynamicSecrets)
    ///     {
    ///         _dynamicSecrets = dynamicSecrets;
    ///     }
    ///     
    ///     public async Task&lt;string&gt; GetConnectionStringAsync()
    ///     {
    ///         var credential = await _dynamicSecrets.GetDynamicSecretAsync("PrimaryDatabase");
    ///         
    ///         if (credential?.ExpirationDateUtc &lt;= DateTime.UtcNow)
    ///             throw new InvalidOperationException("Database credential expired");
    ///             
    ///         return $"Server=db.example.com;Database=prod;User Id={credential.Username};Password={credential.Password};";
    ///     }
    /// }
    /// </code>
    /// 
    /// <para><strong>High-Security Microservice:</strong></para>
    /// <code>
    /// // Microservice with rotating database credentials
    /// var builder = Host.CreateApplicationBuilder(args);
    /// 
    /// // Security-focused logging
    /// builder.Services.AddSingleton&lt;ILoggerService, SecurityAuditLoggerService&gt;();
    /// 
    /// // Dynamic secrets only
    /// builder.Services.TryAddPvNugsEnvVariablesDynamicSecretManager(builder.Configuration);
    /// 
    /// // Background service with credential monitoring
    /// builder.Services.AddHostedService&lt;SecureDataProcessor&gt;();
    /// 
    /// var host = builder.Build();
    /// await host.RunAsync();
    /// 
    /// public class SecureDataProcessor : BackgroundService
    /// {
    ///     private readonly IPvNugsDynamicSecretManager _dynamicSecrets;
    ///     private readonly ILogger _logger;
    ///     
    ///     public SecureDataProcessor(
    ///         IPvNugsDynamicSecretManager dynamicSecrets,
    ///         ILogger logger)
    ///     {
    ///         _dynamicSecrets = dynamicSecrets;
    ///         _logger = logger;
    ///     }
    ///     
    ///     protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    ///     {
    ///         while (!stoppingToken.IsCancellationRequested)
    ///         {
    ///             try
    ///             {
    ///                 // Get dynamic database credentials
    ///                 var dbCredential = await _dynamicSecrets.GetDynamicSecretAsync("SecureDatabase");
    ///                 
    ///                 if (dbCredential == null)
    ///                 {
    ///                     _logger.LogError("Database credential not available");
    ///                     await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
    ///                     continue;
    ///                 }
    ///                 
    ///                 // Check expiration with buffer time
    ///                 var timeUntilExpiry = dbCredential.ExpirationDateUtc - DateTime.UtcNow;
    ///                 if (timeUntilExpiry.TotalMinutes &lt; 10)
    ///                 {
    ///                     _logger.LogWarning("Database credential expires in {Minutes} minutes", 
    ///                         timeUntilExpiry.TotalMinutes);
    ///                 }
    ///                 
    ///                 // Process data with fresh credentials
    ///                 await ProcessSecureDataAsync(dbCredential);
    ///             }
    ///             catch (Exception ex)
    ///             {
    ///                 _logger.LogError(ex, "Error processing secure data");
    ///             }
    ///             
    ///             await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
    ///         }
    ///     }
    /// }
    /// </code>
    /// 
    /// <para><strong>Configuration Examples:</strong></para>
    /// <code>
    /// // appsettings.json for dynamic credentials only
    /// {
    ///   "PvNugsSecretManagerEnvVariablesConfig": {
    ///     "Prefix": "DynamicCreds"
    ///   },
    ///   "DynamicCreds": {
    ///     "PrimaryDatabase__username": "temp_user_abc123",
    ///     "PrimaryDatabase__password": "temp_pass_xyz789",
    ///     "PrimaryDatabase__expirationDateUtc": "2024-06-30T14:30:00Z",
    ///     
    ///     "RedisCache__username": "cache_user_def456",
    ///     "RedisCache__password": "cache_pass_uvw012",
    ///     "RedisCache__expirationDateUtc": "2024-06-30T15:00:00Z"
    ///   }
    /// }
    /// 
    /// // Environment variables for containerized deployment
    /// // DynamicCreds__PrimaryDatabase__username=temp_user_abc123
    /// // DynamicCreds__PrimaryDatabase__password=temp_pass_xyz789
    /// // DynamicCreds__PrimaryDatabase__expirationDateUtc=2024-06-30T14:30:00Z
    /// </code>
    /// 
    /// <para><strong>Integration with True Dynamic Secret Systems:</strong></para>
    /// <code>
    /// // This simulated implementation can later be replaced with true dynamic systems
    /// // The same interface and configuration patterns will work with:
    /// // - HashiCorp Vault Database Secrets Engine
    /// // - AWS Secrets Manager with rotation
    /// // - Azure Key Vault dynamic secrets
    /// // - Custom dynamic secret management systems
    /// 
    /// // Future migration example:
    /// // builder.Services.TryAddSingleton&lt;IPvNugsDynamicSecretManager, HashiCorpVaultDynamicSecretManager&gt;();
    /// // All consuming code remains unchanged
    /// </code>
    /// </example>
    public static IServiceCollection TryAddPvNugsEnvVariablesDynamicSecretManager(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Validate that required dependencies exist
        if (services.All(x => x.ServiceType != typeof(ILoggerService)))
        {
            throw new InvalidOperationException(
                "ILoggerService must be registered before adding PvNugs Secret Manager. " +
                "Please register a logger service implementation first.");
        }
        services.TryAddSingleton<IConfiguration>(
            _ => configuration);
        
        services.Configure<PvNugsSecretManagerEnvVariablesConfig>(
            configuration.GetSection(nameof(PvNugsSecretManagerEnvVariablesConfig)));
        
        services.TryAddSingleton<
            IPvNugsDynamicSecretManager, DynamicSecretManager>();
        
        return services;
    }
    
}
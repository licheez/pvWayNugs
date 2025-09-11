using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using pvNugsLoggerNc9Abstractions;
using pvNugsSecretManagerNc9Abstractions;

namespace pvNugsSecretManagerNc9EnvVariables;

/// <summary>
/// Provides dependency injection extension methods for registering the pvNugs Secret Manager 
/// environment variables implementation with the Microsoft.Extensions.DependencyInjection container.
/// </summary>
/// <remarks>
/// <para>
/// This static class contains extension methods that simplify the registration of secret management
/// services in applications using the Microsoft dependency injection container. The extensions follow
/// the standard .NET pattern of "Try" methods that won't overwrite existing service registrations.
/// </para>
/// <para>
/// <strong>Services Registered:</strong>
/// </para>
/// <list type="bullet">
/// <item><see cref="IPvNugsStaticSecretManager"/> implemented by <see cref="StaticSecretManager"/></item>
/// <item><see cref="IPvNugsDynamicSecretManager"/> implemented by <see cref="DynamicSecretManager"/></item>
/// <item>Configuration binding for <see cref="PvNugsSecretManagerEnvVariablesConfig"/></item>
/// </list>
/// <para>
/// <strong>Service Lifetimes:</strong>
/// </para>
/// <para>
/// All secret manager services are registered as singletons because they are stateless and
/// depend only on configuration and logging services, making them safe and efficient to share
/// across the application lifetime.
/// </para>
/// <para>
/// <strong>Prerequisites:</strong>
/// </para>
/// <para>
/// Before calling these extension methods, you must ensure that an <see cref="ILoggerService"/>
/// implementation is registered in the service container, as the secret managers have a 
/// dependency on logging services.
/// </para>
/// <para>
/// <strong>Thread Safety:</strong>
/// </para>
/// <para>
/// The registration methods themselves are not thread-safe and should only be called during
/// application startup configuration. However, the registered services are designed to be
/// thread-safe for concurrent use.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Complete setup example in Program.cs
/// var builder = WebApplication.CreateBuilder(args);
/// 
/// // 1. First, register a logger service (required dependency)
/// builder.Services.TryAddPvNugsConsoleLoggerService(SeverityEnu.Debug);
/// 
/// // 2. Then register the secret manager services
/// builder.Services.TryAddPvNugsSecretManagerEnvVariables(builder.Configuration);
/// 
/// var app = builder.Build();
/// 
/// // Usage in controllers or services
/// app.MapGet("/test-secrets", async (
///     IPvNugsStaticSecretManager staticManager,
///     IPvNugsDynamicSecretManager dynamicManager) =&gt;
/// {
///     var staticSecret = await staticManager.GetStaticSecretAsync("ApiKey");
///     var dynamicCred = await dynamicManager.GetDynamicSecretAsync("DatabaseService");
///     
///     return new 
///     { 
///         HasApiKey = !string.IsNullOrEmpty(staticSecret),
///         HasDatabaseCred = dynamicCred != null 
///     };
/// });
/// 
/// // Example with configuration section
/// // appsettings.json:
/// // {
/// //   "PvNugsSecretManagerEnvVariablesConfig": {
/// //     "Prefix": "Production"
/// //   },
/// //   "Production": {
/// //     "ApiKey": "secret-api-key",
/// //     "DatabaseService__username": "dbuser",
/// //     "DatabaseService__password": "dbpass",
/// //     "DatabaseService__expirationDateUtc": "2024-12-31T23:59:59Z"
/// //   }
/// // }
/// 
/// // Alternative registration in a service class
/// public class MyService
/// {
///     public static void ConfigureServices(IServiceCollection services, IConfiguration config)
///     {
///         // Ensure logger is registered first
///         services.AddSingleton&lt;ILoggerService, MyCustomLogger&gt;();
///         
///         // Register secret management
///         services.TryAddPvNugsSecretManagerEnvVariables(config);
///         
///         // Other service registrations...
///     }
/// }
/// </code>
/// </example>

public static class PvNugsSecretManagerEnvVariablesDi
{
    /// <summary>
    /// Registers the pvNugs Secret Manager environment variables implementation services
    /// with the dependency injection container using configuration binding.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add the services to.</param>
    /// <param name="configuration">
    /// The configuration instance used to bind the <see cref="PvNugsSecretManagerEnvVariablesConfig"/>
    /// settings and provide secret values to the secret managers.
    /// </param>
    /// <returns>
    /// The same <see cref="IServiceCollection"/> instance for method chaining.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="services"/> or <paramref name="configuration"/> is null.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when no <see cref="ILoggerService"/> implementation is found in the service collection.
    /// This validation ensures that the required dependency is available before registering
    /// secret manager services that depend on logging functionality.
    /// </exception>
    /// <remarks>
    /// <para>
    /// This method performs the following registrations:
    /// </para>
    /// <list type="number">
    /// <item>Validates that <see cref="ILoggerService"/> is already registered</item>
    /// <item>Configures <see cref="PvNugsSecretManagerEnvVariablesConfig"/> by binding to the configuration section</item>
    /// <item>Registers <see cref="IPvNugsStaticSecretManager"/> with <see cref="StaticSecretManager"/> implementation as singleton</item>
    /// <item>Registers <see cref="IPvNugsDynamicSecretManager"/> with <see cref="DynamicSecretManager"/> implementation as singleton</item>
    /// </list>
    /// <para>
    /// <strong>Configuration Binding:</strong>
    /// </para>
    /// <para>
    /// The method automatically binds configuration from the section named by 
    /// <see cref="PvNugsSecretManagerEnvVariablesConfig.Section"/>. This provides type-safe
    /// access to configuration options throughout the secret manager implementation.
    /// </para>
    /// <para>
    /// <strong>Service Registration Behavior:</strong>
    /// </para>
    /// <para>
    /// Uses <c>TryAddSingleton</c> for service registrations, meaning:
    /// </para>
    /// <list type="bullet">
    /// <item>Services will only be registered if no existing registration is found</item>
    /// <item>Safe to call multiple times without creating duplicate registrations</item>
    /// <item>Allows for custom implementations to be registered first if needed</item>
    /// <item>Maintains singleton lifetime for optimal performance with stateless services</item>
    /// </list>
    /// <para>
    /// <strong>Error Prevention:</strong>
    /// </para>
    /// <para>
    /// The dependency validation prevents runtime errors that would occur when the secret
    /// managers attempt to resolve <see cref="ILoggerService"/> during construction. The
    /// validation provides clear guidance on how to resolve the dependency issue.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Basic registration
    /// var builder = WebApplication.CreateBuilder(args);
    /// 
    /// // Register logger first (required)
    /// builder.Services.AddSingleton&lt;ILoggerService, ConsoleLoggerService&gt;();
    /// 
    /// // Register secret manager services
    /// builder.Services.TryAddPvNugsSecretManagerEnvVariables(builder.Configuration);
    /// 
    /// // Build and use
    /// var app = builder.Build();
    /// 
    /// // Services are now available for injection
    /// var staticManager = app.Services.GetRequiredService&lt;IPvNugsStaticSecretManager&gt;();
    /// var dynamicManager = app.Services.GetRequiredService&lt;IPvNugsDynamicSecretManager&gt;();
    /// 
    /// // Configuration example for appsettings.json:
    /// // {
    /// //   "PvNugsSecretManagerEnvVariablesConfig": {
    /// //     "Prefix": "MyApp"
    /// //   },
    /// //   "MyApp": {
    /// //     "DatabasePassword": "secret123",
    /// //     "ApiService__username": "apiuser",
    /// //     "ApiService__password": "apipass",
    /// //     "ApiService__expirationDateUtc": "2024-06-01T00:00:00Z"
    /// //   }
    /// // }
    /// 
    /// // Environment variables alternative:
    /// // PvNugsSecretManagerEnvVariablesConfig__Prefix=MyApp
    /// // MyApp__DatabasePassword=secret123
    /// // MyApp__ApiService__username=apiuser
    /// // MyApp__ApiService__password=apipass
    /// // MyApp__ApiService__expirationDateUtc=2024-06-01T00:00:00Z
    /// 
    /// // Error handling example
    /// try
    /// {
    ///     services.TryAddPvNugsSecretManagerEnvVariables(configuration);
    /// }
    /// catch (InvalidOperationException ex)
    /// {
    ///     // Handle missing logger dependency
    ///     Console.WriteLine($"Setup error: {ex.Message}");
    ///     // Register logger and try again
    ///     services.AddSingleton&lt;ILoggerService, MyLoggerImplementation&gt;();
    ///     services.TryAddPvNugsSecretManagerEnvVariables(configuration);
    /// }
    /// </code>
    /// </example>
    public static IServiceCollection TryAddPvNugsSecretManagerEnvVariables(
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
            configuration.GetSection(PvNugsSecretManagerEnvVariablesConfig.Section));
        services.TryAddSingleton<
            IPvNugsStaticSecretManager, StaticSecretManager>();
        services.TryAddSingleton<
            IPvNugsDynamicSecretManager, DynamicSecretManager>();
        
        return services;
    }
}
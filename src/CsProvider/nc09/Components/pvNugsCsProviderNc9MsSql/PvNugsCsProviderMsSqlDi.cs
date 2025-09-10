using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using pvNugsCsProviderNc9Abstractions;
using Microsoft.Extensions.Options;
using pvNugsLoggerNc9Abstractions;
using pvNugsSecretManagerNc9Abstractions;

namespace pvNugsCsProviderNc9MsSql;

/// <summary>
/// Provides dependency injection configuration for the MsSQL connection string provider.
/// This static class extends <see cref="IServiceCollection"/> to register the <see cref="CsProvider"/> 
/// and its required configuration for MsSQL database connections with multiple authentication modes.
/// </summary>
/// <remarks>
/// <para><c>Required Dependencies:</c></para>
/// <list type="bullet">
/// <item><description><see cref="pvNugsLoggerNc9Abstractions.IConsoleLoggerService"/> - Mandatory logging service for error and diagnostic logging throughout the provider lifecycle.</description></item>
/// </list>
/// 
/// <para><c>Optional Dependencies (mode-specific):</c></para>
/// <list type="bullet">
/// <item><description><see cref="pvNugsSecretManagerNc9Abstractions.IPvNugsStaticSecretManager"/> - Optional service for StaticSecret mode, retrieves passwords from secure storage using static secret names.</description></item>
/// <item><description><see cref="pvNugsSecretManagerNc9Abstractions.IPvNugsDynamicSecretManager"/> - Optional service for DynamicSecret mode, generates temporary database credentials with automatic expiration and renewal.</description></item>
/// </list>
/// 
/// <para><c>Constructor Selection:</c></para>
/// <para>The <see cref="CsProvider"/> supports three operational modes based on which dependencies are registered:</para>
/// <list type="number">
/// <item><description><c>Config Mode:</c> Only <see cref="pvNugsLoggerNc9Abstractions.IConsoleLoggerService"/> is required. Credentials are read from configuration files.</description></item>
/// <item><description><c>StaticSecret Mode:</c> Requires <see cref="pvNugsLoggerNc9Abstractions.IConsoleLoggerService"/> and <see cref="pvNugsSecretManagerNc9Abstractions.IPvNugsStaticSecretManager"/>. Passwords are retrieved from secret storage.</description></item>
/// <item><description><c>DynamicSecret Mode:</c> Requires <see cref="pvNugsLoggerNc9Abstractions.IConsoleLoggerService"/> and <see cref="pvNugsSecretManagerNc9Abstractions.IPvNugsDynamicSecretManager"/>. Uses temporary credentials with automatic renewal.</description></item>
/// </list>
/// 
/// <para><c>Configuration:</c></para>
/// <para>The provider requires <see cref="PvNugsCsProviderMsSqlConfig"/> to be configured through the application's configuration system.
/// The configuration section name is defined by <see cref="PvNugsCsProviderMsSqlConfig.Section"/>.</para>
/// </remarks>
/// <example>
/// <para>Register for Config mode (configuration-based authentication):</para>
/// <code>
/// services.AddSingleton&lt;IConsoleLoggerService, ConsoleLoggerServiceImpl&gt;();
/// services.TryAddPvNugsCsProviderMsSql(configuration);
/// </code>
/// 
/// <para>Register for StaticSecret mode (secret manager with static secrets):</para>
/// <code>
/// services.AddSingleton&lt;IConsoleLoggerService, ConsoleLoggerServiceImpl&gt;();
/// services.AddSingleton&lt;IPvNugsStaticSecretManager, StaticSecretManagerImpl&gt;();
/// services.TryAddPvNugsCsProviderMsSql(configuration);
/// </code>
/// 
/// <para>Register for DynamicSecret mode (secret manager with dynamic credentials):</para>
/// <code>
/// services.AddSingleton&lt;IConsoleLoggerService, ConsoleLoggerServiceImpl&gt;();
/// services.AddSingleton&lt;IPvNugsDynamicSecretManager, DynamicSecretManagerImpl&gt;();
/// services.TryAddPvNugsCsProviderMsSql(configuration);
/// </code>
/// </example>
public static class PvNugsCsProviderMsSqlDi
{
    /// <summary>
    /// Registers the MsSQL connection string provider and its configuration with the dependency injection container.
    /// This method configures the provider as a singleton service implementing both <see cref="IPvNugsCsProvider"/> and <see cref="IPvNugsMsSqlCsProvider"/>,
    /// enabling role-based MsSQL connections with multiple authentication modes throughout your application.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add the services to.</param>
    /// <param name="config">The configuration instance containing the MsSQL provider settings from appsettings.json or other configuration sources.</param>
    /// <returns>The same <see cref="IServiceCollection"/> instance so that additional calls can be chained in a fluent manner.</returns>
    /// <remarks>
    /// <para><c>Service Registration:</c></para>
    /// <para>This method performs three key registrations:</para>
    /// <list type="number">
    /// <item><description>Configures <see cref="PvNugsCsProviderMsSqlConfig"/> using the Options pattern, binding to the configuration section specified by <see cref="PvNugsCsProviderMsSqlConfig.Section"/>.</description></item>
    /// <item><description>Registers <see cref="CsProvider"/> as a singleton implementation of <see cref="IPvNugsCsProvider"/> using factory-based constructor selection.</description></item>
    /// <item><description>Registers the same <see cref="CsProvider"/> instance as implementation of <see cref="IPvNugsMsSqlCsProvider"/> for typed access.</description></item>
    /// </list>
    /// 
    /// <para><c>Dependency Resolution and Mode Detection:</c></para>
    /// <para>The provider automatically selects the appropriate constructor based on configuration mode and registered dependencies:</para>
    /// <list type="bullet">
    /// <item><description><c>Config Mode:</c> Uses primary constructor with logger and configuration options only.</description></item>
    /// <item><description><c>StaticSecret Mode:</c> Uses constructor overload with <see cref="IPvNugsStaticSecretManager"/> for password retrieval.</description></item>
    /// <item><description><c>DynamicSecret Mode:</c> Uses constructor overload with <see cref="IPvNugsDynamicSecretManager"/> for dynamic credential generation.</description></item>
    /// </list>
    /// 
    /// <para><c>Configuration Requirements:</c></para>
    /// <para>The configuration section must contain appropriate settings based on the intended operational mode:</para>
    /// <list type="bullet">
    /// <item><description><c>All Modes:</c> Server, Database, and Mode properties are always required.</description></item>
    /// <item><description><c>Config Mode:</c> Additionally requires Username when not using integrated security. Password is optional for password-less authentication.</description></item>
    /// <item><description><c>StaticSecret Mode:</c> Additionally requires Username and SecretName for secret manager integration.</description></item>
    /// <item><description><c>DynamicSecret Mode:</c> Additionally requires SecretName. Username is ignored as it's dynamically generated.</description></item>
    /// </list>
    /// 
    /// <para><c>Secret Name Resolution:</c></para>
    /// <para>For StaticSecret and DynamicSecret modes, the provider constructs secret names using the pattern: <c>{config.SecretName}-{SqlRole}</c></para>
    /// <para>Where SqlRole can be Owner, Application, or Reader, allowing role-based credential management in your secret store.</para>
    /// 
    /// <para><c>Thread Safety and Lifecycle:</c></para>
    /// <para>The registered provider is thread-safe and designed as a singleton service. It uses internal caching and locking mechanisms 
    /// to ensure efficient and safe credential retrieval across multiple concurrent requests. Dynamic credentials are automatically 
    /// refreshed before expiration without blocking application operations.</para>
    /// 
    /// <para><c>Integration Patterns:</c></para>
    /// <para>After registration, inject <see cref="IPvNugsCsProvider"/> or <see cref="IPvNugsMsSqlCsProvider"/> into your services 
    /// to retrieve connection strings. The provider supports multiple SQL roles (Owner, Application, Reader) for implementing 
    /// principle of least privilege in database access.</para>
    /// </remarks>
    /// <exception cref="System.ArgumentNullException">Thrown when <paramref name="services"/> or <paramref name="config"/> is null.</exception>
    /// <exception cref="Microsoft.Extensions.Options.OptionsValidationException">
    /// Thrown during service resolution if the configuration is invalid for the selected mode 
    /// (e.g., missing required properties, invalid Mode value).
    /// </exception>
    /// <exception cref="System.InvalidOperationException">
    /// Thrown during service resolution if required dependencies are not registered in the container 
    /// (e.g., missing IConsoleLoggerService or required secret manager services).
    /// </exception>
    /// <example>
    /// <para><c>Basic registration for Config mode:</c></para>
    /// <code>
    /// public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    /// {
    ///     // Register required logger service
    ///     services.AddSingleton&lt;IConsoleLoggerService, ConsoleLoggerServiceImpl&gt;();
    ///     
    ///     // Register the MsSQL connection string provider
    ///     services.TryAddPvNugsCsProviderMsSql(configuration);
    ///     
    ///     // Now you can inject IPvNugsCsProvider in your services
    ///     services.AddScoped&lt;IDataService, DataService&gt;();
    /// }
    /// </code>
    /// 
    /// <para><c>Advanced registration for DynamicSecret mode with HashiCorp Vault:</c></para>
    /// <code>
    /// public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    /// {
    ///     // Register required dependencies
    ///     services.AddSingleton&lt;IConsoleLoggerService, ConsoleLoggerServiceImpl&gt;();
    ///     services.AddSingleton&lt;IPvNugsDynamicSecretManager, HashicorpVaultDynamicSecretManager&gt;();
    ///     
    ///     // Register the provider - it will automatically detect DynamicSecret mode
    ///     services.TryAddPvNugsCsProviderMsSql(configuration);
    /// }
    /// 
    /// // Usage in your service
    /// public class DataService
    /// {
    ///     public DataService(IPvNugsMsSqlCsProvider csProvider) { ... }
    ///     
    ///     public async Task&lt;List&lt;User&gt;&gt; GetUsersAsync()
    ///     {
    ///         var connectionString = await csProvider.GetConnectionStringAsync(SqlRoleEnu.Reader);
    ///         // Use connection string with SqlConnection...
    ///     }
    /// }
    /// </code>
    /// 
    /// <para><c>Configuration example (appsettings.json):</c></para>
    /// <code>
    /// {
    ///   "PvNugsCsProviderMsSqlConfig": {
    ///     "Mode": "DynamicSecret",
    ///     "Server": "mydb.database.windows.net",
    ///     "Database": "myapp_production",
    ///     "Port": 1433,
    ///     "SecretName": "myapp-sqlserver",
    ///     "ApplicationName": "MyApp",
    ///     "TimeoutInSeconds": 30
    ///   }
    /// }
    /// </code>
    /// </example>
    /// <seealso cref="CsProvider"/>
    /// <seealso cref="IPvNugsCsProvider"/>
    /// <seealso cref="IPvNugsMsSqlCsProvider"/>
    /// <seealso cref="PvNugsCsProviderMsSqlConfig"/>
    /// <seealso cref="CsProviderModeEnu"/>
    /// <seealso cref="pvNugsLoggerNc9Abstractions.IConsoleLoggerService"/>
    /// <seealso cref="pvNugsSecretManagerNc9Abstractions.IPvNugsStaticSecretManager"/>
    /// <seealso cref="pvNugsSecretManagerNc9Abstractions.IPvNugsDynamicSecretManager"/>
    public static IServiceCollection TryAddPvNugsCsProviderMsSql(
        this IServiceCollection services, IConfiguration config)
    {
        // Configure options with validation
        services.Configure<PvNugsCsProviderMsSqlConfig>(configSection =>
        {
            config.GetSection(PvNugsCsProviderMsSqlConfig.Section).Bind(configSection);
            
            // Early validation
            ValidateConfiguration(configSection);
        });
        
        // Factory-based registration for mode-specific constructor selection
        services.TryAddSingleton<IPvNugsCsProvider>(serviceProvider =>
        {
            try 
            {
                return CreateProvider(serviceProvider);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    "Failed to create SQL Server connection string provider. " +
                    "Ensure all required dependencies are registered and configuration is valid.", ex);
            }
        });
        
        // Register specific interface
        services.TryAddSingleton<IPvNugsMsSqlCsProvider>(serviceProvider => 
            (CsProvider)serviceProvider.GetRequiredService<IPvNugsCsProvider>());
        
        return services;
    }
    
    /// <summary>
    /// Creates a new instance of <see cref="CsProvider"/> using the appropriate constructor based on configuration mode and available services.
    /// This method acts as a factory that examines the configuration and registered services to select the correct operational mode.
    /// </summary>
    /// <param name="serviceProvider">The service provider to resolve dependencies from.</param>
    /// <returns>A fully configured <see cref="CsProvider"/> instance ready for use.</returns>
    /// <remarks>
    /// <para><c>Constructor Selection Logic:</c></para>
    /// <list type="bullet">
    /// <item><description><c>Config Mode:</c> Uses primary constructor when mode is Config, regardless of registered secret managers.</description></item>
    /// <item><description><c>StaticSecret Mode:</c> Uses StaticSecret constructor when mode is StaticSecret and <see cref="IPvNugsStaticSecretManager"/> is registered.</description></item>
    /// <item><description><c>DynamicSecret Mode:</c> Uses DynamicSecret constructor when mode is DynamicSecret and <see cref="IPvNugsDynamicSecretManager"/> is registered.</description></item>
    /// </list>
    /// 
    /// <para><c>Error Conditions:</c></para>
    /// <para>This method will throw exceptions if the configuration mode requires services that are not registered in the container.</para>
    /// </remarks>
    /// <exception cref="InvalidOperationException">
    /// Thrown when required dependencies are missing for the configured mode, or when an unsupported mode is specified.
    /// </exception>
    private static CsProvider CreateProvider(IServiceProvider serviceProvider)
    {
        var logger = serviceProvider.GetRequiredService<IConsoleLoggerService>();
        var options = serviceProvider.GetRequiredService<IOptions<PvNugsCsProviderMsSqlConfig>>();
        var config = options.Value;
        
        // Mode-specific factory logic
        return config.Mode switch
        {
            CsProviderModeEnu.Config => 
                new CsProvider(logger, options),
                
            CsProviderModeEnu.StaticSecret => CreateStaticSecretProvider(
                serviceProvider, logger, options),
                
            CsProviderModeEnu.DynamicSecret => CreateDynamicSecretProvider(
                serviceProvider, logger, options),
                
            _ => throw new ArgumentOutOfRangeException(
                $"Unsupported authentication mode: {config.Mode}")
        };
    }
    
    /// <summary>
    /// Creates a <see cref="CsProvider"/> instance configured for StaticSecret mode.
    /// This method validates that the required <see cref="IPvNugsStaticSecretManager"/> is registered
    /// and creates the provider using the StaticSecret constructor overload.
    /// </summary>
    /// <param name="serviceProvider">The service provider to resolve dependencies from.</param>
    /// <param name="logger">The console logger service for error and diagnostic logging.</param>
    /// <param name="options">Configuration options containing database connection parameters and secret settings.</param>
    /// <returns>A <see cref="CsProvider"/> instance configured for StaticSecret operations.</returns>
    /// <remarks>
    /// <para>StaticSecret mode requires the following services to be registered:</para>
    /// <list type="bullet">
    /// <item><description><see cref="IConsoleLoggerService"/> - Required for all modes.</description></item>
    /// <item><description><see cref="IPvNugsStaticSecretManager"/> - Required specifically for StaticSecret mode to retrieve passwords from secure storage.</description></item>
    /// </list>
    /// 
    /// <para>In StaticSecret mode, the provider retrieves passwords from the secret manager using role-specific secret names
    /// following the pattern: <c>{config.SecretName}-{role}</c>, while usernames come from configuration.</para>
    /// </remarks>
    /// <exception cref="InvalidOperationException">
    /// Thrown when <see cref="IPvNugsStaticSecretManager"/> is not registered in the service container.
    /// The exception message provides guidance on how to register the missing service.
    /// </exception>
    private static CsProvider CreateStaticSecretProvider(
        IServiceProvider serviceProvider,
        IConsoleLoggerService logger,
        IOptions<PvNugsCsProviderMsSqlConfig> options)
    {
        var secretManager = serviceProvider.GetService<IPvNugsStaticSecretManager>();
        if (secretManager == null)
        {
            throw new InvalidOperationException(
                "StaticSecret mode requires IPvNugsStaticSecretManager to be registered. " +
                "Register it with: services.AddSingleton<IPvNugsStaticSecretManager, YourImplementation>()");
        }
        
        return new CsProvider(logger, options, secretManager);
    }
    
    /// <summary>
    /// Creates a <see cref="CsProvider"/> instance configured for DynamicSecret mode.
    /// This method validates that the required <see cref="IPvNugsDynamicSecretManager"/> is registered
    /// and creates the provider using the DynamicSecret constructor overload.
    /// </summary>
    /// <param name="serviceProvider">The service provider to resolve dependencies from.</param>
    /// <param name="logger">The console logger service for error and diagnostic logging.</param>
    /// <param name="options">Configuration options containing database connection parameters and secret settings.</param>
    /// <returns>A <see cref="CsProvider"/> instance configured for DynamicSecret operations with automatic credential renewal.</returns>
    /// <remarks>
    /// <para>DynamicSecret mode requires the following services to be registered:</para>
    /// <list type="bullet">
    /// <item><description><see cref="IConsoleLoggerService"/> - Required for all modes.</description></item>
    /// <item><description><see cref="IPvNugsDynamicSecretManager"/> - Required specifically for DynamicSecret mode to generate temporary database credentials.</description></item>
    /// </list>
    /// 
    /// <para>In DynamicSecret mode, the provider requests fresh credentials from the secret manager using role-specific secret names
    /// following the pattern: <c>{config.SecretName}-{role}</c>. Both username and password are dynamically generated
    /// and include expiration times for automatic renewal.</para>
    /// 
    /// <para>The provider automatically handles credential expiration and renewal, ensuring applications always have valid credentials
    /// without manual intervention or service restarts.</para>
    /// </remarks>
    /// <exception cref="InvalidOperationException">
    /// Thrown when <see cref="IPvNugsDynamicSecretManager"/> is not registered in the service container.
    /// The exception message provides guidance on how to register the missing service.
    /// </exception>
    private static CsProvider CreateDynamicSecretProvider(
        IServiceProvider serviceProvider,
        IConsoleLoggerService logger,
        IOptions<PvNugsCsProviderMsSqlConfig> options)
    {
        var secretManager = serviceProvider.GetService<IPvNugsDynamicSecretManager>();
        if (secretManager == null)
        {
            throw new InvalidOperationException(
                "DynamicSecret mode requires IPvNugsDynamicSecretManager to be registered. " +
                "Register it with: services.AddSingleton<IPvNugsDynamicSecretManager, YourImplementation>()");
        }
        
        return new CsProvider(logger, options, secretManager);
    }
    
    /// <summary>
    /// Validates the configuration settings for the MsSQL connection string provider.
    /// This method performs early validation to catch configuration errors during application startup
    /// rather than at runtime when connection strings are first requested.
    /// </summary>
    /// <param name="config">The configuration object to validate.</param>
    /// <remarks>
    /// <para><c>Validation Rules:</c></para>
    /// <para>This method enforces both universal and mode-specific validation rules:</para>
    /// 
    /// <para><c>Universal Requirements:</c></para>
    /// <list type="bullet">
    /// <item><description>Server property must not be null or whitespace.</description></item>
    /// <item><description>Database property must not be null or whitespace.</description></item>
    /// </list>
    /// 
    /// <para><c>Config Mode Requirements:</c></para>
    /// <list type="bullet">
    /// <item><description>Username is required when UseIntegratedSecurity is false.</description></item>
    /// <item><description>Password is optional (supports password-less authentication scenarios).</description></item>
    /// </list>
    /// 
    /// <para><c>StaticSecret Mode Requirements:</c></para>
    /// <list type="bullet">
    /// <item><description>Username is required (retrieved from configuration).</description></item>
    /// <item><description>SecretName is required (used to construct role-specific secret names).</description></item>
    /// </list>
    /// 
    /// <para><c>DynamicSecret Mode Requirements:</c></para>
    /// <list type="bullet">
    /// <item><description>SecretName is required (used to construct role-specific secret names).</description></item>
    /// <item><description>Username is ignored (dynamically generated by the secret manager).</description></item>
    /// </list>
    /// 
    /// <para><c>Windows Authentication:</c></para>
    /// <para>When UseIntegratedSecurity is true, Username and Password requirements are relaxed as they are not used
    /// for authentication. The provider will use the current Windows user context instead.</para>
    /// </remarks>
    /// <exception cref="ArgumentException">
    /// Thrown when required configuration properties are missing or invalid for the specified mode.
    /// The exception message indicates which property is missing and for which mode.
    /// </exception>
    private static void ValidateConfiguration(PvNugsCsProviderMsSqlConfig config)
    {
        if (string.IsNullOrWhiteSpace(config.Server))
            throw new ArgumentException("Server is required in configuration");
            
        if (string.IsNullOrWhiteSpace(config.Database))
            throw new ArgumentException("Database is required in configuration");
            
        // Mode-specific validation
        switch (config.Mode)
        {
            case CsProviderModeEnu.Config when !config.UseIntegratedSecurity 
                                             && string.IsNullOrWhiteSpace(config.Username):
                throw new ArgumentException("Username is required for Config mode without integrated security");
                
            case CsProviderModeEnu.StaticSecret when string.IsNullOrWhiteSpace(config.Username):
                throw new ArgumentException("Username is required for StaticSecret mode");
                
            case CsProviderModeEnu.StaticSecret when string.IsNullOrWhiteSpace(config.SecretName):
                throw new ArgumentException("SecretName is required for StaticSecret mode");
                
            case CsProviderModeEnu.DynamicSecret when string.IsNullOrWhiteSpace(config.SecretName):
                throw new ArgumentException("SecretName is required for DynamicSecret mode");
        }
    }
}
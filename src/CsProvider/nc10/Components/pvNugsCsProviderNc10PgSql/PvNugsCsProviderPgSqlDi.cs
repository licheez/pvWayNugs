using System.Runtime.CompilerServices;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using pvNugsCsProviderNc10Abstractions;
using pvNugsLoggerNc10Abstractions;
using pvNugsSecretManagerNc10Abstractions;

namespace pvNugsCsProviderNc10PgSql;

/// <summary>
/// Provides dependency injection configuration for the PostgreSQL connection string provider.
/// This static class extends <see cref="IServiceCollection"/> to register the <see cref="CsProvider"/> 
/// and its required configuration for PostgreSQL database connections with multiple authentication modes.
/// </summary>
/// <remarks>
/// <para><strong>Required Dependencies:</strong></para>
/// <list type="bullet">
/// <item><description><see cref="IConsoleLoggerService"/> - Mandatory logging service for error and diagnostic logging throughout the provider lifecycle.</description></item>
/// </list>
/// 
/// <para><strong>Optional Dependencies (mode-specific):</strong></para>
/// <list type="bullet">
/// <item><description><see cref="pvNugsSecretManagerNc10Abstractions.IPvNugsSecretManager"/> - Required for StaticSecret and DynamicSecret modes. For DynamicSecret mode, must have SupportsDatabaseSecrets = true.</description></item>
/// </list>
/// 
/// <para><strong>Mode Selection:</strong></para>
/// <para>The provider operates in one of three modes based on configuration:</para>
/// <list type="number">
/// <item><description><strong>Config Mode:</strong> Uses credentials from configuration. No secret manager required.</description></item>
/// <item><description><strong>StaticSecret Mode:</strong> Requires IPvNugsSecretManager. Uses GetStaticSecretAsync to retrieve passwords.</description></item>
/// <item><description><strong>DynamicSecret Mode:</strong> Requires IPvNugsSecretManager with SupportsDatabaseSecrets = true. Uses GetDynamicSecretAsync for time-limited credentials with automatic renewal.</description></item>
/// </list>
/// 
/// <para><strong>Configuration:</strong></para>
/// <para>The provider requires <see cref="PvNugsCsProviderPgSqlConfig"/> to be configured through the application's configuration system.
/// The configuration section name is defined by <see cref="PvNugsCsProviderPgSqlConfig.Section"/>. The Mode property determines which constructor and credential retrieval strategy will be used.</para>
/// 
/// <para><strong>Thread Safety and Lifecycle:</strong></para>
/// <para>The provider is thread-safe and designed as a singleton service. The factory pattern ensures that only one provider instance is created per application lifetime, with proper dependency resolution occurring at startup time.</para>
/// </remarks>
/// <example>
/// <para><strong>Register for Config mode (configuration-based authentication):</strong></para>
/// <code>
/// services.AddSingleton&lt;IConsoleLoggerService, ConsoleLoggerServiceImpl&gt;();
/// services.TryAddPvNugsCsProviderPgSql(configuration);
/// // No secret manager needed - uses credentials from configuration
/// </code>
/// 
/// <para><strong>Register for StaticSecret mode (secret manager with static secrets):</strong></para>
/// <code>
/// services.AddSingleton&lt;IConsoleLoggerService, ConsoleLoggerServiceImpl&gt;();
/// services.AddSingleton&lt;IPvNugsSecretManager, KeyVaultSecretManager&gt;();
/// services.TryAddPvNugsCsProviderPgSql(configuration);
/// // Provider will use GetStaticSecretAsync for passwords
/// </code>
/// 
/// <para><strong>Register for DynamicSecret mode (secret manager with dynamic credentials):</strong></para>
/// <code>
/// services.AddSingleton&lt;IConsoleLoggerService, ConsoleLoggerServiceImpl&gt;();
/// services.AddSingleton&lt;IPvNugsSecretManager, HashiCorpVaultSecretManager&gt;(); // Must have SupportsDatabaseSecrets = true
/// services.TryAddPvNugsCsProviderPgSql(configuration);
/// // Provider will use GetDynamicSecretAsync for username/password pairs with expiration
/// </code>
/// </example>
public static class PvNugsCsProviderPgSqlDi
{
    /// <summary>
    /// Registers the PostgreSQL connection string provider and its configuration with the dependency injection container.
    /// This method configures the provider as a singleton service implementing <see cref="IPvNugsCsProvider"/>,
    /// using a factory pattern to construct the provider based on the configured mode.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add the services to.</param>
    /// <param name="config">The configuration instance containing the PostgreSQL provider settings from appsettings.json or other configuration sources.</param>
    /// <returns>The same <see cref="IServiceCollection"/> instance so that additional calls can be chained in a fluent manner.</returns>
    /// <remarks>
    /// <para><strong>Service Registration Process:</strong></para>
    /// <para>This method performs two key registrations:</para>
    /// <list type="number">
    /// <item><description><strong>Configuration Binding:</strong> Configures <see cref="PvNugsCsProviderPgSqlConfig"/> using the Options pattern, binding to the configuration section specified by <see cref="PvNugsCsProviderPgSqlConfig.Section"/>.</description></item>
    /// <item><description><strong>Factory Registration:</strong> Registers a factory function that constructs the <see cref="CsProvider"/> based on the configured mode.</description></item>
    /// </list>
    /// 
    /// <para><strong>Mode-Based Constructor Selection:</strong></para>
    /// <para><strong>Mode-Based Constructor Selection:</strong></para>
    /// <para>The factory function selects the appropriate constructor based on the configured mode:</para>
    /// <list type="bullet">
    /// <item><description><strong>Config Mode:</strong> Uses the primary constructor (logger, options). No secret manager required.</description></item>
    /// <item><description><strong>StaticSecret Mode:</strong> Uses the secondary constructor (logger, options, secretManager). Validates that IPvNugsSecretManager is registered.</description></item>
    /// <item><description><strong>DynamicSecret Mode:</strong> Uses the secondary constructor (logger, options, secretManager). Validates that IPvNugsSecretManager is registered AND has SupportsDatabaseSecrets = true.</description></item>
    /// </list>
    /// 
    /// <para><strong>Configuration Requirements by Mode:</strong></para>
    /// <para>The configuration section must contain appropriate settings based on the selected mode:</para>
    /// <list type="bullet">
    /// <item><description><strong>All Modes:</strong> Server, Database, Schema, and Mode properties are always required.</description></item>
    /// <item><description><strong>Config Mode:</strong> Additionally requires Username. Password is optional for password-less authentication.</description></item>
    /// <item><description><strong>StaticSecret Mode:</strong> Additionally requires Username and SecretName for secret manager integration.</description></item>
    /// <item><description><strong>DynamicSecret Mode:</strong> Additionally requires SecretName. Username is ignored as it's dynamically generated.</description></item>
    /// </list>
    /// 
    /// <para><strong>Factory Pattern Implementation Details:</strong></para>
    /// <para>The factory function uses the configured mode to determine which constructor to call, 
    /// validating that required dependencies are available using <see cref="Microsoft.Extensions.DependencyInjection.ServiceProviderServiceExtensions.GetRequiredService{T}(IServiceProvider)"/> 
    /// for mandatory dependencies, ensuring proper exception handling for missing required services.</para>
    /// 
    /// <para><strong>Secret Parameters:</strong></para>
    /// <para>For StaticSecret and DynamicSecret modes, role-specific SecretParams dictionaries (ReaderSecretParams, ApplicationSecretParams, OwnerSecretParams) 
    /// are passed directly to the IPvNugsSecretManager without modification. The expected dictionary keys depend entirely on your secret manager provider implementation 
    /// (e.g., Azure Key Vault, HashiCorp Vault, Environment Variables). Consult your provider's documentation for required parameter keys.</para>
    /// 
    /// <para><strong>Singleton Lifecycle Management:</strong></para>
    /// <para>The provider is designed as a singleton service with internal caching and locking mechanisms 
    /// to ensure efficient and safe credential retrieval across multiple concurrent requests. Dynamic credentials are automatically 
    /// refreshed before expiration without blocking application operations.</para>
    /// 
    /// <para><strong>Integration Patterns:</strong></para>
    /// <para>After registration, inject <see cref="IPvNugsCsProvider"/> or <see cref="IPvNugsPgSqlCsProvider"/> into your services 
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
    /// (e.g., missing IConsoleLoggerService, missing IPvNugsSecretManager for StaticSecret/DynamicSecret modes, 
    /// or IPvNugsSecretManager.SupportsDatabaseSecrets = false for DynamicSecret mode).
    /// </exception>
    /// <example>
    /// <para><strong>Basic registration with Config mode:</strong></para>
    /// <code>
    /// public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    /// {
    ///     // Register required logger service
    ///     services.AddSingleton&lt;IConsoleLoggerService, ConsoleLoggerServiceImpl&gt;();
    ///     
    ///     // Register the PostgreSQL connection string provider (Mode = Config in appsettings.json)
    ///     services.TryAddPvNugsCsProviderPgSql(configuration);
    ///     
    ///     // Now you can inject IPvNugsCsProvider in your services
    ///     services.AddScoped&lt;IDataService, DataService&gt;();
    /// }
    /// </code>
    /// 
    /// <para><strong>Advanced registration with DynamicSecret mode:</strong></para>
    /// <code>
    /// public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    /// {
    ///     // Register required dependencies
    ///     services.AddSingleton&lt;IConsoleLoggerService, ConsoleLoggerServiceImpl&gt;();
    ///     services.AddSingleton&lt;IPvNugsSecretManager, HashiCorpVaultSecretManager&gt;(); // Must have SupportsDatabaseSecrets = true
    ///     
    ///     // Register the provider (Mode = DynamicSecret in appsettings.json)
    ///     services.TryAddPvNugsCsProviderPgSql(configuration);
    /// }
    /// 
    /// public class DataService
    /// {
    ///     public DataService(IPvNugsPgSqlCsProvider csProvider) { ... }
    ///     
    ///     public async Task&lt;List&lt;User&gt;&gt; GetUsersAsync()
    ///     {
    ///         // This will use dynamic credentials with automatic renewal
    ///         var connectionString = await csProvider.GetConnectionStringAsync(SqlRoleEnu.Reader);
    ///         // Use connection string with Npgsql...
    ///     }
    /// }
    /// </code>
    /// 
    /// <para><strong>Configuration example supporting different modes:</strong></para>
    /// <code>
    /// {
    ///   "PvNugsCsProviderPgSqlConfig": {
    ///     "Mode": "DynamicSecret",  // Can be "Config", "StaticSecret", or "DynamicSecret"
    ///     "Server": "mydb.postgres.database.azure.com",
    ///     "Database": "myapp_production",
    ///     "Schema": "app_schema",
    ///     "Port": 5432,
    ///     "Username": "fallback_user",           // Required for Config and StaticSecret modes
    ///     "ReaderSecretParams": {                // Required for StaticSecret and DynamicSecret modes
    ///       // Keys depend on your secret manager provider
    ///       // HashiCorp Vault example: "mountPoint": "database", "role": "myapp-reader"
    ///       // Azure Key Vault example: "name": "myapp-postgres-reader"
    ///     },
    ///     "ApplicationSecretParams": { },        // Provider-specific keys
    ///     "OwnerSecretParams": { },              // Provider-specific keys
    ///     "Timezone": "UTC",
    ///     "TimeoutInSeconds": 30,
    ///     "ExpirationWarningToleranceInMinutes": 30,  // DynamicSecret mode only
    ///     "ExpirationErrorToleranceInMinutes": 5      // DynamicSecret mode only
    ///   }
    /// }
    /// </code>
    /// </example>
    /// <seealso cref="CsProvider"/>
    /// <seealso cref="IPvNugsCsProvider"/>
    /// <seealso cref="IPvNugsPgSqlCsProvider"/>
    /// <seealso cref="PvNugsCsProviderPgSqlConfig"/>
    /// <seealso cref="CsProviderModeEnu"/>
    /// <seealso cref="IConsoleLoggerService"/>
    /// <seealso cref="pvNugsSecretManagerNc10Abstractions.IPvNugsSecretManager"/>
    public static IServiceCollection TryAddPvNugsCsProviderPgSql(
        this IServiceCollection services, IConfiguration config)
    {
        // Configure options with validation
        services.Configure<PvNugsCsProviderPgSqlConfig>(configSection =>
        {
            config.GetSection(PvNugsCsProviderPgSqlConfig.Section)
                .Bind(configSection);
            var configRows = configSection.Rows ?? [];
            foreach (var configRow in configRows)
            {
                ValidateConfiguration(configRow);
            }
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
                    "Failed to create PostgreSQL connection string provider. " +
                    "Ensure all required dependencies are registered and configuration is valid.", ex);
            }
        });

        // Register specific interface
        services.TryAddSingleton<IPvNugsPgSqlCsProvider>(serviceProvider =>
            (CsProvider)serviceProvider.GetRequiredService<IPvNugsCsProvider>());

        return services;
    }

    /// <summary>
    /// Factory method that creates an instance of <see cref="CsProvider"/>
    /// based on the registered dependencies and configuration.
    /// </summary>
    /// <param name="serviceProvider">The service provider used to resolve dependencies.</param>
    /// <returns>An instance of <see cref="CsProvider"/>.</returns>
    /// <exception cref="InvalidOperationException"></exception>
    /// <exception cref="SwitchExpressionException"></exception>
    private static CsProvider CreateProvider(IServiceProvider serviceProvider)
    {
        var logger = serviceProvider.GetRequiredService<IConsoleLoggerService>();
        var options = serviceProvider.GetRequiredService<IOptions<PvNugsCsProviderPgSqlConfig>>();
        var secretManager = serviceProvider.GetService<IPvNugsSecretManager>();
        var config = options.Value;

        // Mode-specific factory logic
        switch (config.Mode)
        {
            case CsProviderModeEnu.Config:
                return new CsProvider(logger, options);
            
            case CsProviderModeEnu.StaticSecret:
                if (secretManager == null)
                {
                    throw new InvalidOperationException(
                        $"Mode {config.Mode} requires a registered IPvNugsSecretManager. " +
                        "Register it with: services.AddSingleton<IPvNugsSecretManager, YourImplementation>()");
                }
                return new CsProvider(logger, options, secretManager);
            
            case CsProviderModeEnu.DynamicSecret:
                if (secretManager == null)
                {
                    throw new InvalidOperationException(
                        $"Mode {config.Mode} requires a registered IPvNugsSecretManager. " +
                        "Register it with: services.AddSingleton<IPvNugsSecretManager, YourImplementation>()");
                }
                if (!secretManager.SupportsDatabaseSecrets)
                {
                    throw new InvalidOperationException(
                        $"Mode {config.Mode} requires a secret manager that supports dynamic database secrets. " +
                        "Ensure your implementation of IPvNugsSecretManager has SupportsDatabaseSecrets = true.");
                }
                return new CsProvider(logger, options, secretManager);
            
            default:
                throw new SwitchExpressionException(
                    $"Unsupported mode: {config.Mode}. Valid modes are: Config, StaticSecret, DynamicSecret.");
        }
    }

    /// <summary>
    /// Validates the configuration row for required properties based on the selected mode.
    /// </summary>
    /// <param name="configRow">The configuration row to validate.</param>
    /// <exception cref="OptionsValidationException">Thrown when a required property is missing or invalid.</exception>
    private static void ValidateConfiguration(PvNugsCsProviderPgSqlConfigRow configRow)
    {
        if (string.IsNullOrWhiteSpace(configRow.Name))
            throw new OptionsValidationException(
                "Name is required for each configuration row.", 
                typeof(PvNugsCsProviderPgSqlConfigRow), ["Name"]);
        if (string.IsNullOrWhiteSpace(configRow.Server))
            throw new OptionsValidationException(
                "Server is required for each configuration row.", 
                typeof(PvNugsCsProviderPgSqlConfigRow), ["Server"]);
        if (string.IsNullOrWhiteSpace(configRow.Database))
            throw new OptionsValidationException(
                "Database is required for each configuration row.", 
                typeof(PvNugsCsProviderPgSqlConfigRow), ["Database"]);
        if (string.IsNullOrWhiteSpace(configRow.Schema))
            throw new OptionsValidationException(
                "Schema is required for each configuration row.", 
                typeof(PvNugsCsProviderPgSqlConfigRow), ["Schema"]);
        
        switch (configRow.Mode)
        {
            case CsProviderModeEnu.Config:
                // Username is required in Config mode
                if (string.IsNullOrWhiteSpace(configRow.Username))
                    throw new OptionsValidationException(
                        "Username is required in Config mode.", 
                        typeof(PvNugsCsProviderPgSqlConfigRow),
                        ["Username"]);
                // Password is optional
                break;
            
            case CsProviderModeEnu.StaticSecret:
                // Username is required in StaticSecret mode
                if (string.IsNullOrWhiteSpace(configRow.Username))
                    throw new OptionsValidationException(
                        "Username is required in StaticSecret mode.", 
                        typeof(PvNugsCsProviderPgSqlConfigRow),
                        ["Username"]);
                // at least one SecretParams dictionary is required
                if (configRow.ReaderSecretParams == null
                    && configRow.ApplicationSecretParams == null
                    && configRow.OwnerSecretParams == null)
                    throw new OptionsValidationException(
                        "at least one SecretParams dictionary is required in StaticSecret mode.", 
                        typeof(PvNugsCsProviderPgSqlConfigRow),
                        ["ReaderSecretParams or ApplicationSecretParams or OwnerSecretParams"]);
                break;
            
            case CsProviderModeEnu.DynamicSecret:
                // Username is ignored as it is dynamically generated
                // at least one SecretParams dictionary is required
                if (configRow.ReaderSecretParams == null
                    && configRow.ApplicationSecretParams == null
                    && configRow.OwnerSecretParams == null)
                    throw new OptionsValidationException(
                        "at least one SecretParams dictionary is required in DynamicSecret mode.", 
                        typeof(PvNugsCsProviderPgSqlConfigRow),
                        ["ReaderSecretParams or ApplicationSecretParams or OwnerSecretParams"]);
                break;
            
            default:
                throw new OptionsValidationException(
                    $"Unsupported mode: {configRow.Mode}", 
                    typeof(PvNugsCsProviderPgSqlConfigRow),
                    ["Mode"]);
        }
    }
}
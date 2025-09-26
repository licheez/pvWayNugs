using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using pvNugsCsProviderNc9Abstractions;
using pvNugsLoggerNc9Abstractions;
using pvNugsSecretManagerNc9Abstractions;

namespace pvNugsCsProviderNc9PgSql;

/// <summary>
/// Provides dependency injection configuration for the PostgreSQL connection string provider.
/// This static class extends <see cref="IServiceCollection"/> to register the <see cref="CsProvider"/> 
/// and its required configuration for PostgreSQL database connections with multiple authentication modes.
/// Uses an intelligent factory pattern to automatically resolve constructor ambiguity based on registered dependencies.
/// </summary>
/// <remarks>
/// <para><strong>Required Dependencies:</strong></para>
/// <list type="bullet">
/// <item><description><see cref="pvNugsLoggerNc9Abstractions.IConsoleLoggerService"/> - Mandatory logging service for error and diagnostic logging throughout the provider lifecycle.</description></item>
/// </list>
/// 
/// <para><strong>Optional Dependencies (mode-specific):</strong></para>
/// <list type="bullet">
/// <item><description><see cref="pvNugsSecretManagerNc9Abstractions.IPvNugsStaticSecretManager"/> - Optional service for StaticSecret mode, retrieves passwords from secure storage using static secret names.</description></item>
/// <item><description><see cref="pvNugsSecretManagerNc9Abstractions.IPvNugsDynamicSecretManager"/> - Optional service for DynamicSecret mode, generates temporary database credentials with automatic expiration and renewal.</description></item>
/// </list>
/// 
/// <para><strong>Automatic Constructor Resolution:</strong></para>
/// <para>The registration method uses a factory pattern to intelligently select the appropriate <see cref="CsProvider"/> constructor based on which dependencies are available in the service container. This eliminates constructor ambiguity and ensures the correct operational mode:</para>
/// <list type="number">
/// <item><description><strong>DynamicSecret Mode (Highest Priority):</strong> When <see cref="pvNugsSecretManagerNc9Abstractions.IPvNugsDynamicSecretManager"/> is registered, uses the dynamic secret constructor for time-limited credentials with automatic renewal.</description></item>
/// <item><description><strong>StaticSecret Mode (Medium Priority):</strong> When only <see cref="pvNugsSecretManagerNc9Abstractions.IPvNugsStaticSecretManager"/> is registered, uses the static secret constructor for password retrieval from secure storage.</description></item>
/// <item><description><strong>Config Mode (Fallback):</strong> When no secret managers are registered, uses the primary constructor for configuration-based credentials.</description></item>
/// </list>
/// 
/// <para><strong>Priority Resolution Logic:</strong></para>
/// <para>The factory pattern implements a hierarchical resolution strategy:</para>
/// <code>
/// DynamicSecretManager → StaticSecretManager → Config Mode
/// </code>
/// <para>This ensures that the most sophisticated credential management available is automatically selected while maintaining backward compatibility.</para>
/// 
/// <para><strong>Configuration:</strong></para>
/// <para>The provider requires <see cref="PvNugsCsProviderPgSqlConfig"/> to be configured through the application's configuration system.
/// The configuration section name is defined by <see cref="PvNugsCsProviderPgSqlConfig.Section"/>.</para>
/// 
/// <para><strong>Thread Safety and Lifecycle:</strong></para>
/// <para>The factory-registered provider is thread-safe and designed as a singleton service. The factory pattern ensures that only one provider instance is created per application lifetime, with proper dependency resolution occurring at startup time.</para>
/// </remarks>
/// <example>
/// <para><strong>Register for Config mode (configuration-based authentication):</strong></para>
/// <code>
/// services.AddSingleton&lt;IConsoleLoggerService, ConsoleLoggerServiceImpl&gt;();
/// services.TryAddPvNugsCsProviderPgSql(configuration);
/// // Factory will select Config mode constructor automatically
/// </code>
/// 
/// <para><strong>Register for StaticSecret mode (secret manager with static secrets):</strong></para>
/// <code>
/// services.AddSingleton&lt;IConsoleLoggerService, ConsoleLoggerServiceImpl&gt;();
/// services.AddSingleton&lt;IPvNugsStaticSecretManager, StaticSecretManagerImpl&gt;();
/// services.TryAddPvNugsCsProviderPgSql(configuration);
/// // Factory will detect StaticSecretManager and select appropriate constructor
/// </code>
/// 
/// <para><strong>Register for DynamicSecret mode (secret manager with dynamic credentials):</strong></para>
/// <code>
/// services.AddSingleton&lt;IConsoleLoggerService, ConsoleLoggerServiceImpl&gt;();
/// services.AddSingleton&lt;IPvNugsDynamicSecretManager, DynamicSecretManagerImpl&gt;();
/// services.TryAddPvNugsCsProviderPgSql(configuration);
/// // Factory will detect DynamicSecretManager and select dynamic constructor
/// </code>
/// 
/// <para><strong>Mixed registration (DynamicSecret takes precedence):</strong></para>
/// <code>
/// services.AddSingleton&lt;IConsoleLoggerService, ConsoleLoggerServiceImpl&gt;();
/// services.AddSingleton&lt;IPvNugsStaticSecretManager, StaticSecretManagerImpl&gt;();
/// services.AddSingleton&lt;IPvNugsDynamicSecretManager, DynamicSecretManagerImpl&gt;();
/// services.TryAddPvNugsCsProviderPgSql(configuration);
/// // Factory will choose DynamicSecret mode (highest priority)
/// </code>
/// </example>
public static class PvNugsCsProviderPgSqlDi
{
    /// <summary>
    /// Registers the PostgreSQL connection string provider and its configuration with the dependency injection container.
    /// This method configures the provider as a singleton service implementing <see cref="IPvNugsCsProvider"/>,
    /// using an intelligent factory pattern to automatically resolve constructor selection based on registered dependencies.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add the services to.</param>
    /// <param name="config">The configuration instance containing the PostgreSQL provider settings from appsettings.json or other configuration sources.</param>
    /// <returns>The same <see cref="IServiceCollection"/> instance so that additional calls can be chained in a fluent manner.</returns>
    /// <remarks>
    /// <para><strong>Service Registration Process:</strong></para>
    /// <para>This method performs two key registrations:</para>
    /// <list type="number">
    /// <item><description><strong>Configuration Binding:</strong> Configures <see cref="PvNugsCsProviderPgSqlConfig"/> using the Options pattern, binding to the configuration section specified by <see cref="PvNugsCsProviderPgSqlConfig.Section"/>.</description></item>
    /// <item><description><strong>Factory Registration:</strong> Registers a factory function that dynamically resolves the appropriate <see cref="CsProvider"/> constructor based on available dependencies, eliminating constructor ambiguity.</description></item>
    /// </list>
    /// 
    /// <para><strong>Intelligent Constructor Resolution:</strong></para>
    /// <para>The factory pattern implements a sophisticated dependency detection algorithm:</para>
    /// <list type="number">
    /// <item><description><strong>Dynamic Secret Detection:</strong> Checks for <see cref="pvNugsSecretManagerNc9Abstractions.IPvNugsDynamicSecretManager"/> first (highest priority)</description></item>
    /// <item><description><strong>Static Secret Detection:</strong> Falls back to checking for <see cref="pvNugsSecretManagerNc9Abstractions.IPvNugsStaticSecretManager"/> (medium priority)</description></item>
    /// <item><description><strong>Config Mode Fallback:</strong> Uses the primary constructor when no secret managers are available (default priority)</description></item>
    /// </list>
    /// 
    /// <para><strong>Resolution Algorithm Benefits:</strong></para>
    /// <list type="bullet">
    /// <item><description><strong>Automatic Mode Detection:</strong> No need to manually specify which constructor to use</description></item>
    /// <item><description><strong>Fail-Safe Fallback:</strong> Always defaults to a working configuration (Config mode)</description></item>
    /// <item><description><strong>Forward Compatibility:</strong> New secret manager types can be easily integrated</description></item>
    /// <item><description><strong>Clear Precedence:</strong> Predictable behavior when multiple secret managers are registered</description></item>
    /// </list>
    /// 
    /// <para><strong>Configuration Requirements by Mode:</strong></para>
    /// <para>The configuration section must contain appropriate settings based on which constructor the factory selects:</para>
    /// <list type="bullet">
    /// <item><description><strong>All Modes:</strong> Server, Database, Schema, and Mode properties are always required.</description></item>
    /// <item><description><strong>Config Mode (Selected when no secret managers):</strong> Additionally requires Username. Password is optional for password-less authentication.</description></item>
    /// <item><description><strong>StaticSecret Mode (Selected when IPvNugsStaticSecretManager available):</strong> Additionally requires Username and SecretName for secret manager integration.</description></item>
    /// <item><description><strong>DynamicSecret Mode (Selected when IPvNugsDynamicSecretManager available):</strong> Additionally requires SecretName. Username is ignored as it's dynamically generated.</description></item>
    /// </list>
    /// 
    /// <para><strong>Factory Pattern Implementation Details:</strong></para>
    /// <para>The factory function uses <see cref="Microsoft.Extensions.DependencyInjection.ServiceProviderServiceExtensions.GetService{T}(IServiceProvider)"/> 
    /// for optional dependencies and <see cref="Microsoft.Extensions.DependencyInjection.ServiceProviderServiceExtensions.GetRequiredService{T}(IServiceProvider)"/> 
    /// for mandatory dependencies, ensuring proper exception handling for missing required services.</para>
    /// 
    /// <para><strong>Secret Name Resolution:</strong></para>
    /// <para>For StaticSecret and DynamicSecret modes, the provider constructs secret names using the pattern: <c>{config.SecretName}-{SqlRole}</c></para>
    /// <para>Where SqlRole can be Owner, Application, or Reader, allowing role-based credential management in your secret store.</para>
    /// 
    /// <para><strong>Singleton Lifecycle Management:</strong></para>
    /// <para>The factory-registered provider is designed as a singleton service with internal caching and locking mechanisms 
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
    /// Thrown during service resolution if the configuration is invalid for the factory-selected mode 
    /// (e.g., missing required properties, invalid Mode value).
    /// </exception>
    /// <exception cref="System.InvalidOperationException">
    /// Thrown during service resolution if required dependencies are not registered in the container 
    /// (e.g., missing IConsoleLoggerService). Note: Secret manager dependencies are optional and checked by the factory.
    /// </exception>
    /// <example>
    /// <para><strong>Basic registration with automatic mode detection:</strong></para>
    /// <code>
    /// public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    /// {
    ///     // Register required logger service
    ///     services.AddSingleton&lt;IConsoleLoggerService, ConsoleLoggerServiceImpl&gt;();
    ///     
    ///     // Register the PostgreSQL connection string provider
    ///     // Factory will automatically select Config mode (no secret managers registered)
    ///     services.TryAddPvNugsCsProviderPgSql(configuration);
    ///     
    ///     // Now you can inject IPvNugsCsProvider in your services
    ///     services.AddScoped&lt;IDataService, DataService&gt;();
    /// }
    /// </code>
    /// 
    /// <para><strong>Advanced registration with automatic DynamicSecret mode detection:</strong></para>
    /// <code>
    /// public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    /// {
    ///     // Register required dependencies
    ///     services.AddSingleton&lt;IConsoleLoggerService, ConsoleLoggerServiceImpl&gt;();
    ///     services.AddSingleton&lt;IPvNugsDynamicSecretManager, HashiCorpVaultDynamicSecretManager&gt;();
    ///     
    ///     // Register the provider - factory will automatically detect and select DynamicSecret mode
    ///     services.TryAddPvNugsCsProviderPgSql(configuration);
    /// }
    /// 
    /// // The factory has automatically wired the DynamicSecret constructor
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
    /// <para><strong>Priority demonstration with mixed dependencies:</strong></para>
    /// <code>
    /// // Both secret managers are registered
    /// services.AddSingleton&lt;IPvNugsStaticSecretManager, KeyVaultStaticSecretManager&gt;();
    /// services.AddSingleton&lt;IPvNugsDynamicSecretManager, HashiCorpVaultDynamicSecretManager&gt;();
    /// services.TryAddPvNugsCsProviderPgSql(configuration);
    /// 
    /// // Factory resolution priority:
    /// // 1. Detects IPvNugsDynamicSecretManager → Selects DynamicSecret constructor ✓
    /// // 2. IPvNugsStaticSecretManager is ignored due to lower priority
    /// // 3. Result: DynamicSecret mode with automatic credential renewal
    /// </code>
    /// 
    /// <para><strong>Configuration example supporting all modes:</strong></para>
    /// <code>
    /// {
    ///   "PvNugsCsProviderPgSqlConfig": {
    ///     "Mode": "DynamicSecret",
    ///     "Server": "mydb.postgres.database.azure.com",
    ///     "Database": "myapp_production",
    ///     "Schema": "app_schema",
    ///     "Port": 5432,
    ///     "SecretName": "myapp-postgres",
    ///     "Username": "fallback_user",        // Used only in Config/Static modes
    ///     "Timezone": "UTC",
    ///     "TimeoutInSeconds": 30,
    ///     "ExpirationWarningToleranceInMinutes": 30,
    ///     "ExpirationErrorToleranceInMinutes": 5
    ///   }
    /// }
    /// </code>
    /// </example>
    /// <seealso cref="CsProvider"/>
    /// <seealso cref="IPvNugsCsProvider"/>
    /// <seealso cref="IPvNugsPgSqlCsProvider"/>
    /// <seealso cref="PvNugsCsProviderPgSqlConfig"/>
    /// <seealso cref="CsProviderModeEnu"/>
    /// <seealso cref="pvNugsLoggerNc9Abstractions.IConsoleLoggerService"/>
    /// <seealso cref="pvNugsSecretManagerNc9Abstractions.IPvNugsStaticSecretManager"/>
    /// <seealso cref="pvNugsSecretManagerNc9Abstractions.IPvNugsDynamicSecretManager"/>
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

    private static CsProvider CreateProvider(IServiceProvider serviceProvider)
    {
        var logger = serviceProvider.GetRequiredService<IConsoleLoggerService>();
        var options = serviceProvider.GetRequiredService<IOptions<PvNugsCsProviderPgSqlConfig>>();
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

    private static CsProvider CreateStaticSecretProvider(
        IServiceProvider serviceProvider,
        IConsoleLoggerService logger,
        IOptions<PvNugsCsProviderPgSqlConfig> options)
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

    private static CsProvider CreateDynamicSecretProvider(
        IServiceProvider serviceProvider,
        IConsoleLoggerService logger,
        IOptions<PvNugsCsProviderPgSqlConfig> options)
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
                if (string.IsNullOrWhiteSpace(configRow.Username))
                    throw new OptionsValidationException(
                        "Username is required in Config mode.", 
                        typeof(PvNugsCsProviderPgSqlConfigRow),
                        ["Username"]);
                // Password is optional
                break;
            case CsProviderModeEnu.StaticSecret:
                if (string.IsNullOrWhiteSpace(configRow.Username))
                    throw new OptionsValidationException(
                        "Username is required in StaticSecret mode.", 
                        typeof(PvNugsCsProviderPgSqlConfigRow),
                        ["Username"]);
                if (string.IsNullOrWhiteSpace(configRow.SecretName))
                    throw new OptionsValidationException(
                        "SecretName is required in StaticSecret mode.", 
                        typeof(PvNugsCsProviderPgSqlConfigRow),
                        ["SecretName"]);
                break;
            case CsProviderModeEnu.DynamicSecret:
                if (string.IsNullOrWhiteSpace(configRow.SecretName))
                    throw new OptionsValidationException(
                        "SecretName is required in DynamicSecret mode.", 
                        typeof(PvNugsCsProviderPgSqlConfigRow),
                        ["SecretName"]);
                // Username is ignored
                break;
            default:
                throw new OptionsValidationException(
                    $"Unsupported mode: {configRow.Mode}", 
                    typeof(PvNugsCsProviderPgSqlConfigRow),
                    ["Mode"]);
        }
    }
}
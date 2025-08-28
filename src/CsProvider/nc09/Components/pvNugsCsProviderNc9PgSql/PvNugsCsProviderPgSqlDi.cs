using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using pvNugsCsProviderNc9Abstractions;

namespace pvNugsCsProviderNc9PgSql;

/// <summary>
/// Provides dependency injection configuration for the PostgreSQL connection string provider.
/// This static class extends <see cref="IServiceCollection"/> to register the <see cref="CsProvider"/> 
/// and its required configuration for PostgreSQL database connections with multiple authentication modes.
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
/// <para>The provider requires <see cref="PvNugsCsProviderPgSqlConfig"/> to be configured through the application's configuration system.
/// The configuration section name is defined by <see cref="PvNugsCsProviderPgSqlConfig.Section"/>.</para>
/// </remarks>
/// <example>
/// <para>Register for Config mode (configuration-based authentication):</para>
/// <code>
/// services.AddSingleton&lt;IConsoleLoggerService, ConsoleLoggerServiceImpl&gt;();
/// services.TryAddPvNugsCsProviderPgSql(configuration);
/// </code>
/// 
/// <para>Register for StaticSecret mode (secret manager with static secrets):</para>
/// <code>
/// services.AddSingleton&lt;IConsoleLoggerService, ConsoleLoggerServiceImpl&gt;();
/// services.AddSingleton&lt;IPvNugsStaticSecretManager, StaticSecretManagerImpl&gt;();
/// services.TryAddPvNugsCsProviderPgSql(configuration);
/// </code>
/// 
/// <para>Register for DynamicSecret mode (secret manager with dynamic credentials):</para>
/// <code>
/// services.AddSingleton&lt;IConsoleLoggerService, ConsoleLoggerServiceImpl&gt;();
/// services.AddSingleton&lt;IPvNugsDynamicSecretManager, DynamicSecretManagerImpl&gt;();
/// services.TryAddPvNugsCsProviderPgSql(configuration);
/// </code>
/// </example>
public static class PvNugsCsProviderPgSqlDi
{
    /// <summary>
    /// Registers the PostgreSQL connection string provider and its configuration with the dependency injection container.
    /// This method configures the provider as a singleton service implementing <see cref="IPvNugsCsProvider"/>,
    /// enabling role-based PostgreSQL connections with multiple authentication modes throughout your application.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add the services to.</param>
    /// <param name="config">The configuration instance containing the PostgreSQL provider settings from appsettings.json or other configuration sources.</param>
    /// <returns>The same <see cref="IServiceCollection"/> instance so that additional calls can be chained in a fluent manner.</returns>
    /// <remarks>
    /// <para><c>Service Registration:</c></para>
    /// <para>This method performs two key registrations:</para>
    /// <list type="number">
    /// <item><description>Configures <see cref="PvNugsCsProviderPgSqlConfig"/> using the Options pattern, binding to the configuration section specified by <see cref="PvNugsCsProviderPgSqlConfig.Section"/>.</description></item>
    /// <item><description>Registers <see cref="CsProvider"/> as a singleton implementation of <see cref="IPvNugsCsProvider"/> using <see cref="Microsoft.Extensions.DependencyInjection.Extensions.ServiceCollectionDescriptorExtensions.TryAddSingleton{TService, TImplementation}(IServiceCollection)"/>.</description></item>    /// </list>
    /// 
    /// <para><c>Dependency Resolution and Mode Detection:</c></para>
    /// <para>The provider automatically selects the appropriate constructor based on which dependencies are registered:</para>
    /// <list type="bullet">
    /// <item><description><c>Config Mode:</c> Only requires <see cref="pvNugsLoggerNc9Abstractions.IConsoleLoggerService"/>. Uses primary constructor with logger and configuration options.</description></item>
    /// <item><description><c>StaticSecret Mode:</c> Requires <see cref="pvNugsLoggerNc9Abstractions.IConsoleLoggerService"/> and <see cref="pvNugsSecretManagerNc9Abstractions.IPvNugsStaticSecretManager"/>. Uses constructor overload with static secret manager.</description></item>
    /// <item><description><c>DynamicSecret Mode:</c> Requires <see cref="pvNugsLoggerNc9Abstractions.IConsoleLoggerService"/> and <see cref="pvNugsSecretManagerNc9Abstractions.IPvNugsDynamicSecretManager"/>. Uses constructor overload with dynamic secret manager.</description></item>
    /// </list>
    /// 
    /// <para><c>Configuration Requirements:</c></para>
    /// <para>The configuration section must contain appropriate settings based on the intended operational mode:</para>
    /// <list type="bullet">
    /// <item><description><c>All Modes:</c> Server, Database, Schema, and Mode properties are always required.</description></item>
    /// <item><description><c>Config Mode:</c> Additionally requires Username. Password is optional for password-less authentication.</description></item>
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
    ///     // Register the PostgreSQL connection string provider
    ///     services.TryAddPvNugsCsProviderPgSql(configuration);
    ///     
    ///     // Now you can inject IPvNugsCsProvider in your services
    ///     services.AddScoped&lt;IDataService, DataService&gt;();
    /// }
    /// </code>
    /// 
    /// <para><c>Advanced registration for DynamicSecret mode with Azure Key Vault:</c></para>
    /// <code>
    /// public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    /// {
    ///     // Register required dependencies
    ///     services.AddSingleton&lt;IConsoleLoggerService, ConsoleLoggerServiceImpl&gt;();
    ///     services.AddSingleton&lt;IPvNugsDynamicSecretManager, HashicorpVaultDynamicSecretManager&gt;();
    ///     
    ///     // Register the provider - it will automatically detect DynamicSecret mode
    ///     services.TryAddPvNugsCsProviderPgSql(configuration);
    /// }
    /// 
    /// // Usage in your service
    /// public class DataService
    /// {
    ///     public DataService(IPvNugsPgSqlCsProvider csProvider) { ... }
    ///     
    ///     public async Task&lt;List&lt;User&gt;&gt; GetUsersAsync()
    ///     {
    ///         var connectionString = await csProvider.GetConnectionStringAsync(SqlRoleEnu.Reader);
    ///         // Use connection string with Npgsql...
    ///     }
    /// }
    /// </code>
    /// 
    /// <para><c>Configuration example (appsettings.json):</c></para>
    /// <code>
    /// {
    ///   "PvNugsCsProviderPgSqlConfig": {
    ///     "Mode": "DynamicSecret",
    ///     "Server": "mydb.postgres.database.azure.com",
    ///     "Database": "myapp_production",
    ///     "Schema": "app_schema",
    ///     "Port": 5432,
    ///     "SecretName": "myapp-postgres",
    ///     "Timezone": "UTC",
    ///     "TimeoutInSeconds": 30
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
        services.Configure<PvNugsCsProviderPgSqlConfig>(
            config.GetSection(PvNugsCsProviderPgSqlConfig.Section));
        
        services.TryAddSingleton<IPvNugsCsProvider, CsProvider>();
        
        return services;
    }
}
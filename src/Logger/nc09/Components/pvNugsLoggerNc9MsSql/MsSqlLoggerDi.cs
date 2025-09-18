using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using pvNugsCsProviderNc9Abstractions;
using pvNugsLoggerNc9Abstractions;

namespace pvNugsLoggerNc9MsSql;

/// <summary>
/// Provides dependency injection extension methods for registering Microsoft SQL Server logging services.
/// </summary>
/// <remarks>
/// <para>
/// This static class contains extension methods for <see cref="IServiceCollection"/> that simplify the
/// registration of SQL Server-based logging components in the dependency injection container. It handles
/// the complex setup of multiple interface registrations, configuration binding, and service lifetime management.
/// </para>
/// <para>
/// The extension methods use the "TryAdd" pattern, which means multiple calls to the same registration method
/// will not cause conflicts - only the first registration will be effective. This allows for safe composition
/// of multiple logging configurations.
/// </para>
/// <para>
/// <strong>Prerequisites:</strong> Before using these extensions, ensure that the following services are
/// registered in your DI container:
/// </para>
/// <list type="bullet">
/// <item><see cref="IPvNugsMsSqlCsProvider"/> - Required for database connectivity</item>
/// <item><see cref="IConsoleLoggerService"/> - Optional, for internal logging operations</item>
/// <item>Configuration sections for <see cref="PvNugsMsSqlLogWriterConfig"/> and <see cref="PvNugsLoggerConfig"/></item>
/// </list>
/// </remarks>
/// <example>
/// <code>
/// // Complete setup in Program.cs or Startup.cs
/// var builder = WebApplication.CreateBuilder(args);
/// 
/// // Register required dependencies first
/// builder.Services.AddSingleton&lt;IPvNugsMsSqlCsProvider, YourCsProvider&gt;();
/// builder.Services.AddSingleton&lt;IConsoleLoggerService, ConsoleLoggerService&gt;(); // Optional
/// 
/// // Register SQL Server logging services
/// builder.Services.TryAddPvNugsMsSqlLogger(builder.Configuration);
/// 
/// var app = builder.Build();
/// 
/// // Usage in a controller or service
/// public class HomeController : Controller
/// {
///     private readonly IMsSqlLoggerService _logger;
///     
///     public HomeController(IMsSqlLoggerService logger)
///     {
///         _logger = logger;
///     }
///     
///     public async Task&lt;IActionResult&gt; Index()
///     {
///         await _logger.LogAsync("Home page accessed", SeverityEnu.Info);
///         return View();
///     }
/// }
/// </code>
/// </example>
public static class MsSqlLoggerDi
{
    /// <summary>
    /// Registers Microsoft SQL Server logging services with the dependency injection container.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
    /// <param name="config">
    /// The configuration instance containing the required configuration sections:
    /// <list type="bullet">
    /// <item><see cref="PvNugsMsSqlLogWriterConfig.Section"/> - SQL Server log writer configuration</item>
    /// <item><see cref="PvNugsLoggerConfig.Section"/> - General logger configuration (minimum log level, etc.)</item>
    /// </list>
    /// </param>
    /// <returns>
    /// The same <see cref="IServiceCollection"/> instance to support method chaining.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="services"/> or <paramref name="config"/> is null.
    /// </exception>
    /// <remarks>
    /// <para>
    /// This method registers the following services as singletons:
    /// </para>
    /// <list type="bullet">
    /// <item><see cref="ILogWriter"/> → <see cref="MsSqlLogWriter"/></item>
    /// <item><see cref="ISqlLogWriter"/> → <see cref="MsSqlLogWriter"/></item>
    /// <item><see cref="IMsSqlLogWriter"/> → <see cref="MsSqlLogWriter"/></item>
    /// <item><see cref="ILoggerService"/> → <see cref="MsSqlLoggerService"/></item>
    /// <item><see cref="ISqlLoggerService"/> → <see cref="MsSqlLoggerService"/></item>
    /// <item><see cref="IMsSqlLoggerService"/> → <see cref="MsSqlLoggerService"/></item>
    /// </list>
    /// <para>
    /// All registrations use the <c>TryAddSingleton</c> method, which means:
    /// </para>
    /// <list type="bullet">
    /// <item>Services are registered with singleton lifetime for optimal performance</item>
    /// <item>Multiple calls to this method won't cause duplicate registrations</item>
    /// <item>The same <see cref="MsSqlLogWriter"/> instance is shared across all interface types</item>
    /// <item>The same <see cref="MsSqlLoggerService"/> instance is shared across all interface types</item>
    /// </list>
    /// <para>
    /// <strong>Configuration Requirements:</strong> The method expects the following configuration sections
    /// to be present in the provided <paramref name="config"/>:
    /// </para>
    /// <code>
    /// {
    ///   "PvNugsMsSqlLogWriterConfig": {
    ///     "TableName": "ApplicationLogs",
    ///     "SchemaName": "dbo",
    ///     "CreateTableAtFirstUse": true,
    ///     // ... other writer configuration
    ///   },
    ///   "PvNugsLoggerConfig": {
    ///     "MinLevel": "Info"
    ///     // ... other logger configuration
    ///   }
    /// }
    /// </code>
    /// <para>
    /// <strong>Dependency Requirements:</strong> This method assumes that the following services
    /// are already registered or will be registered elsewhere:
    /// </para>
    /// <list type="bullet">
    /// <item><see cref="IPvNugsMsSqlCsProvider"/> (Required) - Provides database connection strings</item>
    /// <item><see cref="IConsoleLoggerService"/> (Optional) - Used for internal logging by the log writer</item>
    /// </list>
    /// <para>
    /// <strong>Thread Safety:</strong> All registered services are designed to be thread-safe and can
    /// be safely used concurrently across multiple threads.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Basic registration with minimal configuration
    /// services.TryAddPvNugsMsSqlLogger(configuration);
    /// 
    /// // Registration with additional setup
    /// services.AddSingleton&lt;IPvNugsMsSqlCsProvider&gt;(provider =&gt; 
    ///     new YourCustomCsProvider(connectionString));
    /// 
    /// services.TryAddPvNugsMsSqlLogger(configuration);
    /// 
    /// // You can now inject any of these interfaces:
    /// // - IMsSqlLoggerService (most specific)
    /// // - ISqlLoggerService (SQL-specific)
    /// // - ILoggerService (generic logging)
    /// // - IMsSqlLogWriter (direct writer access)
    /// // - ISqlLogWriter (SQL writer interface)
    /// // - ILogWriter (generic writer interface)
    /// </code>
    /// </example>
    /// <seealso cref="PvNugsMsSqlLogWriterConfig"/>
    /// <seealso cref="PvNugsLoggerConfig"/>
    /// <seealso cref="MsSqlLogWriter"/>
    /// <seealso cref="MsSqlLoggerService"/>
    public static IServiceCollection TryAddPvNugsMsSqlLogger(
        this IServiceCollection services, IConfiguration config)
    {
        services.Configure<PvNugsMsSqlLogWriterConfig>(
            config.GetSection(PvNugsMsSqlLogWriterConfig.Section));
        
        services.TryAddSingleton<ILogWriter, MsSqlLogWriter>();
        services.TryAddSingleton<ISqlLogWriter, MsSqlLogWriter>();
        services.TryAddSingleton<IMsSqlLogWriter, MsSqlLogWriter>();
        
        services.Configure<PvNugsLoggerConfig>(
            config.GetSection(PvNugsLoggerConfig.Section));
        
        services.TryAddSingleton<ILoggerService, MsSqlLoggerService>();
        services.TryAddSingleton<ISqlLoggerService, MsSqlLoggerService>();
        services.TryAddSingleton<IMsSqlLoggerService, MsSqlLoggerService>();
        
        return services;
    }
}
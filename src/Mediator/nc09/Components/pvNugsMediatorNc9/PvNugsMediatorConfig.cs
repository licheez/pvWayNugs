using pvNugsMediatorNc9Abstractions;

namespace pvNugsMediatorNc9;

/// <summary>
/// Configuration options for the PvNugs Mediator service.
/// </summary>
/// <remarks>
/// <para>
/// This class defines how the mediator discovers and registers handlers at runtime.
/// Configure these options during service registration to control the mediator's behavior.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Configure via action delegate
/// services.TryAddPvNugsMediator(config =>
/// {
///     config.DiscoveryMode = DiscoveryMode.FullScan;
///     config.ServiceLifetime = ServiceLifetime.Scoped;
/// });
/// 
/// // Or via IConfiguration
/// services.Configure&lt;PvNugsMediatorConfig&gt;(
///     configuration.GetSection(PvNugsMediatorConfig.Section));
/// </code>
/// </example>
public class PvNugsMediatorConfig
{
    /// <summary>
    /// Gets the configuration section name used when binding from <c>IConfiguration</c>.
    /// </summary>
    /// <value>The string "PvNugsMediatorConfig".</value>
    /// <remarks>
    /// Use this constant when configuring the mediator from appsettings.json or other configuration sources.
    /// </remarks>
    public const string Section = nameof(PvNugsMediatorConfig);

    /// <summary>
    /// Gets or sets the handler discovery mode.
    /// </summary>
    /// <value>
    /// The <see cref="pvNugsMediatorNc9Abstractions.DiscoveryMode"/> to use. 
    /// Defaults to <see cref="pvNugsMediatorNc9Abstractions.DiscoveryMode.Manual"/>.
    /// </value>
    /// <remarks>
    /// <para>
    /// Determines how the mediator locates and registers handler implementations:
    /// </para>
    /// <list type="bullet">
    /// <item><description><b>Manual</b>: Handlers must be explicitly registered (recommended for production)</description></item>
    /// <item><description><b>Decorated</b>: Discovers handlers with <c>[MediatorHandler]</c> attribute</description></item>
    /// <item><description><b>FullScan</b>: Auto-discovers all handler implementations via reflection (best for development)</description></item>
    /// </list>
    /// </remarks>
    public DiscoveryMode DiscoveryMode { get; set; } = DiscoveryMode.Manual;
    
    /// <summary>
    /// Gets or sets the default service lifetime for auto-discovered handlers.
    /// </summary>
    /// <value>
    /// The <see cref="pvNugsMediatorNc9Abstractions.ServiceLifetime"/> to use for handlers. 
    /// Defaults to <see cref="pvNugsMediatorNc9Abstractions.ServiceLifetime.Transient"/>.
    /// </value>
    /// <remarks>
    /// <para>
    /// This lifetime is only used when <see cref="DiscoveryMode"/> is set to <c>FullScan</c>.
    /// For <c>Decorated</c> mode, each handler can specify its own lifetime via the <c>[MediatorHandler]</c> attribute.
    /// For <c>Manual</c> mode, you control the lifetime when registering handlers.
    /// </para>
    /// </remarks>
    public ServiceLifetime ServiceLifetime { get; set; } = ServiceLifetime.Transient;
}
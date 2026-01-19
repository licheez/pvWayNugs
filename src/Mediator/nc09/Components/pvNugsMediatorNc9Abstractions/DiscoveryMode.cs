using pvNugsMediatorNc9Abstractions.pvNugs;

namespace pvNugsMediatorNc9Abstractions;

/// <summary>
/// Specifies how the mediator discovers and resolves handlers from the dependency injection container.
/// </summary>
/// <remarks>
/// <para>
/// The discovery mode determines the strategy used by the mediator implementation to locate
/// request handlers, notification handlers, and pipeline behaviors at runtime. Each mode offers
/// different trade-offs between performance, flexibility, and ease of use.
/// </para>
/// <para>
/// Choose the discovery mode based on your application's requirements:
/// </para>
/// <list type="bullet">
/// <item>
/// <description><b>Manual</b>: Best for production environments where performance is critical and all handlers are explicitly registered.</description>
/// </item>
/// <item>
/// <description><b>Decorated</b>: Balanced approach using attributes for automatic discovery with minimal overhead.</description>
/// </item>
/// <item>
/// <description><b>FullScan</b>: Most flexible for development, automatically finds all handlers via reflection.</description>
/// </item>
/// </list>
/// </remarks>
/// <example>
/// <code>
/// // Configure discovery mode during mediator registration
/// services.AddPvNugsMediator(options =>
/// {
///     // Production: Use manual registration for best performance
///     options.DiscoveryMode = DiscoveryMode.Manual;
///     
///     // Development: Use FullScan for convenience
///     // options.DiscoveryMode = DiscoveryMode.FullScan;
/// });
/// </code>
/// </example>
public enum DiscoveryMode
{
    /// <summary>
    /// Handlers are resolved exclusively through explicit dependency injection registrations.
    /// </summary>
    /// <remarks>
    /// <para>
    /// In Manual mode, the mediator only uses the <see cref="System.IServiceProvider"/> to resolve
    /// handlers that have been explicitly registered in the DI container. No automatic discovery
    /// or reflection is performed.
    /// </para>
    /// <para>
    /// <b>Advantages:</b>
    /// </para>
    /// <list type="bullet">
    /// <item><description>⚡ <b>Best Performance</b>: No reflection overhead, fastest handler resolution</description></item>
    /// <item><description>🎯 <b>Explicit Control</b>: You decide exactly which handlers are available</description></item>
    /// <item><description>🔒 <b>Compile-Time Safety</b>: Missing registrations can be caught early</description></item>
    /// <item><description>📦 <b>Smaller Footprint</b>: No additional runtime scanning or metadata</description></item>
    /// </list>
    /// <para>
    /// <b>Disadvantages:</b>
    /// </para>
    /// <list type="bullet">
    /// <item><description>🔧 <b>More Setup</b>: Every handler must be manually registered in DI</description></item>
    /// <item><description>📝 <b>Maintenance</b>: New handlers require explicit registration</description></item>
    /// </list>
    /// <para>
    /// <b>Recommended For:</b> Production environments, performance-critical applications,
    /// scenarios where explicit control is preferred.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Configure Manual mode
    /// services.AddPvNugsMediator(options => 
    /// {
    ///     options.DiscoveryMode = DiscoveryMode.Manual;
    /// });
    /// 
    /// // Explicitly register each handler
    /// services.AddTransient&lt;IPvNugsMediatorRequestHandler&lt;GetUserRequest, User&gt;, GetUserHandler&gt;();
    /// services.AddTransient&lt;IPvNugsNotificationHandler&lt;UserCreated&gt;, SendEmailHandler&gt;();
    /// services.AddTransient&lt;IPvNugsNotificationHandler&lt;UserCreated&gt;, LogEventHandler&gt;();
    /// </code>
    /// </example>
    Manual = 0,

    /// <summary>
    /// Handlers are discovered using decorator attributes applied to handler classes.
    /// </summary>
    /// <remarks>
    /// <para>
    /// In Decorated mode, the mediator scans for handler classes marked with specific attributes
    /// (e.g., <c>[MediatorHandler]</c> or similar). Only classes with the decorator are registered
    /// and made available to the mediator.
    /// </para>
    /// <para>
    /// <b>Advantages:</b>
    /// </para>
    /// <list type="bullet">
    /// <item><description>🎯 <b>Selective Discovery</b>: Only decorated classes are found</description></item>
    /// <item><description>⚡ <b>Good Performance</b>: Faster than FullScan, scans only decorated types</description></item>
    /// <item><description>✨ <b>Convention-Based</b>: Simple attribute marks a class as a handler</description></item>
    /// <item><description>🔍 <b>Self-Documenting</b>: Attributes clearly indicate handler purpose</description></item>
    /// </list>
    /// <para>
    /// <b>Disadvantages:</b>
    /// </para>
    /// <list type="bullet">
    /// <item><description>📝 <b>Requires Attributes</b>: Must decorate every handler class</description></item>
    /// <item><description>🔍 <b>Some Reflection</b>: Scans assemblies for decorated types</description></item>
    /// <item><description>🎓 <b>Learning Curve</b>: Team needs to understand decorator convention</description></item>
    /// </list>
    /// <para>
    /// <b>Recommended For:</b> Medium to large applications, teams that prefer convention over
    /// configuration, scenarios where selective handler registration is desired.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Configure Decorated mode
    /// services.AddPvNugsMediator(options => 
    /// {
    ///     options.DiscoveryMode = DiscoveryMode.Decorated;
    /// });
    /// 
    /// // Handler classes with decorator attribute are automatically discovered
    /// [MediatorHandler]
    /// public class GetUserHandler : IPvNugsMediatorRequestHandler&lt;GetUserRequest, User&gt;
    /// {
    ///     public async Task&lt;User&gt; HandleAsync(GetUserRequest request, CancellationToken ct)
    ///     {
    ///         // Implementation
    ///     }
    /// }
    /// 
    /// // Handler without decorator is NOT discovered
    /// public class InternalHelper : IPvNugsMediatorRequestHandler&lt;HelperRequest, Result&gt;
    /// {
    ///     // This won't be registered (no attribute)
    /// }
    /// </code>
    /// </example>
    Decorated = 1,

    /// <summary>
    /// All handlers are automatically discovered via reflection scanning of loaded assemblies.
    /// </summary>
    /// <remarks>
    /// <para>
    /// In FullScan mode, the mediator uses reflection to scan all loaded assemblies and automatically
    /// registers any class that implements handler interfaces (<see cref="pvNugs.IPvNugsMediatorRequestHandler{TRequest,TResponse}"/>,
    /// <see cref="IPvNugsMediatorNotificationHandler{TNotification}"/>, etc.). No explicit registration
    /// or decoration is required.
    /// </para>
    /// <para>
    /// <b>Advantages:</b>
    /// </para>
    /// <list type="bullet">
    /// <item><description>🚀 <b>Zero Configuration</b>: Just implement the interface, handler is auto-discovered</description></item>
    /// <item><description>✨ <b>Maximum Convenience</b>: Perfect for rapid development and prototyping</description></item>
    /// <item><description>🔄 <b>Dynamic</b>: New handlers are automatically available without registration</description></item>
    /// <item><description>🎯 <b>Convention-Based</b>: Interface implementation is the only requirement</description></item>
    /// </list>
    /// <para>
    /// <b>Disadvantages:</b>
    /// </para>
    /// <list type="bullet">
    /// <item><description>⏱️ <b>Startup Overhead</b>: Reflection scanning takes time during application startup</description></item>
    /// <item><description>🔍 <b>All Assemblies Scanned</b>: May discover unintended handlers</description></item>
    /// <item><description>💾 <b>Memory Usage</b>: Keeps metadata about all discovered types</description></item>
    /// <item><description>🎭 <b>Less Explicit</b>: Harder to see what handlers are available</description></item>
    /// </list>
    /// <para>
    /// <b>Recommended For:</b> Development environments, small to medium applications, rapid prototyping,
    /// scenarios where convenience outweighs performance concerns, applications with infrequent restarts.
    /// </para>
    /// <para>
    /// <b>Performance Note:</b> The scanning occurs during application startup. Consider using
    /// <see cref="Manual"/> or <see cref="Decorated"/> mode in production if startup time is critical.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Configure FullScan mode
    /// services.AddPvNugsMediator(options => 
    /// {
    ///     options.DiscoveryMode = DiscoveryMode.FullScan;
    ///     
    ///     // Optionally limit which assemblies to scan
    ///     options.AssembliesToScan = new[] 
    ///     { 
    ///         typeof(GetUserHandler).Assembly,  // Your handlers assembly
    ///         typeof(OrderHandlers).Assembly    // Another assembly with handlers
    ///     };
    /// });
    /// 
    /// // Handlers are automatically discovered - no registration needed!
    /// public class GetUserHandler : IPvNugsMediatorRequestHandler&lt;GetUserRequest, User&gt;
    /// {
    ///     public async Task&lt;User&gt; HandleAsync(GetUserRequest request, CancellationToken ct)
    ///     {
    ///         return await _repository.GetUserAsync(request.UserId, ct);
    ///     }
    /// }
    /// 
    /// public class SendEmailHandler : IPvNugsMediatorNotificationHandler&lt;UserCreatedNotification&gt;
    /// {
    ///     public async Task Handle(UserCreatedNotification notification, CancellationToken ct)
    ///     {
    ///         await _emailService.SendWelcomeEmailAsync(notification.Email, ct);
    ///     }
    /// }
    /// 
    /// // Both handlers above are automatically registered!
    /// </code>
    /// </example>
    FullScan = 2
}


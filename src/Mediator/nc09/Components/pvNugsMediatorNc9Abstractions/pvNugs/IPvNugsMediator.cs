using pvNugsMediatorNc9Abstractions.Mediator;

namespace pvNugsMediatorNc9Abstractions.pvNugs;

/// <summary>
/// PvNugs-branded mediator interface that extends the base <see cref="IMediator"/> interface
/// with additional diagnostic and introspection capabilities.
/// </summary>
/// <remarks>
/// <para>
/// This interface extends <see cref="IMediator"/> with PvNugs-specific features while maintaining
/// full backward compatibility. It allows PvNugs implementations to be used interchangeably 
/// with any code that depends on the base <see cref="IMediator"/> interface.
/// </para>
/// <para>
/// Beyond the standard mediator functionality, this interface provides:
/// </para>
/// <list type="bullet">
/// <item><description>Handler introspection via <see cref="GetRegisteredHandlers"/></description></item>
/// <item><description>Development and debugging support</description></item>
/// <item><description>DI container validation capabilities</description></item>
/// </list>
/// <para>
/// Use this interface in your PvNugs-based applications for dependency injection,
/// while the implementation can still be consumed by code expecting the base <see cref="IMediator"/>.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Register PvNugs implementation
/// services.AddScoped&lt;IPvNugsMediator, PvNugsMediatorImplementation&gt;();
/// 
/// // Can be injected as IPvNugsMediator
/// public class MyService
/// {
///     private readonly IPvNugsMediator _mediator;
///     
///     public MyService(IPvNugsMediator mediator)
///     {
///         _mediator = mediator;
///     }
///     
///     public void DiagnosticCheck()
///     {
///         // Use PvNugs-specific feature
///         var handlers = _mediator.GetRegisteredHandlers();
///         foreach (var handler in handlers)
///         {
///             Console.WriteLine(handler);
///         }
///     }
/// }
/// 
/// // Or as IMediator for backward compatibility
/// public class LegacyService
/// {
///     private readonly IMediator _mediator;
///     
///     public LegacyService(IMediator mediator) // Same implementation works
///     {
///         _mediator = mediator;
///     }
/// }
/// </code>
/// </example>
public interface IPvNugsMediator: IMediator
{
    /// <summary>
    /// Gets information about all mediator components registered in the dependency injection container.
    /// </summary>
    /// <returns>
    /// A collection of <see cref="MediatorRegistrationInfo"/> objects describing all registered
    /// request handlers, notification handlers, and pipeline behaviors.
    /// </returns>
    /// <remarks>
    /// <para>
    /// This method is primarily intended for development, debugging, and diagnostic scenarios.
    /// It provides introspection capabilities to:
    /// </para>
    /// <list type="bullet">
    /// <item><description>Verify that expected handlers are registered</description></item>
    /// <item><description>Debug DI configuration issues</description></item>
    /// <item><description>Generate documentation of available handlers</description></item>
    /// <item><description>Implement health checks for mediator components</description></item>
    /// <item><description>Validate application startup configuration</description></item>
    /// </list>
    /// <para>
    /// The returned collection includes:
    /// </para>
    /// <list type="bullet">
    /// <item><description><b>Request Handlers</b>: Implementations of <see cref="IPvNugsMediatorRequestHandler{TRequest,TResponse}"/> or <see cref="IRequestHandler{TRequest,TResponse}"/></description></item>
    /// <item><description><b>Notification Handlers</b>: Implementations of <see cref="IPvNugsNotificationHandler{TNotification}"/> or <see cref="INotificationHandler{TNotification}"/></description></item>
    /// <item><description><b>Pipeline Behaviors</b>: Implementations of <see cref="IPvNugsMediatorPipelineRequestHandler{TRequest,TResponse}"/> or <see cref="IPipelineBehavior{TRequest,TResponse}"/></description></item>
    /// </list>
    /// <para>
    /// <b>Performance Note</b>: This method uses reflection to inspect the DI container.
    /// Avoid calling it in hot paths or production request handling. Cache the results
    /// if needed for repeated access.
    /// </para>
    /// <para>
    /// <b>Recommended Usage</b>: Call during application startup, in diagnostic endpoints,
    /// or in development/testing environments only.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // During application startup - validate configuration
    /// var mediator = app.Services.GetRequiredService&lt;IPvNugsMediator&gt;();
    /// var registrations = mediator.GetRegisteredHandlers();
    /// 
    /// Console.WriteLine($"Found {registrations.Count()} registered handlers:");
    /// foreach (var reg in registrations)
    /// {
    ///     Console.WriteLine($"  {reg}");
    /// }
    /// 
    /// // Group by type
    /// var byType = registrations.GroupBy(r => r.RegistrationType);
    /// foreach (var group in byType)
    /// {
    ///     Console.WriteLine($"\n{group.Key}: {group.Count()}");
    ///     foreach (var reg in group)
    ///     {
    ///         Console.WriteLine($"  - {reg.ImplementationType.Name}");
    ///     }
    /// }
    /// 
    /// // Health check endpoint
    /// app.MapGet("/health/mediator", (IPvNugsMediator mediator) =>
    /// {
    ///     var handlers = mediator.GetRegisteredHandlers();
    ///     return new 
    ///     { 
    ///         Status = "Healthy",
    ///         RequestHandlers = handlers.Count(r => r.RegistrationType.Contains("Request")),
    ///         NotificationHandlers = handlers.Count(r => r.RegistrationType.Contains("Notification")),
    ///         Pipelines = handlers.Count(r => r.RegistrationType.Contains("Pipeline"))
    ///     };
    /// });
    /// 
    /// // Verify specific handler is registered
    /// var hasUserHandler = registrations.Any(r => 
    ///     r.MessageType?.Name == "GetUserByIdRequest");
    /// if (!hasUserHandler)
    /// {
    ///     throw new InvalidOperationException("GetUserByIdRequest handler not registered!");
    /// }
    /// </code>
    /// </example>
    IEnumerable<MediatorRegistrationInfo> GetRegisteredHandlers();
}


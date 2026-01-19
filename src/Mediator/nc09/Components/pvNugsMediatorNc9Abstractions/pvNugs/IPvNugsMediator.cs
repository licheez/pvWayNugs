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
    /// Sends a request to its handler and returns the response.
    /// </summary>
    /// <typeparam name="TResponse">
    /// The type of response expected from the request handler.
    /// </typeparam>
    /// <param name="request">
    /// The request instance to be handled. Must implement <see cref="IRequest{TResponse}"/>.
    /// </param>
    /// <param name="cancellationToken">
    /// A token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.
    /// </param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains
    /// the response of type <typeparamref name="TResponse"/> from the handler.
    /// </returns>
    /// <remarks>
    /// <para>
    /// This method follows the request/response pattern where exactly one handler is expected
    /// to process the request. The handler is resolved from the dependency injection container
    /// based on the request type.
    /// </para>
    /// <para>
    /// If pipeline behaviors are registered, they will be executed in order, wrapping the
    /// actual handler execution. This allows for cross-cutting concerns like logging, validation,
    /// caching, or performance monitoring.
    /// </para>
    /// <para>
    /// <b>MediatR Compatibility:</b> This method uses the same naming convention as MediatR's
    /// <c>Send</c> method, making it a drop-in replacement for existing MediatR-based code.
    /// </para>
    /// </remarks>
    /// <exception cref="InvalidOperationException">
    /// Thrown when no handler is registered for the request type.
    /// </exception>
    /// <example>
    /// <code>
    /// // MediatR-compatible syntax
    /// var user = await _mediator.Send(new GetUserByIdRequest { UserId = 123 });
    /// await _mediator.Send(new DeleteUserRequest { UserId = 123 }); // Returns Unit
    /// </code>
    /// </example>
    Task<TResponse> SendAsync<TResponse>(
        IRequest<TResponse> request, 
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Publishes a notification to all registered handlers.
    /// </summary>
    /// <typeparam name="TNotification">
    /// The type of notification being published. Must implement <see cref="INotification"/>.
    /// </typeparam>
    /// <param name="notification">
    /// The notification instance to be published to all handlers.
    /// </param>
    /// <param name="cancellationToken">
    /// A token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.
    /// </param>
    /// <returns>
    /// A task that represents the asynchronous operation.
    /// </returns>
    /// <remarks>
    /// <para>
    /// This method follows the publish/subscribe pattern where zero or more handlers can respond
    /// to the notification. All handlers are resolved from the dependency injection container
    /// and typically execute concurrently.
    /// </para>
    /// <para>
    /// Notifications are fire-and-forget - they don't return values. This is useful for
    /// broadcasting events or triggering side effects across multiple components.
    /// </para>
    /// <para>
    /// <b>MediatR Compatibility:</b> This method uses the same naming convention as MediatR's
    /// <c>Publish</c> method, making it a drop-in replacement for existing MediatR-based code.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // MediatR-compatible syntax
    /// await _mediator.Publish(new UserCreatedNotification { UserId = 123, Email = "user@example.com" });
    /// </code>
    /// </example>
    Task PublishAsync<TNotification>(
        TNotification notification, 
        CancellationToken cancellationToken = default)
        where TNotification : INotification;
    
    /// <summary>
    /// Publishes a notification object to all registered handlers.
    /// </summary>
    /// <param name="notification">
    /// The notification object to be published. The object type is used to resolve handlers.
    /// </param>
    /// <param name="cancellationToken">
    /// A token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.
    /// </param>
    /// <returns>
    /// A task that represents the asynchronous operation.
    /// </returns>
    /// <remarks>
    /// <para>
    /// This is a non-generic overload that accepts any object as a notification.
    /// The runtime type of the notification is used to resolve and invoke the appropriate handlers.
    /// </para>
    /// <para>
    /// This method is useful when the notification type is only known at runtime or when
    /// working with polymorphic notification hierarchies.
    /// </para>
    /// <para>
    /// <b>MediatR Compatibility:</b> This overload matches MediatR's object-based <c>Publish</c> signature.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// object notification = new UserCreatedNotification { UserId = 123, Email = "user@example.com" };
    /// await _mediator.Publish(notification);
    /// </code>
    /// </example>
    Task PublishAsync(
        object notification, 
        CancellationToken cancellationToken = default);
    
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
    /// <item><description><b>Notification Handlers</b>: Implementations of <see cref="IPvNugsMediatorNotificationHandler{TNotification}"/> or <see cref="INotificationHandler{TNotification}"/></description></item>
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


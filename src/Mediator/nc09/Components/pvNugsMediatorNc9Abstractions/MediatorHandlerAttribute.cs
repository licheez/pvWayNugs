using pvNugsMediatorNc9Abstractions.pvNugs;

namespace pvNugsMediatorNc9Abstractions;

/// <summary>
/// Marks a class as a mediator handler for automatic discovery
/// when using <see cref="DiscoveryMode.Decorated"/>.
/// </summary>
/// <remarks>
/// <para>
/// This attribute is used in conjunction with <see cref="DiscoveryMode.Decorated"/> to enable
/// selective automatic handler registration. Only classes decorated with this attribute will
/// be discovered and registered by the mediator during application startup.
/// </para>
/// <para>
/// The attribute can be applied to any class that implements one of the mediator handler interfaces:
/// </para>
/// <list type="bullet">
/// <item><description><see cref="pvNugs.IPvNugsMediatorRequestHandler{TRequest,TResponse}"/></description></item>
/// <item><description><see cref="pvNugs.IPvNugsMediatorRequestHandler{TRequest}"/></description></item>
/// <item><description><see cref="IPvNugsMediatorNotificationHandler{TNotification}"/></description></item>
/// <item><description><see cref="pvNugs.IPvNugsMediatorPipelineRequestHandler{TRequest,TResponse}"/></description></item>
/// <item><description>Or their base interface equivalents</description></item>
/// </list>
/// <para>
/// <b>Lifetime Configuration:</b> Use the <see cref="Lifetime"/> property to specify the
/// DI service lifetime for the handler. Defaults to <see cref="ServiceLifetime.Transient"/>.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Request handler with default Transient lifetime
/// [MediatorHandler]
/// public class GetUserByIdHandler : IPvNugsMediatorRequestHandler&lt;GetUserByIdRequest, User&gt;
/// {
///     public async Task&lt;User&gt; HandleAsync(GetUserByIdRequest request, CancellationToken ct)
///     {
///         // Implementation
///         return await _repository.GetByIdAsync(request.UserId, ct);
///     }
/// }
/// 
/// // Notification handler with Scoped lifetime
/// [MediatorHandler(Lifetime = ServiceLifetime.Scoped)]
/// public class UserCreatedEmailHandler : IPvNugsMediatorNotificationHandler&lt;UserCreatedNotification&gt;
/// {
///     private readonly IEmailService _emailService;
///     
///     public UserCreatedEmailHandler(IEmailService emailService)
///     {
///         _emailService = emailService;
///     }
///     
///     public async Task HandleAsync(UserCreatedNotification notification, CancellationToken ct)
///     {
///         await _emailService.SendWelcomeEmailAsync(notification.Email, ct);
///     }
/// }
/// 
/// // Pipeline behavior with Singleton lifetime
/// [MediatorHandler(Lifetime = ServiceLifetime.Singleton)]
/// public class LoggingPipeline&lt;TRequest, TResponse&gt; 
///     : IPvNugsMediatorPipelineRequestHandler&lt;TRequest, TResponse&gt;
///     where TRequest : IPvNugsMediatorRequest&lt;TResponse&gt;
/// {
///     public async Task&lt;TResponse&gt; HandleAsync(
///         TRequest request, 
///         RequestHandlerDelegate&lt;TResponse&gt; next, 
///         CancellationToken ct)
///     {
///         // Logging implementation
///         return await next();
///     }
/// }
/// 
/// // This handler will NOT be discovered (missing attribute)
/// public class InternalHelper : IPvNugsMediatorRequestHandler&lt;InternalRequest, Result&gt;
/// {
///     // Not decorated, so not registered in Decorated mode
/// }
/// </code>
/// </example>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class MediatorHandlerAttribute : Attribute
{
    /// <summary>
    /// Gets or sets the service lifetime for this handler in the dependency injection container.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Specifies how the DI container should manage the lifetime of this handler instance.
    /// </para>
    /// <list type="bullet">
    /// <item>
    /// <term><see cref="ServiceLifetime.Transient"/></term>
    /// <description>A new instance is created each time it's requested (default for handlers)</description>
    /// </item>
    /// <item>
    /// <term><see cref="ServiceLifetime.Scoped"/></term>
    /// <description>One instance per request/scope (useful for handlers with scoped dependencies like DbContext)</description>
    /// </item>
    /// <item>
    /// <term><see cref="ServiceLifetime.Singleton"/></term>
    /// <description>One instance for the application lifetime (use for stateless handlers)</description>
    /// </item>
    /// </list>
    /// <para>
    /// <b>Default:</b> <see cref="ServiceLifetime.Transient"/>
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Transient (default) - new instance each time
    /// [MediatorHandler]
    /// public class TransientHandler : IPvNugsMediatorRequestHandler&lt;Request, Response&gt; { }
    /// 
    /// // Scoped - one instance per HTTP request
    /// [MediatorHandler(Lifetime = ServiceLifetime.Scoped)]
    /// public class ScopedHandler : IPvNugsMediatorRequestHandler&lt;Request, Response&gt; { }
    /// 
    /// // Singleton - one instance for entire application
    /// [MediatorHandler(Lifetime = ServiceLifetime.Singleton)]
    /// public class SingletonHandler : IPvNugsMediatorRequestHandler&lt;Request, Response&gt; { }
    /// </code>
    /// </example>
    public ServiceLifetime Lifetime { get; init; } = ServiceLifetime.Transient;
}

/// <summary>
/// Specifies the lifetime of a service in the dependency injection container.
/// </summary>
/// <remarks>
/// This enum matches the service lifetimes available in Microsoft.Extensions.DependencyInjection.
/// </remarks>
public enum ServiceLifetime
{
    /// <summary>
    /// A new instance of the service is created every time it is requested.
    /// </summary>
    /// <remarks>
    /// Best for lightweight, stateless services. Each consumer gets a fresh instance.
    /// </remarks>
    Transient = 0,

    /// <summary>
    /// A single instance of the service is created per scope (e.g., per HTTP request in web applications).
    /// </summary>
    /// <remarks>
    /// Best for services that maintain state during a request but should not be shared across requests.
    /// Examples: DbContext, unit of work patterns.
    /// </remarks>
    Scoped = 1,

    /// <summary>
    /// A single instance of the service is created for the entire application lifetime.
    /// </summary>
    /// <remarks>
    /// Best for stateless services or services that can safely be shared. Memory efficient.
    /// Be cautious with thread safety.
    /// </remarks>
    Singleton = 2
}


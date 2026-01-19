namespace pvNugsMediatorNc9Abstractions.Mediator;


/// <summary>
/// Defines the mediator that routes requests to their handlers and publishes notifications to subscribers.
/// </summary>
/// <remarks>
/// <para>
/// The mediator pattern reduces coupling between components by preventing direct dependencies
/// between senders and receivers. This interface provides two primary mechanisms:
/// </para>
/// <list type="bullet">
/// <item>
/// <term>Request/Response</term>
/// <description>Send a request to exactly one handler and receive a response via <see cref="SendAsync{TResponse}"/>.</description>
/// </item>
/// <item>
/// <term>Publish/Subscribe</term>
/// <description>Publish a notification to zero or more handlers via <see cref="PublishAsync{TNotification}"/> or <see cref="PublishAsync(object, CancellationToken)"/>.</description>
/// </item>
/// </list>
/// <para>
/// The mediator automatically discovers and invokes registered handlers from the dependency injection container.
/// Handlers are resolved at runtime, allowing for flexible and testable architectures.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// public class UserService
/// {
///     private readonly IMediator _mediator;
///     
///     public UserService(IMediator mediator)
///     {
///         _mediator = mediator;
///     }
///     
///     public async Task&lt;User&gt; GetUserAsync(int userId)
///     {
///         // Send request and get response
///         var user = await _mediator.SendAsync(new GetUserByIdRequest { UserId = userId });
///         return user;
///     }
///     
///     public async Task CreateUserAsync(User user)
///     {
///         // Send command without meaningful response
///         await _mediator.SendAsync(new CreateUserRequest { User = user });
///         
///         // Publish notification to multiple subscribers
///         await _mediator.PublishAsync(new UserCreatedNotification { UserId = user.Id, Email = user.Email });
///     }
/// }
/// </code>
/// </example>
public interface IMediator
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
    /// </remarks>
    /// <exception cref="InvalidOperationException">
    /// Thrown when no handler is registered for the request type.
    /// </exception>
    /// <example>
    /// <code>
    /// var user = await _mediator.SendAsync(new GetUserByIdRequest { UserId = 123 });
    /// await _mediator.SendAsync(new DeleteUserRequest { UserId = 123 }); // Returns Unit
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
    /// </remarks>
    /// <example>
    /// <code>
    /// await _mediator.PublishAsync&lt;UserCreatedNotification&gt;(
    ///     new UserCreatedNotification { UserId = 123, Email = "user@example.com" });
    /// </code>
    /// </example>
    Task PublishAsync<TNotification>(
        INotification notification, 
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
    /// </remarks>
    /// <example>
    /// <code>
    /// object notification = new UserCreatedNotification { UserId = 123, Email = "user@example.com" };
    /// await _mediator.PublishAsync(notification);
    /// </code>
    /// </example>
    Task PublishAsync(
        object notification, 
        CancellationToken cancellationToken = default);
}
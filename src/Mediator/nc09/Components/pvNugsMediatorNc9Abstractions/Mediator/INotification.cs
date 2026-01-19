namespace pvNugsMediatorNc9Abstractions.Mediator;

/// <summary>
/// Represents a mediator notification that can be published to multiple handlers.
/// </summary>
/// <remarks>
/// <para>
/// This is a marker interface used to identify notification objects in the mediator pattern.
/// Notifications follow a publish/subscribe pattern and can be handled by zero or more
/// <see cref="INotificationHandler{TNotification}"/> instances.
/// </para>
/// <para>
/// Unlike <see cref="IRequest{TResponse}"/>, which expects exactly one handler
/// and returns a response, notifications are fire-and-forget events that don't return values
/// and can have multiple handlers execute concurrently.
/// </para>
/// <para>
/// Publish notifications using <see cref="IMediator.Publish{TNotification}"/>
/// or <see cref="IMediator.Publish(object, CancellationToken)"/>.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// public class UserCreatedNotification : INotification
/// {
///     public int UserId { get; init; }
///     public string Email { get; init; }
/// }
/// 
/// // Multiple handlers can respond to the same notification
/// public class SendWelcomeEmailHandler : INotificationHandler&lt;UserCreatedNotification&gt;
/// {
///     public async Task Handle(UserCreatedNotification notification, CancellationToken cancellationToken)
///     {
///         await _emailService.SendWelcomeEmailAsync(notification.Email);
///     }
/// }
/// 
/// public class LogUserCreationHandler : INotificationHandler&lt;UserCreatedNotification&gt;
/// {
///     public async Task Handle(UserCreatedNotification notification, CancellationToken cancellationToken)
///     {
///         _logger.LogInformation("User {UserId} created", notification.UserId);
///     }
/// }
/// 
/// // Usage:
/// await _mediator.Publish(new UserCreatedNotification { UserId = 123, Email = "user@example.com" });
/// </code>
/// </example>
public interface INotification;
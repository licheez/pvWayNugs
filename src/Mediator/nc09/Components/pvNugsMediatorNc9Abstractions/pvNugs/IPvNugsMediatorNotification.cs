using pvNugsMediatorNc9Abstractions.Mediator;

namespace pvNugsMediatorNc9Abstractions.pvNugs;

/// <summary>
/// PvNugs-branded notification interface that extends the base <see cref="INotification"/> interface.
/// </summary>
/// <remarks>
/// <para>
/// This interface is a marker that inherits all functionality from <see cref="INotification"/>.
/// It identifies notification objects in the PvNugs mediator pattern while maintaining
/// compatibility with the base <see cref="INotification"/> interface.
/// </para>
/// <para>
/// Notifications implementing this interface can be published using 
/// <see cref="IMediator.PublishAsync{TNotification}"/> and will be handled by all registered
/// <see cref="IPvNugsNotificationHandler{TNotification}"/> or <see cref="INotificationHandler{TNotification}"/> instances.
/// </para>
/// <para>
/// Unlike requests, notifications follow a publish/subscribe pattern and can have zero or more handlers.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Define a PvNugs notification
/// public class UserCreatedNotification : IPvNugsMediatorNotification
/// {
///     public int UserId { get; init; }
///     public string Email { get; init; }
/// }
/// 
/// // Multiple handlers can respond to the same notification
/// public class SendWelcomeEmailHandler : IPvNugsNotificationHandler&lt;UserCreatedNotification&gt;
/// {
///     public async Task HandleAsync(UserCreatedNotification notification, CancellationToken cancellationToken)
///     {
///         await _emailService.SendWelcomeEmailAsync(notification.Email, cancellationToken);
///     }
/// }
/// 
/// // Publish the notification
/// await _mediator.PublishAsync(new UserCreatedNotification { UserId = 123, Email = "user@example.com" });
/// </code>
/// </example>
public interface IPvNugsMediatorNotification: INotification;
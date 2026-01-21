using pvNugsMediatorNc9Abstractions.Mediator;

namespace pvNugsMediatorNc9Abstractions.pvNugs;

/// <summary>
/// PvNugs-branded handler for processing a specific type of mediator notification.
/// </summary>
/// <typeparam name="TNotification">
/// The type of notification to be handled. Must implement <see cref="INotification"/>.
/// </typeparam>
/// <remarks>
/// <para>
/// This interface defines the PvNugs-branded notification handler pattern, providing an alternative
/// to <see cref="INotificationHandler{TNotification}"/> with explicit async naming.
/// </para>
/// <para>
/// Unlike request handlers, which have a one-to-one relationship with requests,
/// multiple notification handlers can be registered for the same notification type,
/// and all will be invoked when the notification is published.
/// </para>
/// <para>
/// The PvNugs mediator supports both this interface and <see cref="INotificationHandler{TNotification}"/>,
/// allowing mixed usage patterns and seamless migration from MediatR.
/// </para>
/// <para>
/// The <c>TNotification</c> parameter is marked as contravariant (<c>in</c>) to support
/// handler inheritance patterns when needed.
/// </para>
/// <para>
/// Handlers are automatically discovered and invoked by <see cref="IPvNugsMediator.PublishAsync{TNotification}"/>
/// when a notification is published through the mediator. Handlers typically execute sequentially.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Define a PvNugs notification handler
/// public class SendWelcomeEmailHandler : IPvNugsMediatorNotificationHandler&lt;UserCreatedNotification&gt;
/// {
///     private readonly IEmailService _emailService;
///     
///     public SendWelcomeEmailHandler(IEmailService emailService)
///     {
///         _emailService = emailService;
///     }
///     
///     public async Task HandleAsync(
///         UserCreatedNotification notification, 
///         CancellationToken cancellationToken)
///     {
///         await _emailService.SendWelcomeEmailAsync(notification.Email, cancellationToken);
///     }
/// }
/// 
/// // Register in DI (multiple handlers can be registered for the same notification)
/// services.AddTransient&lt;IPvNugsMediatorNotificationHandler&lt;UserCreatedNotification&gt;, SendWelcomeEmailHandler&gt;();
/// services.AddTransient&lt;IPvNugsMediatorNotificationHandler&lt;UserCreatedNotification&gt;, LogUserCreationHandler&gt;();
/// 
/// // Publish the notification (both handlers will execute)
/// await _mediator.PublishAsync(new UserCreatedNotification { UserId = 123, Email = "user@example.com" });
/// </code>
/// </example>
public interface IPvNugsMediatorNotificationHandler<in TNotification>
    where TNotification : INotification
{
    /// <summary>
    /// Handles the specified notification asynchronously.
    /// </summary>
    /// <param name="notification">
    /// The notification instance containing the event data.
    /// </param>
    /// <param name="cancellationToken">
    /// A token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.
    /// </param>
    /// <returns>
    /// A task that represents the asynchronous operation.
    /// </returns>
    /// <remarks>
    /// <para>
    /// This is the only method that needs to be implemented for PvNugs notification handlers.
    /// The async suffix makes it clear that this method performs asynchronous operations.
    /// </para>
    /// <para>
    /// The mediator will automatically discover and invoke this method when notifications are published.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// public class MyNotificationHandler : IPvNugsMediatorNotificationHandler&lt;MyNotification&gt;
    /// {
    ///     public async Task HandleAsync(MyNotification notification, CancellationToken ct)
    ///     {
    ///         await _service.ProcessAsync(notification);
    ///     }
    /// }
    /// </code>
    /// </example>
    Task HandleAsync(
        TNotification notification, 
        CancellationToken cancellationToken = default);
}

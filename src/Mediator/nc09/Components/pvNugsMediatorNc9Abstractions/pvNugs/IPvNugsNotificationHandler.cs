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
/// This interface extends <see cref="INotificationHandler{TNotification}"/> and serves as
/// a marker to identify PvNugs notification handlers while maintaining compatibility with
/// the base interface.
/// </para>
/// <para>
/// Unlike request handlers, which have a one-to-one relationship with requests,
/// multiple notification handlers can be registered for the same notification type,
/// and all will be invoked when the notification is published.
/// </para>
/// <para>
/// The <c>TNotification</c> parameter is marked as contravariant (<c>in</c>) to support
/// handler inheritance patterns when needed.
/// </para>
/// <para>
/// Handlers are automatically discovered and invoked by <see cref="IMediator.Publish{TNotification}"/>
/// when a notification is published through the mediator. Handlers typically execute concurrently.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Define a PvNugs notification handler
/// public class SendWelcomeEmailHandler : IPvNugsNotificationHandler&lt;UserCreatedNotification&gt;
/// {
///     private readonly IEmailService _emailService;
///     
///     public SendWelcomeEmailHandler(IEmailService emailService)
///     {
///         _emailService = emailService;
///     }
///     
///     // Implement HandleAsync
///     public async Task HandleAsync(
///         UserCreatedNotification notification, 
///         CancellationToken cancellationToken)
///     {
///         await _emailService.SendWelcomeEmailAsync(notification.Email, cancellationToken);
///     }
///     
///     // Delegate Handle to HandleAsync for MediatR compatibility
///     public Task Handle(UserCreatedNotification notification, CancellationToken cancellationToken)
///         => HandleAsync(notification, cancellationToken);
/// }
/// 
/// // Register in DI (multiple handlers can be registered for the same notification)
/// services.AddTransient&lt;IPvNugsNotificationHandler&lt;UserCreatedNotification&gt;, SendWelcomeEmailHandler&gt;();
/// services.AddTransient&lt;IPvNugsNotificationHandler&lt;UserCreatedNotification&gt;, LogUserCreationHandler&gt;();
/// 
/// // Publish the notification (both handlers will execute)
/// await _mediator.Publish(new UserCreatedNotification { UserId = 123, Email = "user@example.com" });
/// </code>
/// </example>
public interface IPvNugsNotificationHandler<in TNotification> : 
    INotificationHandler<TNotification>
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
    /// This method provides an alternative, more explicit naming convention for handling notifications.
    /// By default, it delegates to the <see cref="INotificationHandler{TNotification}.Handle"/> method
    /// to maintain MediatR compatibility.
    /// </para>
    /// <para>
    /// Implementers can choose to:
    /// </para>
    /// <list type="bullet">
    /// <item><description>Implement only <c>Handle</c> (MediatR style) - <c>HandleAsync</c> will call it automatically</description></item>
    /// <item><description>Implement only <c>HandleAsync</c> (explicit async) - <c>Handle</c> must call it</description></item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Option 1: Implement HandleAsync only
    /// public class MyNotificationHandler : IPvNugsNotificationHandler&lt;MyNotification&gt;
    /// {
    ///     public async Task HandleAsync(MyNotification notification, CancellationToken ct)
    ///     {
    ///         // Your implementation
    ///         await _service.ProcessAsync(notification);
    ///     }
    ///     
    ///     // Handle calls HandleAsync
    ///     public Task Handle(MyNotification notification, CancellationToken ct)
    ///         => HandleAsync(notification, ct);
    /// }
    /// 
    /// // Option 2: Implement Handle only (MediatR compatible)
    /// public class MyNotificationHandler : IPvNugsNotificationHandler&lt;MyNotification&gt;
    /// {
    ///     public async Task Handle(MyNotification notification, CancellationToken ct)
    ///     {
    ///         // Your implementation
    ///         await _service.ProcessAsync(notification);
    ///     }
    ///     // HandleAsync calls Handle automatically (default implementation)
    /// }
    /// </code>
    /// </example>
    Task HandleAsync(
        TNotification notification,
        CancellationToken cancellationToken = default);
}

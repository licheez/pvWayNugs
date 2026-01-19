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
/// Handlers are automatically discovered and invoked by <see cref="IMediator.PublishAsync{TNotification}"/>
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
///     public async Task HandleAsync(
///         UserCreatedNotification notification, 
///         CancellationToken cancellationToken)
///     {
///         await _emailService.SendWelcomeEmailAsync(notification.Email, cancellationToken);
///     }
/// }
/// 
/// // Register in DI (multiple handlers can be registered for the same notification)
/// services.AddTransient&lt;IPvNugsNotificationHandler&lt;UserCreatedNotification&gt;, SendWelcomeEmailHandler&gt;();
/// services.AddTransient&lt;IPvNugsNotificationHandler&lt;UserCreatedNotification&gt;, LogUserCreationHandler&gt;();
/// 
/// // Publish the notification (both handlers will execute)
/// await _mediator.PublishAsync(new UserCreatedNotification { UserId = 123, Email = "user@example.com" });
/// </code>
/// </example>
public interface IPvNugsNotificationHandler<in TNotification> : 
    INotificationHandler<TNotification>
    where TNotification : INotification;
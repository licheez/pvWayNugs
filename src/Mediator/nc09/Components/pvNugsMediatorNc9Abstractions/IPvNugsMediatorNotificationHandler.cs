namespace pvNugsMediatorNc9Abstractions;

/// <summary>
/// Defines a handler for processing a specific type of mediator notification.
/// </summary>
/// <typeparam name="TNotification">
/// The type of notification to be handled. Must implement <see cref="IPvNugsMediatorNotification"/>.
/// </typeparam>
/// <remarks>
/// <para>
/// This interface is part of the mediator pattern's publish/subscribe implementation.
/// Unlike request handlers, which have a one-to-one relationship with requests,
/// multiple notification handlers can be registered for the same notification type,
/// and all will be invoked when the notification is published.
/// </para>
/// <para>
/// The <c>TNotification</c> parameter is marked as contravariant (<c>in</c>) to support
/// handler inheritance patterns when needed.
/// </para>
/// <para>
/// Handlers are automatically discovered and invoked by 
/// <see cref="IPvNugsMediator.PublishAsync{TNotification}"/> when a notification
/// is published through the mediator. Handlers typically execute concurrently.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// public class UserCreatedNotification : IPvNugsMediatorNotification
/// {
///     public int UserId { get; init; }
///     public string Email { get; init; }
/// }
/// 
/// // First handler - sends welcome email
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
/// // Second handler - logs the event
/// public class LogUserCreationHandler : IPvNugsMediatorNotificationHandler&lt;UserCreatedNotification&gt;
/// {
///     private readonly ILogger _logger;
///     
///     public LogUserCreationHandler(ILogger logger)
///     {
///         _logger = logger;
///     }
///     
///     public async Task HandleAsync(
///         UserCreatedNotification notification, 
///         CancellationToken cancellationToken)
///     {
///         _logger.LogInformation("User {UserId} was created", notification.UserId);
///         await Task.CompletedTask;
///     }
/// }
/// 
/// // Registration in DI:
/// services.AddTransient&lt;IPvNugsMediatorNotificationHandler&lt;UserCreatedNotification&gt;, SendWelcomeEmailHandler&gt;();
/// services.AddTransient&lt;IPvNugsMediatorNotificationHandler&lt;UserCreatedNotification&gt;, LogUserCreationHandler&gt;();
/// 
/// // Usage - both handlers will execute:
/// await _mediator.PublishAsync(new UserCreatedNotification { UserId = 123, Email = "user@example.com" });
/// </code>
/// </example>
public interface IPvNugsMediatorNotificationHandler<in TNotification>
    where TNotification : IPvNugsMediatorNotification
{
    /// <summary>
    /// Handles the specified notification.
    /// </summary>
    /// <param name="notification">
    /// The notification instance containing the data related to the event.
    /// </param>
    /// <param name="cancellationToken">
    /// A token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.
    /// </param>
    /// <returns>
    /// A task that represents the asynchronous operation.
    /// </returns>
    /// <remarks>
    /// <para>
    /// This method should contain the logic for responding to the notification.
    /// It is invoked automatically by the mediator when <see cref="IPvNugsMediator.PublishAsync{TNotification}"/>
    /// is called with a matching notification type.
    /// </para>
    /// <para>
    /// When multiple handlers are registered for the same notification type, they typically
    /// execute concurrently. Ensure your handler implementation is thread-safe if necessary.
    /// </para>
    /// <para>
    /// Unlike request handlers, notification handlers do not return values. If one handler
    /// throws an exception, it may or may not affect other handlers depending on the mediator
    /// implementation.
    /// </para>
    /// </remarks>
    Task HandleAsync(
        TNotification notification, 
        CancellationToken cancellationToken = default);
}


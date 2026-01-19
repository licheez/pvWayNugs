// ReSharper disable UnusedAutoPropertyAccessor.Global
namespace pvNugsMediatorNc9Abstractions.pvNugs;

/// <summary>
/// Contains information about a registered mediator component in the DI container.
/// </summary>
/// <remarks>
/// This type is used by <see cref="IPvNugsMediator.GetRegisteredHandlers"/> to provide
/// diagnostic information about registered request handlers, notification handlers,
/// and pipeline behaviors. Primarily intended for development and debugging scenarios.
/// </remarks>
// ReSharper disable once ClassNeverInstantiated.Global
public sealed class MediatorRegistrationInfo
{
    /// <summary>
    /// Gets the type of registration (e.g., "Request Handler", "Notification Handler", "Pipeline Behavior").
    /// </summary>
    public required string RegistrationType { get; init; }
    
    /// <summary>
    /// Gets the service interface type that was registered.
    /// </summary>
    /// <remarks>
    /// For example: <c>IPvNugsMediatorRequestHandler&lt;GetUserRequest, User&gt;</c>
    /// </remarks>
    public required Type ServiceType { get; init; }
    
    /// <summary>
    /// Gets the concrete implementation type that handles the service.
    /// </summary>
    /// <remarks>
    /// For example: <c>GetUserRequestHandler</c>
    /// </remarks>
    public required Type ImplementationType { get; init; }
    
    /// <summary>
    /// Gets the service lifetime (Singleton, Scoped, or Transient).
    /// </summary>
    public required string Lifetime { get; init; }
    
    /// <summary>
    /// Gets the request type for request handlers, or null for other component types.
    /// </summary>
    /// <remarks>
    /// For request handlers: the <c>TRequest</c> type parameter.
    /// For notification handlers: the <c>TNotification</c> type parameter.
    /// For pipeline behaviors: null.
    /// </remarks>
    public Type? MessageType { get; init; }
    
    /// <summary>
    /// Gets the response type for request handlers, or null for notifications and pipelines.
    /// </summary>
    /// <remarks>
    /// Only populated for request handlers with the <c>TResponse</c> type parameter.
    /// </remarks>
    public Type? ResponseType { get; init; }
    
    /// <summary>
    /// Returns a formatted string representation of the registration.
    /// </summary>
    /// <returns>A string describing the registration in human-readable format.</returns>
    public override string ToString()
    {
        var message = MessageType != null ? $" [{MessageType.Name}]" : "";
        var response = ResponseType != null ? $" → {ResponseType.Name}" : "";
        return $"{RegistrationType}{message}{response}: {ImplementationType.Name} ({Lifetime})";
    }
}


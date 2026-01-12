namespace pvNugsMediatorNc9Abstractions;

/// <summary>
/// Represents a mediator request that expects a response of type <typeparamref name="TResponse"/>.
/// </summary>
/// <typeparam name="TResponse">
/// The type of the response that will be returned when this request is handled.
/// Use <see cref="Unit"/> for requests that don't need to return a meaningful value.
/// </typeparam>
/// <remarks>
/// <para>
/// This interface is a marker interface used to identify request objects in the mediator pattern.
/// Requests are sent through the <see cref="IPvNugsMediator.SendAsync{TResponse}"/> method 
/// and are processed by a corresponding <see cref="IPvNugsMediatorRequestHandler{TRequest, TResponse}"/>.
/// </para>
/// <para>
/// Unlike <see cref="IPvNugsMediatorNotification"/>, which supports multiple handlers (publish/subscribe),
/// a request is handled by exactly one handler and returns a response.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// public class GetUserByIdRequest : IPvNugsMediatorRequest&lt;User&gt;
/// {
///     public int UserId { get; init; }
/// }
/// 
/// public class GetUserByIdHandler : IPvNugsMediatorRequestHandler&lt;GetUserByIdRequest, User&gt;
/// {
///     public async Task&lt;User&gt; HandleAsync(GetUserByIdRequest request, CancellationToken cancellationToken)
///     {
///         // Retrieve and return user
///         return await _userRepository.GetByIdAsync(request.UserId, cancellationToken);
///     }
/// }
/// 
/// // Usage:
/// var user = await _mediator.SendAsync(new GetUserByIdRequest { UserId = 123 });
/// </code>
/// </example>
public interface IPvNugsMediatorRequest<TResponse>;
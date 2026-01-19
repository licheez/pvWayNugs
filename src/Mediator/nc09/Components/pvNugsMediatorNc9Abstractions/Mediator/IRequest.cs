using pvNugsMediatorNc9Abstractions.pvNugs;

namespace pvNugsMediatorNc9Abstractions.Mediator;


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
/// Requests are sent through the <see cref="IMediator.SendAsync{TResponse}"/> method 
/// and are processed by a corresponding <see cref="IRequestHandler{TRequest,TResponse}"/>.
/// </para>
/// <para>
/// Unlike <see cref="INotification"/>, which supports multiple handlers (publish/subscribe),
/// a request is handled by exactly one handler and returns a response.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// public class GetUserByIdRequest : IRequest&lt;User&gt;
/// {
///     public int UserId { get; init; }
/// }
/// 
/// public class GetUserByIdHandler : IRequestHandler&lt;GetUserByIdRequest, User&gt;
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
public interface IRequest<TResponse>;

/// <summary>
/// Represents a mediator request that performs an action without returning a meaningful value.
/// </summary>
/// <remarks>
/// This is a convenience interface that inherits from <see cref="IRequest{TResponse}"/>
/// with <see cref="Unit"/> as the response type. Use this for requests that perform operations
/// but don't need to return data (similar to void methods).
/// </remarks>
/// <example>
/// <code>
/// public class DeleteUserRequest : IRequest
/// {
///     public int UserId { get; init; }
/// }
/// 
/// public class DeleteUserHandler : IRequestHandler&lt;DeleteUserRequest&gt;
/// {
///     public async Task&lt;Unit&gt; HandleAsync(DeleteUserRequest request, CancellationToken cancellationToken)
///     {
///         await _userRepository.DeleteAsync(request.UserId, cancellationToken);
///         return Unit.Value;
///     }
/// }
/// </code>
/// </example>
public interface IRequest : IRequest<Unit>;

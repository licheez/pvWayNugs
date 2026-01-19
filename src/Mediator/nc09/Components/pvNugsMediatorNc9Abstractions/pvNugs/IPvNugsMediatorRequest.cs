using pvNugsMediatorNc9Abstractions.Mediator;

namespace pvNugsMediatorNc9Abstractions.pvNugs;

/// <summary>
/// PvNugs-branded request interface that expects a response of type <typeparamref name="TResponse"/>.
/// </summary>
/// <typeparam name="TResponse">
/// The type of the response that will be returned when this request is handled.
/// Use <see cref="Unit"/> for requests that don't need to return a meaningful value.
/// </typeparam>
/// <remarks>
/// <para>
/// This interface extends <see cref="IRequest{TResponse}"/> and serves as a marker
/// to identify PvNugs request objects in the mediator pattern while maintaining
/// compatibility with the base interface.
/// </para>
/// <para>
/// Requests are sent through <see cref="IMediator.SendAsync{TResponse}"/> and are processed
/// by exactly one corresponding <see cref="IPvNugsMediatorRequestHandler{TRequest,TResponse}"/>
/// or <see cref="IRequestHandler{TRequest,TResponse}"/>.
/// </para>
/// <para>
/// Unlike notifications, requests expect a response and have a one-to-one relationship with their handler.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Define a PvNugs request
/// public class GetUserByIdRequest : IPvNugsMediatorRequest&lt;User&gt;
/// {
///     public int UserId { get; init; }
/// }
/// 
/// // Define the handler
/// public class GetUserByIdHandler : IPvNugsMediatorRequestHandler&lt;GetUserByIdRequest, User&gt;
/// {
///     public async Task&lt;User&gt; HandleAsync(GetUserByIdRequest request, CancellationToken cancellationToken)
///     {
///         return await _userRepository.GetByIdAsync(request.UserId, cancellationToken);
///     }
/// }
/// 
/// // Send the request
/// var user = await _mediator.SendAsync(new GetUserByIdRequest { UserId = 123 });
/// </code>
/// </example>
public interface IPvNugsMediatorRequest<TResponse> : IRequest<TResponse>;

/// <summary>
/// PvNugs-branded request interface that performs an action without returning a meaningful value.
/// </summary>
/// <remarks>
/// <para>
/// This is a convenience interface that inherits from both <see cref="IPvNugsMediatorRequest{TResponse}"/>
/// and <see cref="IRequest"/> with <see cref="Unit"/> as the response type.
/// Use this for command-style requests that perform operations but don't need to return data
/// (similar to void methods).
/// </para>
/// <para>
/// Handlers implementing <see cref="IPvNugsMediatorRequestHandler{TRequest}"/> for this request type
/// should return <see cref="Unit.Value"/> to indicate completion.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Define a void-like request
/// public class DeleteUserRequest : IPvNugsMediatorRequest
/// {
///     public int UserId { get; init; }
/// }
/// 
/// // Define the handler
/// public class DeleteUserHandler : IPvNugsMediatorRequestHandler&lt;DeleteUserRequest&gt;
/// {
///     public async Task&lt;Unit&gt; HandleAsync(DeleteUserRequest request, CancellationToken cancellationToken)
///     {
///         await _userRepository.DeleteAsync(request.UserId, cancellationToken);
///         return Unit.Value;
///     }
/// }
/// 
/// // Send the request
/// await _mediator.SendAsync(new DeleteUserRequest { UserId = 123 });
/// </code>
/// </example>
public interface IPvNugsMediatorRequest : IPvNugsMediatorRequest<Unit>, IRequest;

using pvNugsMediatorNc9Abstractions.Mediator;

namespace pvNugsMediatorNc9Abstractions.pvNugs;

/// <summary>
/// PvNugs-branded handler for processing a specific type of mediator request and producing a response.
/// </summary>
/// <typeparam name="TRequest">
/// The type of request to be handled. Must implement <see cref="IRequest{TResponse}"/>.
/// </typeparam>
/// <typeparam name="TResponse">
/// The type of response that will be returned after handling the request.
/// Use <see cref="Unit"/> for requests that don't need to return a meaningful value.
/// </typeparam>
/// <remarks>
/// <para>
/// This interface extends <see cref="IRequestHandler{TRequest,TResponse}"/> and serves as
/// a marker to identify PvNugs request handlers while maintaining compatibility with
/// the base interface.
/// </para>
/// <para>
/// Each request type should have exactly one handler registered in the dependency injection container.
/// The handler encapsulates the business logic for processing the request and is automatically
/// discovered and invoked by <see cref="IMediator.SendAsync{TResponse}"/>.
/// </para>
/// <para>
/// The <c>TRequest</c> parameter is marked as contravariant (<c>in</c>) to support handler
/// inheritance patterns when needed.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Define a PvNugs request handler
/// public class GetUserByIdHandler : IPvNugsMediatorRequestHandler&lt;GetUserByIdRequest, User&gt;
/// {
///     private readonly IUserRepository _userRepository;
///     
///     public GetUserByIdHandler(IUserRepository userRepository)
///     {
///         _userRepository = userRepository;
///     }
///     
///     public async Task&lt;User&gt; HandleAsync(
///         GetUserByIdRequest request, 
///         CancellationToken cancellationToken)
///     {
///         return await _userRepository.GetByIdAsync(request.UserId, cancellationToken);
///     }
/// }
/// 
/// // Register in DI
/// services.AddTransient&lt;IPvNugsMediatorRequestHandler&lt;GetUserByIdRequest, User&gt;, GetUserByIdHandler&gt;();
/// 
/// // Usage
/// var user = await _mediator.SendAsync(new GetUserByIdRequest { UserId = 123 });
/// </code>
/// </example>
public interface IPvNugsMediatorRequestHandler<in TRequest, TResponse>: 
    IRequestHandler<TRequest, TResponse> 
    where TRequest : IRequest<TResponse>;

/// <summary>
/// PvNugs-branded handler for processing a mediator request that doesn't return a meaningful value.
/// </summary>
/// <typeparam name="TRequest">
/// The type of request to be handled. Must implement <see cref="IRequest"/>.
/// </typeparam>
/// <remarks>
/// <para>
/// This is a convenience interface that inherits from both 
/// <see cref="IPvNugsMediatorRequestHandler{TRequest,TResponse}"/> and
/// <see cref="IRequestHandler{TRequest}"/> with <see cref="Unit"/> as the response type.
/// Use this for command-style handlers that perform actions but don't need to return data
/// (similar to void methods).
/// </para>
/// <para>
/// Handlers implementing this interface should return <see cref="Unit.Value"/> 
/// from their <see cref="IRequestHandler{TRequest,TResponse}.HandleAsync"/> method.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Define a void-like handler
/// public class DeleteUserHandler : IPvNugsMediatorRequestHandler&lt;DeleteUserRequest&gt;
/// {
///     private readonly IUserRepository _userRepository;
///     
///     public DeleteUserHandler(IUserRepository userRepository)
///     {
///         _userRepository = userRepository;
///     }
///     
///     public async Task&lt;Unit&gt; HandleAsync(
///         DeleteUserRequest request, 
///         CancellationToken cancellationToken)
///     {
///         await _userRepository.DeleteAsync(request.UserId, cancellationToken);
///         return Unit.Value;
///     }
/// }
/// 
/// // Register in DI
/// services.AddTransient&lt;IPvNugsMediatorRequestHandler&lt;DeleteUserRequest&gt;, DeleteUserHandler&gt;();
/// 
/// // Usage
/// await _mediator.SendAsync(new DeleteUserRequest { UserId = 123 });
/// </code>
/// </example>
public interface IPvNugsMediatorRequestHandler<in TRequest>: 
    IPvNugsMediatorRequestHandler<TRequest, Unit>,
    IRequestHandler<TRequest>
    where TRequest : IRequest;

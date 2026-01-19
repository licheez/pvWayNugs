
namespace pvNugsMediatorNc9Abstractions.Mediator;

/// <summary>
/// Defines a handler for processing a specific type of mediator request and producing a response.
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
/// This interface is part of the mediator pattern implementation. Each request type should have
/// exactly one corresponding handler registered in the dependency injection container.
/// The handler encapsulates the business logic for processing the request.
/// </para>
/// <para>
/// The <c>TRequest</c> parameter is marked as contravariant (<c>in</c>) to support handler
/// inheritance patterns when needed.
/// </para>
/// <para>
/// Handlers are automatically discovered and invoked by <see cref="IMediator.Send{TResponse}"/>
/// when a request is sent through the mediator.
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
///     private readonly IUserRepository _userRepository;
///     
///     public GetUserByIdHandler(IUserRepository userRepository)
///     {
///         _userRepository = userRepository;
///     }
///     
///     public async Task&lt;User&gt; Handle(
///         GetUserByIdRequest request, 
///         CancellationToken cancellationToken)
///     {
///         var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);
///         
///         if (user == null)
///             throw new UserNotFoundException(request.UserId);
///             
///         return user;
///     }
/// }
/// 
/// // Registration in DI:
/// services.AddTransient&lt;IRequestHandler&lt;GetUserByIdRequest, User&gt;, GetUserByIdHandler&gt;();
/// 
/// // Usage:
/// var user = await _mediator.Send(new GetUserByIdRequest { UserId = 123 });
/// </code>
/// </example>
public interface IRequestHandler<in TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    /// <summary>
    /// Handles the specified request and returns a response.
    /// </summary>
    /// <param name="request">
    /// The request instance containing the data needed to process the operation.
    /// </param>
    /// <param name="cancellationToken">
    /// A token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.
    /// </param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains
    /// the response of type <typeparamref name="TResponse"/>.
    /// </returns>
    /// <remarks>
    /// <para>
    /// This method should contain the business logic for processing the request.
    /// It is invoked automatically by the mediator when <see cref="IMediator.Send{TResponse}"/>
    /// is called with a matching request type.
    /// </para>
    /// <para>
    /// <b>MediatR Compatibility:</b> This method uses the same name as MediatR's <c>Handle</c> method.
    /// </para>
    /// </remarks>
    Task<TResponse> Handle(
        TRequest request,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Defines a handler for processing a mediator request that doesn't return a meaningful value.
/// </summary>
/// <typeparam name="TRequest">
/// The type of request to be handled. Must implement <see cref="IRequest"/>.
/// </typeparam>
/// <remarks>
/// <para>
/// This is a convenience interface that inherits from 
/// <see cref="IRequestHandler{TRequest, TResponse}"/> with <see cref="Unit"/> 
/// as the response type. Use this for command-style handlers that perform actions 
/// but don't need to return data (similar to void methods).
/// </para>
/// <para>
/// Handlers implementing this interface should return <see cref="Unit.Value"/> 
/// from their <see cref="IRequestHandler{TRequest, TResponse}.Handle"/> method.
/// </para>
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
///     private readonly IUserRepository _userRepository;
///     
///     public DeleteUserHandler(IUserRepository userRepository)
///     {
///         _userRepository = userRepository;
///     }
///     
///     public async Task&lt;Unit&gt; Handle(
///         DeleteUserRequest request, 
///         CancellationToken cancellationToken)
///     {
///         await _userRepository.DeleteAsync(request.UserId, cancellationToken);
///         return Unit.Value; // Return Unit to indicate completion
///     }
/// }
/// 
/// // Registration in DI:
/// services.AddTransient&lt;IRequestHandler&lt;DeleteUserRequest&gt;, DeleteUserHandler&gt;();
/// 
/// // Usage:
/// await _mediator.Send(new DeleteUserRequest { UserId = 123 });
/// </code>
/// </example>
public interface IRequestHandler<in TRequest>: 
    IRequestHandler<TRequest, Unit>
    where TRequest : IRequest;


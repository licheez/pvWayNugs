
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
/// This is a convenience interface for command-style handlers that perform actions 
/// but don't need to return data (similar to void methods). Unlike the two-parameter
/// version, this interface returns <see cref="Task"/> instead of <see cref="Task{Unit}"/>,
/// eliminating the need to return <see cref="Unit.Value"/>.
/// </para>
/// <para>
/// <b>MediatR Compatibility:</b> This interface provides a cleaner alternative to MediatR's
/// approach of returning Task&lt;Unit&gt;, while maintaining the same interface name for
/// easy migration. Existing code using the Handle method name will work seamlessly.
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
///     public async Task Handle(
///         DeleteUserRequest request, 
///         CancellationToken cancellationToken)
///     {
///         await _userRepository.DeleteAsync(request.UserId, cancellationToken);
///         // No need to return Unit.Value!
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
public interface IRequestHandler<in TRequest>
    where TRequest : IRequest
{
    /// <summary>
    /// Handles the specified request asynchronously.
    /// </summary>
    /// <param name="request">
    /// The request instance containing the data needed to process the operation.
    /// </param>
    /// <param name="cancellationToken">
    /// A token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.
    /// </param>
    /// <returns>
    /// A task that represents the asynchronous operation.
    /// </returns>
    /// <remarks>
    /// <para>
    /// This method should contain the business logic for processing the request.
    /// Unlike handlers that return a specific response type, this method returns
    /// <see cref="Task"/> instead of <see cref="Task{Unit}"/>, providing a more
    /// natural async/await experience.
    /// </para>
    /// <para>
    /// <b>MediatR Compatibility:</b> This method uses the same name as MediatR's Handle method.
    /// </para>
    /// </remarks>
    Task Handle(
        TRequest request,
        CancellationToken cancellationToken = default);
}

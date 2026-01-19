namespace pvNugsMediatorNc9Abstractions.Mediator;

/// <summary>
/// Represents a delegate that invokes the next handler in the mediator pipeline.
/// </summary>
/// <typeparam name="TResponse">
/// The type of response that will be returned by the request handler.
/// </typeparam>
/// <returns>
/// A task that represents the asynchronous operation. The task result contains
/// the response of type <typeparamref name="TResponse"/>.
/// </returns>
/// <remarks>
/// This delegate is used in the pipeline pattern to chain multiple handlers together.
/// Each pipeline handler can execute logic before and/or after calling the next delegate.
/// </remarks>
public delegate Task<TResponse> RequestHandlerDelegate<TResponse>();

/// <summary>
/// Defines a pipeline behavior that can intercept and process mediator requests
/// as they flow through the request handling pipeline.
/// </summary>
/// <typeparam name="TRequest">
/// The type of request being processed. Must implement <see cref="IRequest{TResponse}"/>.
/// </typeparam>
/// <typeparam name="TResponse">
/// The type of response that will be returned after handling the request.
/// </typeparam>
/// <remarks>
/// <para>
/// Pipeline behaviors implement cross-cutting concerns such as logging, validation,
/// caching, exception handling, or performance monitoring. They wrap around the actual
/// request handler, executing before and/or after the handler processes the request.
/// </para>
/// <para>
/// Multiple pipeline behaviors can be registered and will be executed in order,
/// forming a chain of responsibility. Each pipeline behavior must call the <c>next</c>
/// delegate to continue the pipeline, or short-circuit by returning early.
/// </para>
/// <para>
/// The <c>TRequest</c> parameter is marked as contravariant (<c>in</c>) to support
/// pipeline inheritance patterns when needed.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Logging pipeline
/// public class LoggingPipelineBehavior&lt;TRequest, TResponse&gt; 
///     : IPipelineBehavior&lt;TRequest, TResponse&gt;
///     where TRequest : IRequest&lt;TResponse&gt;
/// {
///     private readonly ILogger _logger;
///     
///     public LoggingPipelineBehavior(ILogger logger)
///     {
///         _logger = logger;
///     }
///     
///     public async Task&lt;TResponse&gt; HandleAsync(
///         TRequest request,
///         RequestHandlerDelegate&lt;TResponse&gt; next,
///         CancellationToken cancellationToken)
///     {
///         _logger.LogInformation("Handling request {RequestType}", typeof(TRequest).Name);
///         
///         try
///         {
///             var response = await next(); // Continue to next handler in pipeline
///             
///             _logger.LogInformation("Request {RequestType} completed successfully", typeof(TRequest).Name);
///             return response;
///         }
///         catch (Exception ex)
///         {
///             _logger.LogError(ex, "Request {RequestType} failed", typeof(TRequest).Name);
///             throw;
///         }
///     }
/// }
/// 
/// // Validation pipeline that short-circuits
/// public class ValidationPipelineBehavior&lt;TRequest, TResponse&gt; 
///     : IPipelineBehavior&lt;TRequest, TResponse&gt;
///     where TRequest : IRequest&lt;TResponse&gt;
/// {
///     public async Task&lt;TResponse&gt; HandleAsync(
///         TRequest request,
///         RequestHandlerDelegate&lt;TResponse&gt; next,
///         CancellationToken cancellationToken)
///     {
///         if (request is IValidatable validatable)
///         {
///             var errors = validatable.Validate();
///             if (errors.Any())
///                 throw new ValidationException(errors);
///         }
///         
///         return await next(); // Continue only if validation passes
///     }
/// }
/// </code>
/// </example>
public interface IPipelineBehavior<in TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    /// <summary>
    /// Handles the request within the pipeline, optionally executing logic before and/or after
    /// invoking the next handler in the chain.
    /// </summary>
    /// <param name="request">
    /// The request instance containing the data needed to process the operation.
    /// </param>
    /// <param name="next">
    /// A delegate that invokes the next handler in the pipeline. Call this to continue
    /// processing the request, or skip it to short-circuit the pipeline.
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
    /// This method is invoked as part of the request handling pipeline. Execute your
    /// pre-processing logic first, then call <paramref name="next"/> to continue the pipeline,
    /// and finally execute any post-processing logic after <paramref name="next"/> returns.
    /// </para>
    /// <para>
    /// To short-circuit the pipeline (prevent further handlers from executing), return
    /// a response without calling the <paramref name="next"/> delegate.
    /// </para>
    /// </remarks>
    Task<TResponse> HandleAsync(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken = default);
}
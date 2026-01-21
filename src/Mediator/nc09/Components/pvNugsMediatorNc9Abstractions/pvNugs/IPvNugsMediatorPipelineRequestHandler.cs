using pvNugsMediatorNc9Abstractions.Mediator;

namespace pvNugsMediatorNc9Abstractions.pvNugs;

/// <summary>
/// PvNugs-branded pipeline behavior for intercepting and processing mediator requests
/// that flow through the request handling pipeline.
/// </summary>
/// <typeparam name="TRequest">
/// The type of PvNugs request being processed. Must implement <see cref="IPvNugsMediatorRequest{TResponse}"/>.
/// </typeparam>
/// <typeparam name="TResponse">
/// The type of response that will be returned after handling the request.
/// </typeparam>
/// <remarks>
/// <para>
/// This interface extends <see cref="IPipelineBehavior{TRequest,TResponse}"/> and serves as
/// a marker to identify PvNugs pipeline behaviors while maintaining compatibility with
/// the base interface.
/// </para>
/// <para>
/// Pipeline behaviors implement cross-cutting concerns such as logging, validation,
/// caching, exception handling, or performance monitoring. They wrap around the actual
/// request handler, executing before and/or after the handler processes the request.
/// </para>
/// <para>
/// Unlike the base <see cref="IPipelineBehavior{TRequest,TResponse}"/>, this interface
/// constrains the request type to <see cref="IPvNugsMediatorRequest{TResponse}"/>, ensuring
/// type safety and that only PvNugs-branded requests flow through PvNugs pipeline behaviors.
/// </para>
/// <para>
/// Multiple pipeline behaviors can be registered and will be executed in order,
/// forming a chain of responsibility. Each behavior must call the <c>next</c>
/// delegate to continue the pipeline, or short-circuit by returning early.
/// </para>
/// <para>
/// The <c>TRequest</c> parameter is marked as contravariant (<c>in</c>) to support
/// pipeline inheritance patterns when needed.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Logging pipeline behavior
/// public class LoggingPipelineBehavior&lt;TRequest, TResponse&gt; 
///     : IPvNugsMediatorPipelineRequestHandler&lt;TRequest, TResponse&gt;
///     where TRequest : IPvNugsMediatorRequest&lt;TResponse&gt;
/// {
///     private readonly ILogger&lt;LoggingPipelineBehavior&lt;TRequest, TResponse&gt;&gt; _logger;
///     
///     public LoggingPipelineBehavior(ILogger&lt;LoggingPipelineBehavior&lt;TRequest, TResponse&gt;&gt; logger)
///     {
///         _logger = logger;
///     }
///     
///     public async Task&lt;TResponse&gt; Handle(
///         TRequest request,
///         RequestHandlerDelegate&lt;TResponse&gt; next,
///         CancellationToken cancellationToken)
///     {
///         var requestName = typeof(TRequest).Name;
///         _logger.LogInformation("Handling PvNugs request {RequestName}", requestName);
///         
///         try
///         {
///             var stopwatch = Stopwatch.StartNew();
///             var response = await next(); // Continue to next handler in pipeline
///             stopwatch.Stop();
///             
///             _logger.LogInformation(
///                 "Request {RequestName} completed successfully in {ElapsedMs}ms", 
///                 requestName, 
///                 stopwatch.ElapsedMilliseconds);
///             
///             return response;
///         }
///         catch (Exception ex)
///         {
///             _logger.LogError(ex, "Request {RequestName} failed with exception", requestName);
///             throw;
///         }
///     }
/// }
/// 
/// // Validation pipeline that short-circuits
/// public class ValidationPipelineBehavior&lt;TRequest, TResponse&gt; 
///     : IPvNugsMediatorPipelineRequestHandler&lt;TRequest, TResponse&gt;
///     where TRequest : IPvNugsMediatorRequest&lt;TResponse&gt;
/// {
///     public async Task&lt;TResponse&gt; Handle(
///         TRequest request,
///         RequestHandlerDelegate&lt;TResponse&gt; next,
///         CancellationToken cancellationToken)
///     {
///         // Validate before continuing
///         if (request is IValidatable validatable)
///         {
///             var validationResult = validatable.Validate();
///             if (!validationResult.IsValid)
///             {
///                 throw new ValidationException(validationResult.Errors);
///             }
///         }
///         
///         return await next(); // Continue only if validation passes
///     }
/// }
/// 
/// // Register in DI (executed in registration order)
/// services.AddTransient(typeof(IPvNugsMediatorPipelineRequestHandler&lt;,&gt;), typeof(LoggingPipelineBehavior&lt;,&gt;));
/// services.AddTransient(typeof(IPvNugsMediatorPipelineRequestHandler&lt;,&gt;), typeof(ValidationPipelineBehavior&lt;,&gt;));
/// 
/// // Usage - pipeline behaviors execute automatically when sending requests:
/// var user = await _mediator.Send(new GetUserByIdRequest { UserId = 123 });
/// // Order: Logging (before) → Validation (before) → Handler → Validation (after) → Logging (after)
/// </code>
/// </example>
public interface IPvNugsMediatorPipelineRequestHandler<in TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    /// <summary>
    /// Handles the request asynchronously within the pipeline, optionally executing logic before and/or after
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
    /// This is the only method that needs to be implemented for PvNugs pipeline behaviors.
    /// The async suffix makes it clear that this method performs asynchronous operations.
    /// </para>
    /// <para>
    /// The mediator will automatically discover and invoke this method when processing requests
    /// that have registered pipeline behaviors.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// public class LoggingPipeline&lt;TRequest, TResponse&gt; 
    ///     : IPvNugsMediatorPipelineRequestHandler&lt;TRequest, TResponse&gt;
    ///     where TRequest : IRequest&lt;TResponse&gt;
    /// {
    ///     public async Task&lt;TResponse&gt; HandleAsync(
    ///         TRequest request, 
    ///         RequestHandlerDelegate&lt;TResponse&gt; next, 
    ///         CancellationToken ct)
    ///     {
    ///         Console.WriteLine("Before");
    ///         var response = await next();
    ///         Console.WriteLine("After");
    ///         return response;
    ///     }
    /// }
    /// </code>
    /// </example>
    Task<TResponse> HandleAsync(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken = default);
}

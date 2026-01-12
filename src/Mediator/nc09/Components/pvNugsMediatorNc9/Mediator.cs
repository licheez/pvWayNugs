using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using pvNugsLoggerNc9Abstractions;
using pvNugsMediatorNc9Abstractions;

namespace pvNugsMediatorNc9;

/// <summary>
/// Provides a concrete implementation of the mediator pattern for routing requests and publishing notifications.
/// </summary>
/// <remarks>
/// <para>
/// This implementation uses reflection and dependency injection to dynamically resolve and invoke
/// handlers for requests and notifications. It supports pipeline behaviors for cross-cutting concerns
/// and provides comprehensive logging of all operations.
/// </para>
/// <para>
/// Handlers and pipeline behaviors are resolved from the <see cref="IServiceProvider"/> at runtime,
/// allowing for flexible configuration and testability.
/// </para>
/// </remarks>
/// <param name="sp">The service provider used to resolve handlers and pipeline behaviors.</param>
/// <param name="logger">The logger service for tracking mediator operations and errors.</param>
public class Mediator(
    IServiceProvider sp,
    ILoggerService logger) : IPvNugsMediator
{
    private const string HandleMethodName = "HandleAsync";
    
    /// <summary>
    /// Sends a request to its registered handler and returns the response, executing any registered pipeline behaviors.
    /// </summary>
    /// <typeparam name="TResponse">The type of response expected from the handler.</typeparam>
    /// <param name="request">The request to be handled.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains the response from the handler.
    /// </returns>
    /// <exception cref="PvNugsMediatorException">
    /// Thrown when no handler is registered for the request type, when the handler doesn't have a HandleAsync method,
    /// or when an exception occurs during handler or pipeline execution.
    /// </exception>
    /// <remarks>
    /// <para>
    /// This method resolves the appropriate handler from the service provider based on the runtime type
    /// of the request. If pipeline behaviors are registered for this request type, they are executed
    /// in reverse registration order (last registered executes first), creating a chain of responsibility
    /// around the actual handler.
    /// </para>
    /// <para>
    /// All operations are logged using the configured logger service for traceability and debugging.
    /// </para>
    /// </remarks>
    public async Task<TResponse> SendAsync<TResponse>(
        IPvNugsMediatorRequest<TResponse> request,
        CancellationToken cancellationToken = default)
    {
        var requestType = request.GetType();
        await logger.LogAsync(
            $"Handling request of type {requestType.FullName}",
            SeverityEnu.Trace);

        var handlerType =
            typeof(IPvNugsMediatorRequestHandler<,>)
                .MakeGenericType(requestType, typeof(TResponse));
        var handler = sp.GetService(handlerType);
        if (handler == null)
        {
            var err = $"No handler registered for request type {requestType.FullName}";
            await logger.LogAsync(err, SeverityEnu.Error);
            throw new PvNugsMediatorException(err);
        }

        var handleMethod = handlerType.GetMethod(HandleMethodName);
        if (handleMethod == null)
        {
            var err = $"Handler for request type {requestType.FullName} " +
                      $"does not have a '{HandleMethodName}' method";
            await logger.LogAsync(err, SeverityEnu.Error);
            throw new PvNugsMediatorException(err);
        }

        // Final delegate that will call the real handler and return Task<TResponse>
        var handlerDelegate = () =>
            (Task<TResponse>)handleMethod.Invoke(handler, [request, cancellationToken])!;

        var pipelineType =
            typeof(IPvNugsPipelineMediator<,>)
                .MakeGenericType(requestType, typeof(TResponse));
        var pipelines = sp.GetServices(pipelineType).ToArray();
        var nbPipelines = pipelines.Length;

        // Get the Handle method once outside the loop
        var handleMethodPipeline = nbPipelines > 0 
            ? pipelineType.GetMethod(HandleMethodName) 
            : null;
        if (nbPipelines > 0 && handleMethodPipeline == null)
        {
            var err = $"Pipeline for request type {requestType.FullName} " +
                      $"does not have a '{HandleMethodName}' method";
            await logger.LogAsync(err, SeverityEnu.Error);
            throw new PvNugsMediatorException(err);
        }

        // Build pipeline chain from last to first
        for (var i = nbPipelines - 1; i >= 0; i--)
        {
            var pipeline = pipelines[i];
            var next = handlerDelegate; // capture previous

            // Create a wrapper that returns Task<TResponse>
            // RequestHandlerDelegate<TResponse> signature requires Task<TResponse>
            var wrapper = () => next();

            // Create delegate instance of the RequestHandlerDelegate<TResponse> runtime type
            var requestHandlerDelegateType = typeof(RequestHandlerDelegate<>).MakeGenericType(typeof(TResponse));
            var nextDelegate = Delegate.CreateDelegate(requestHandlerDelegateType, wrapper.Target, wrapper.Method);

            // Create a new handlerDelegate that calls the pipeline's Handle method and returns Task<TResponse>
            handlerDelegate = () =>
                (Task<TResponse>)handleMethodPipeline!.Invoke(pipeline, [request, nextDelegate, cancellationToken])!;
        }

        try
        {
            var result = await handlerDelegate();
            return result;
        }
        catch (TargetInvocationException tie)
        {
            await logger.LogAsync(tie, SeverityEnu.Error);
            throw new PvNugsMediatorException(tie.InnerException ?? tie);
        }
    }

    /// <summary>
    /// Publishes a notification to all registered handlers of the specified notification type.
    /// </summary>
    /// <typeparam name="TNotification">The type of notification being published.</typeparam>
    /// <param name="notification">The notification to publish.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous publish operation.</returns>
    /// <exception cref="PvNugsMediatorException">
    /// Thrown when a handler doesn't have a HandleAsync method or when an exception occurs during handler execution.
    /// </exception>
    /// <remarks>
    /// This method resolves all handlers registered for the notification type and invokes them sequentially.
    /// If no handlers are registered, a warning is logged and the method returns without error.
    /// </remarks>
    public async Task PublishAsync<TNotification>(
        IPvNugsMediatorNotification notification,
        CancellationToken cancellationToken = default)
        where TNotification : IPvNugsMediatorNotification
    {
        await PublishAsyncInternal(notification, cancellationToken);
    }

    /// <summary>
    /// Publishes a notification object to all registered handlers based on the runtime type of the notification.
    /// </summary>
    /// <param name="notification">The notification object to publish.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous publish operation.</returns>
    /// <exception cref="PvNugsMediatorException">
    /// Thrown when a handler doesn't have a HandleAsync method or when an exception occurs during handler execution.
    /// </exception>
    /// <remarks>
    /// This non-generic overload uses the runtime type of the notification to resolve handlers.
    /// It is useful when the notification type is only known at runtime or when working with polymorphic notification hierarchies.
    /// </remarks>
    public async Task PublishAsync(
        object notification,
        CancellationToken cancellationToken = default)
    {
        await PublishAsyncInternal(notification, cancellationToken);
    }
    
    /// <summary>
    /// Internal method that handles the actual notification publishing logic for both generic and non-generic overloads.
    /// </summary>
    /// <param name="notification">The notification object to publish.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous publish operation.</returns>
    /// <exception cref="PvNugsMediatorException">
    /// Thrown when a handler doesn't have a HandleAsync method or when an exception occurs during handler execution.
    /// </exception>
    /// <remarks>
    /// <para>
    /// This method uses reflection to determine the notification type and resolve all registered handlers.
    /// Handlers are invoked sequentially in the order they were registered. If any handler throws an exception,
    /// it is wrapped in a <see cref="PvNugsMediatorException"/> and propagated to the caller.
    /// </para>
    /// <para>
    /// All operations are logged, including when no handlers are found (logged as a warning).
    /// </para>
    /// </remarks>
    private async Task PublishAsyncInternal(
        object notification,
        CancellationToken cancellationToken = default)
    {
        var notificationType = notification.GetType();
        await logger.LogAsync(
            $"Handling notification of type {notificationType.FullName}",
            SeverityEnu.Trace);
        
        var handlerType = typeof(IPvNugsMediatorNotificationHandler<>)
            .MakeGenericType(notificationType);
        var handlers = sp.GetServices(handlerType).ToArray();
        
        if (handlers.Length == 0)
        {
            await logger.LogAsync(
                $"No handlers registered for notification type {notificationType.FullName}",
                SeverityEnu.Warning);
            return;
        }

        var handleMethod = handlerType.GetMethod(HandleMethodName);
        if (handleMethod == null)
        {
            var err = $"Handler for notification type {notificationType.FullName} " +
                      $"does not have a '{HandleMethodName}' method";
            await logger.LogAsync(err, SeverityEnu.Error);
            throw new PvNugsMediatorException(err);
        }

        foreach (var handler in handlers)
        {
            try
            {
                await (Task)handleMethod.Invoke(handler, [notification, cancellationToken])!;
            }
            catch (TargetInvocationException tie)
            {
                await logger.LogAsync(tie, SeverityEnu.Error);
                throw new PvNugsMediatorException(tie.InnerException ?? tie);
            }
            catch (Exception e)
            {
                await logger.LogAsync(e, SeverityEnu.Error);
                throw new PvNugsMediatorException(e);
            }
        }   
    }
}
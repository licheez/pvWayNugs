using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using pvNugsLoggerNc9Abstractions;
using pvNugsMediatorNc9Abstractions;
using pvNugsMediatorNc9Abstractions.Mediator;
using pvNugsMediatorNc9Abstractions.pvNugs;

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
/// <param name="serviceCollection">The service collection used for handler introspection.</param>
/// <param name="logger">The logger service for tracking mediator operations and errors.</param>
public class Mediator(
    IServiceProvider sp,
    IServiceCollection serviceCollection,
    ILoggerService logger) : IPvNugsMediator
{
    private const string HandleMethodName = "Handle";
    
    private void ResolveHandlerType(Type requestType, Type responseType,
        out object handler, out MethodInfo handleMethod)
    {
        var baseHandlerType =
            typeof(IRequestHandler<,>)
                .MakeGenericType(requestType, responseType);
        var svc = sp.GetService(baseHandlerType);
        if (svc == null)
        {
            var pvNugsHandlerType =
                typeof(IPvNugsMediatorRequestHandler<,>)
                    .MakeGenericType(requestType, responseType);
            svc = sp.GetService(pvNugsHandlerType);
        }

        if (svc == null)
        {
            var sErr = $"No handler registered for request type {requestType.FullName}";
            logger.Log(sErr, SeverityEnu.Error);
            throw new PvNugsMediatorException(sErr);
        }
        
        var handlerType = svc.GetType();
        var method = handlerType.GetMethod(HandleMethodName);
        if (method != null)
        {
            handleMethod = method;
            handler = svc;
            return;
        }
        
        var mErr = $"Handler for request type {requestType.FullName} " +
                  $"does not have a '{HandleMethodName}' method";
        logger.Log(mErr, SeverityEnu.Error);
        throw new PvNugsMediatorException(mErr);
    }

    private IEnumerable<object?> ResolvePipelines(Type requestType, Type responseType)
    {
        var basePipelineType = 
            typeof(IPipelineBehavior<,>)
                .MakeGenericType(requestType, responseType);
        var basePipes = sp.GetServices(basePipelineType).ToList();
        
        var pvNugsPipelineType =
            typeof(IPvNugsMediatorPipelineRequestHandler<,>)
                .MakeGenericType(requestType, responseType);
        var pvPipes = sp.GetServices(pvNugsPipelineType).ToList();

        return basePipes.Union(pvPipes);
    }
    
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
    /// Thrown when no handler is registered for the request type, when the handler doesn't have a Handle method,
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
    public async Task<TResponse> Send<TResponse>(
        IRequest<TResponse> request,
        CancellationToken cancellationToken = default)
    {
        var requestType = request.GetType();
        await logger.LogAsync(
            $"Handling request of type {requestType.FullName}",
            SeverityEnu.Trace);
        
        ResolveHandlerType(requestType, typeof(TResponse), 
            out var handler, out var handleMethod);

        // Final delegate that will call the real handler and return Task<TResponse>
        var handlerDelegate = () =>
            (Task<TResponse>)handleMethod.Invoke(handler, [request, cancellationToken])!;
        
        var pipelines = 
            ResolvePipelines(requestType, typeof(TResponse)).ToList();
        
        var nbPipelines = pipelines.Count;

        // Build pipeline chain from last to first
        for (var i = nbPipelines - 1; i >= 0; i--)
        {
            var pipeline = pipelines[i];
            var pipelineType = pipeline!.GetType();
            
            // Get the Handle method from this specific pipeline's type
            var handleMethodPipeline = pipelineType.GetMethod(HandleMethodName);
            if (handleMethodPipeline == null)
            {
                var err = $"Pipeline {pipelineType.FullName} for request type {requestType.FullName} " +
                          $"does not have a '{HandleMethodName}' method";
                await logger.LogAsync(err, SeverityEnu.Error);
                throw new PvNugsMediatorException(err);
            }
            
            var next = handlerDelegate; // capture previous

            // Create delegate instance of the RequestHandlerDelegate<TResponse> runtime type
            var requestHandlerDelegateType = typeof(RequestHandlerDelegate<>).MakeGenericType(typeof(TResponse));
            
            // Create a wrapper function with explicit type
            var wrapper = () => next();
            var nextDelegate = Delegate.CreateDelegate(requestHandlerDelegateType, wrapper.Target, wrapper.Method);

            // Capture the pipeline and method for this iteration
            var currentPipeline = pipeline;
            var currentMethod = handleMethodPipeline;
            
            // Create a new handlerDelegate that calls the pipeline's Handle method and returns Task<TResponse>
            handlerDelegate = () =>
                (Task<TResponse>)currentMethod.Invoke(currentPipeline, [request, nextDelegate, cancellationToken])!;
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
    /// <inheritdoc cref="Send{TResponse}(IRequest{TResponse}, CancellationToken)"/>
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <typeparam name="TResponse"></typeparam>
    /// <returns></returns>
    public async Task<TResponse> SendAsync<TResponse>(
        IRequest<TResponse> request,
        CancellationToken cancellationToken = default) =>
        await Send(request, cancellationToken);

    /// <summary>
    /// Publishes a notification to all registered handlers of the specified notification type.
    /// </summary>
    /// <typeparam name="TNotification">The type of notification being published.</typeparam>
    /// <param name="notification">The notification to publish.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous publish operation.</returns>
    /// <exception cref="PvNugsMediatorException">
    /// Thrown when a handler doesn't have a Handle method or when an exception occurs during handler execution.
    /// </exception>
    /// <remarks>
    /// This method resolves all handlers registered for the notification type and invokes them sequentially.
    /// If no handlers are registered, a warning is logged and the method returns without error.
    /// </remarks>
    public async Task Publish<TNotification>(
        TNotification notification,
        CancellationToken cancellationToken = default)
        where TNotification : INotification
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
    /// Thrown when a handler doesn't have a Handle method or when an exception occurs during handler execution.
    /// </exception>
    /// <remarks>
    /// This non-generic overload uses the runtime type of the notification to resolve handlers.
    /// It is useful when the notification type is only known at runtime or when working with polymorphic notification hierarchies.
    /// </remarks>
    public async Task Publish(
        object notification,
        CancellationToken cancellationToken = default)
    {
        await PublishAsyncInternal(notification, cancellationToken);
    }
    
    /// <summary>
    /// <inheritdoc cref="Publish(object, CancellationToken)"/>
    /// </summary>
    /// <param name="notification"></param>
    /// <param name="cancellationToken"></param>
    /// <typeparam name="TNotification"></typeparam>
    public async Task PublishAsync<TNotification>(
        TNotification notification,
        CancellationToken cancellationToken = default)
        where TNotification : INotification =>
        await PublishAsyncInternal(notification, cancellationToken);

    /// <summary>
    /// <inheritdoc cref="Publish(object, CancellationToken)"/>
    /// </summary>
    /// <param name="notification"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public Task PublishAsync(
        object notification, CancellationToken cancellationToken = default)
        => PublishAsyncInternal(notification, cancellationToken);

    /// <summary>
    /// Gets information about all registered mediator components (handlers and pipeline behaviors).
    /// </summary>
    /// <returns>
    /// An enumerable of <see cref="MediatorRegistrationInfo"/> containing details about each registered component.
    /// </returns>
    /// <remarks>
    /// <para>
    /// This method inspects the service provider's service descriptors to find all registered:
    /// - Request handlers (IPvNugsMediatorRequestHandler)
    /// - Notification handlers (IPvNugsMediatorNotificationHandler)
    /// - Pipeline behaviors (IPvNugsMediatorPipelineRequestHandler)
    /// </para>
    /// <para>
    /// This is primarily intended for diagnostics, health checks, and development-time validation
    /// to ensure all expected handlers are properly registered.
    /// </para>
    /// </remarks>
    public IEnumerable<MediatorRegistrationInfo> GetRegisteredHandlers()
    {
        var results = new List<MediatorRegistrationInfo>();
        
        foreach (var descriptor in serviceCollection)
        {
            if (!IsValidDescriptor(descriptor))
                continue;

            var info = TryCreateRegistrationInfo(descriptor);
            if (info != null)
            {
                results.Add(info);
            }
        }

        return results.OrderBy(r => r.RegistrationType)
            .ThenBy(r => r.MessageType?.Name)
            .ThenBy(r => r.ImplementationType.Name);
    }

    /// <summary>
    /// Validates whether a service descriptor is eligible for handler registration inspection.
    /// </summary>
    /// <param name="descriptor">The service descriptor to validate.</param>
    /// <returns>True if the descriptor is generic and has a concrete implementation type; otherwise, false.</returns>
    private static bool IsValidDescriptor(ServiceDescriptor descriptor)
    {
        return descriptor.ServiceType.IsGenericType && 
               descriptor.ImplementationType != null;
    }

    /// <summary>
    /// Attempts to create a MediatorRegistrationInfo from a service descriptor.
    /// </summary>
    /// <param name="descriptor">The service descriptor to inspect.</param>
    /// <returns>A MediatorRegistrationInfo if the descriptor represents a known handler type; otherwise, null.</returns>
    private static MediatorRegistrationInfo? TryCreateRegistrationInfo(ServiceDescriptor descriptor)
    {
        var serviceType = descriptor.ServiceType;
        var genericTypeDefinition = serviceType.GetGenericTypeDefinition();
        var implementationType = descriptor.ImplementationType!;
        var lifetime = descriptor.Lifetime.ToString();

        return TryCreateRequestHandlerInfo(serviceType, genericTypeDefinition, implementationType, lifetime)
               ?? TryCreateBaseRequestHandlerInfo(serviceType, genericTypeDefinition, implementationType, lifetime)
               ?? TryCreateUnitRequestHandlerInfo(serviceType, genericTypeDefinition, implementationType, lifetime)
               ?? TryCreateNotificationHandlerInfo(serviceType, genericTypeDefinition, implementationType, lifetime)
               ?? TryCreateBaseNotificationHandlerInfo(serviceType, genericTypeDefinition, implementationType, lifetime)
               ?? TryCreatePipelineHandlerInfo(serviceType, genericTypeDefinition, implementationType, lifetime)
               ?? TryCreateBasePipelineHandlerInfo(serviceType, genericTypeDefinition, implementationType, lifetime);
    }

    /// <summary>
    /// Attempts to create registration info for a two-parameter request handler.
    /// </summary>
    private static MediatorRegistrationInfo? TryCreateRequestHandlerInfo(
        Type serviceType, Type genericTypeDefinition, Type implementationType, string lifetime)
    {
        if (genericTypeDefinition != typeof(IPvNugsMediatorRequestHandler<,>))
            return null;

        var genericArgs = serviceType.GetGenericArguments();
        return new MediatorRegistrationInfo
        {
            RegistrationType = "Request Handler",
            ServiceType = serviceType,
            ImplementationType = implementationType,
            Lifetime = lifetime,
            MessageType = genericArgs[0],
            ResponseType = genericArgs[1]
        };
    }

    /// <summary>
    /// Attempts to create registration info for a base IRequestHandler.
    /// </summary>
    private static MediatorRegistrationInfo? TryCreateBaseRequestHandlerInfo(
        Type serviceType, Type genericTypeDefinition, Type implementationType, string lifetime)
    {
        if (genericTypeDefinition != typeof(IRequestHandler<,>))
            return null;

        var genericArgs = serviceType.GetGenericArguments();
        return new MediatorRegistrationInfo
        {
            RegistrationType = "Request Handler",
            ServiceType = serviceType,
            ImplementationType = implementationType,
            Lifetime = lifetime,
            MessageType = genericArgs[0],
            ResponseType = genericArgs[1]
        };
    }

    /// <summary>
    /// Attempts to create registration info for a Unit (void) request handler.
    /// </summary>
    private static MediatorRegistrationInfo? TryCreateUnitRequestHandlerInfo(
        Type serviceType, Type genericTypeDefinition, Type implementationType, string lifetime)
    {
        if (genericTypeDefinition != typeof(IRequestHandler<>))
            return null;

        var requestType = serviceType.GetGenericArguments()[0];
        var twoParamHandler = typeof(IPvNugsMediatorRequestHandler<,>)
            .MakeGenericType(requestType, typeof(Unit));

        if (implementationType.GetInterfaces().All(i => i != twoParamHandler))
            return null;

        return new MediatorRegistrationInfo
        {
            RegistrationType = "Request Handler",
            ServiceType = serviceType,
            ImplementationType = implementationType,
            Lifetime = lifetime,
            MessageType = requestType,
            ResponseType = typeof(Unit)
        };
    }

    /// <summary>
    /// Attempts to create registration info for a notification handler.
    /// </summary>
    private static MediatorRegistrationInfo? TryCreateNotificationHandlerInfo(
        Type serviceType, Type genericTypeDefinition, Type implementationType, string lifetime)
    {
        if (genericTypeDefinition != typeof(IPvNugsMediatorNotificationHandler<>))
            return null;

        return new MediatorRegistrationInfo
        {
            RegistrationType = "Notification Handler",
            ServiceType = serviceType,
            ImplementationType = implementationType,
            Lifetime = lifetime,
            MessageType = serviceType.GetGenericArguments()[0],
            ResponseType = null
        };
    }

    /// <summary>
    /// Attempts to create registration info for a base INotificationHandler.
    /// </summary>
    private static MediatorRegistrationInfo? TryCreateBaseNotificationHandlerInfo(
        Type serviceType, Type genericTypeDefinition, Type implementationType, string lifetime)
    {
        if (genericTypeDefinition != typeof(INotificationHandler<>))
            return null;

        return new MediatorRegistrationInfo
        {
            RegistrationType = "Notification Handler",
            ServiceType = serviceType,
            ImplementationType = implementationType,
            Lifetime = lifetime,
            MessageType = serviceType.GetGenericArguments()[0],
            ResponseType = null
        };
    }

    /// <summary>
    /// Attempts to create registration info for a PvNugs pipeline behavior.
    /// </summary>
    private static MediatorRegistrationInfo? TryCreatePipelineHandlerInfo(
        Type serviceType, Type genericTypeDefinition, Type implementationType, string lifetime)
    {
        if (genericTypeDefinition != typeof(IPvNugsMediatorPipelineRequestHandler<,>))
            return null;

        var genericArgs = serviceType.GetGenericArguments();
        return new MediatorRegistrationInfo
        {
            RegistrationType = "Pipeline Behavior",
            ServiceType = serviceType,
            ImplementationType = implementationType,
            Lifetime = lifetime,
            MessageType = genericArgs[0],
            ResponseType = genericArgs[1]
        };
    }

    /// <summary>
    /// Attempts to create registration info for a base interface pipeline behavior.
    /// </summary>
    private static MediatorRegistrationInfo? TryCreateBasePipelineHandlerInfo(
        Type serviceType, Type genericTypeDefinition, Type implementationType, string lifetime)
    {
        if (genericTypeDefinition != typeof(IPipelineBehavior<,>))
            return null;

        var genericArgs = serviceType.GetGenericArguments();
        return new MediatorRegistrationInfo
        {
            RegistrationType = "Pipeline Behavior",
            ServiceType = serviceType,
            ImplementationType = implementationType,
            Lifetime = lifetime,
            MessageType = genericArgs[0],
            ResponseType = genericArgs[1]
        };
    }

    /// <summary>
    /// Internal method that handles the actual notification publishing logic for both generic and non-generic overloads.
    /// </summary>
    /// <param name="notification">The notification object to publish.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous publish operation.</returns>
    /// <exception cref="PvNugsMediatorException">
    /// Thrown when a handler doesn't have a Handle method or when an exception occurs during handler execution.
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

        // Try both base and PvNugs notification handler types
        var baseHandlerType = typeof(INotificationHandler<>)
            .MakeGenericType(notificationType);
        var baseHandlers = sp.GetServices(baseHandlerType).ToList();
        
        var pvNugsHandlerType = typeof(IPvNugsMediatorNotificationHandler<>)
            .MakeGenericType(notificationType);
        var pvNugsHandlers = sp.GetServices(pvNugsHandlerType).ToList();
        
        var handlers = baseHandlers
            .Union(pvNugsHandlers).ToArray();

        if (handlers.Length == 0)
        {
            await logger.LogAsync(
                $"No handlers registered for notification type {notificationType.FullName}",
                SeverityEnu.Warning);
            return;
        }

        foreach (var handler in handlers)
        {
            var handlerType = handler!.GetType();
            var handleMethod = handlerType.GetMethod(HandleMethodName);
            
            if (handleMethod == null)
            {
                var err = $"Handler {handlerType.FullName} for notification type {notificationType.FullName} " +
                          $"does not have a '{HandleMethodName}' method";
                await logger.LogAsync(err, SeverityEnu.Error);
                throw new PvNugsMediatorException(err);
            }

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
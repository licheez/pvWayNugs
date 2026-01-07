using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using pvNugsLoggerNc9Abstractions;
using pvNugsMediatorNc9Abstractions;

namespace pvNugsMediatorNc9;

public class Mediator(
    IServiceProvider sp,
    IConsoleLoggerService logger) : IPvNugsMediator
{
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

        var handleMethod = handlerType.GetMethod("Handle");
        if (handleMethod == null)
        {
            var err = $"Handler for request type {requestType.FullName} does not have a Handle method";
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
        var handleMethodPipeline = nbPipelines > 0 ? pipelineType.GetMethod("Handle") : null;
        if (nbPipelines > 0 && handleMethodPipeline == null)
        {
            var err = $"Pipeline for request type {requestType.FullName} does not have a Handle method";
            await logger.LogAsync(err, SeverityEnu.Error);
            throw new PvNugsMediatorException(err);
        }

        // Build pipeline chain from last to first
        for (var i = nbPipelines - 1; i >= 0; i--)
        {
            var pipeline = pipelines[i];
            var next = handlerDelegate; // capture previous

            // Create a wrapper that returns Task (upcast from Task<TResponse>)
            // RequestHandlerDelegate<TResponse> returns Task, but it's actually the Task<TResponse> upcast to Task
            // This preserves the TResponse result in the returned Task
            Func<Task> wrapper = () => next();

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

    public Task PublishAsync<TNotification>(
        IPvNugsMediatorNotification notification,
        CancellationToken cancellationToken = default)
        where TNotification : IPvNugsMediatorNotification
    {
        throw new NotImplementedException();
    }

    public Task PublishAsync(
        object notification,
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
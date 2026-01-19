using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using pvNugsMediatorNc9Abstractions;

namespace pvNugsMediatorNc9;

/// <summary>
/// Provides dependency injection extension methods for registering the mediator service.
/// </summary>
public static class PvNugsMediatorDi
{
    /// <summary>
    /// Registers the <see cref="pvNugsMediatorNc9Abstractions.IMediator"/> implementation as a singleton service if not already registered.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add the service to.</param>
    /// <returns>The same service collection so that multiple calls can be chained.</returns>
    /// <remarks>
    /// <para>
    /// This method uses <see cref="ServiceCollectionDescriptorExtensions.TryAddSingleton{TService, TImplementation}"/>
    /// to ensure the mediator is only registered once, even if this method is called multiple times.
    /// The mediator is registered as a singleton for optimal performance.
    /// </para>
    /// <para>
    /// Before using the mediator, ensure that:
    /// </para>
    /// <list type="bullet">
    /// <item><description>A logger service implementing <c>ILoggerService</c> is registered (required dependency)</description></item>
    /// <item><description>Request handlers are registered for each request type you want to handle</description></item>
    /// <item><description>Notification handlers are registered for each notification type you want to handle</description></item>
    /// <item><description>Pipeline behaviors are registered (optional, for cross-cutting concerns)</description></item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// var services = new ServiceCollection();
    /// 
    /// // Register logger (required)
    /// services.TryAddPvNugsLoggerSeriService(config);
    /// 
    /// // Register handlers
    /// services.AddTransient&lt;IPvNugsMediatorRequestHandler&lt;GetUserRequest, User&gt;, GetUserHandler&gt;();
    /// 
    /// // Register mediator
    /// services.TryAddPvNugsMediator();
    /// 
    /// var sp = services.BuildServiceProvider();
    /// var mediator = sp.GetRequiredService&lt;IPvNugsMediator&gt;();
    /// </code>
    /// </example>
    public static IServiceCollection TryAddPvNugsMediator(this IServiceCollection services)
    {
        services.TryAddSingleton<IPvNugsMediator, Mediator>();
        return services;
    }
}
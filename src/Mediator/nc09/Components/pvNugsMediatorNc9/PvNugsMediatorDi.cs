using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using pvNugsMediatorNc9Abstractions;
using pvNugsMediatorNc9Abstractions.Mediator;
using pvNugsMediatorNc9Abstractions.pvNugs;

namespace pvNugsMediatorNc9;

/// <summary>
/// Provides dependency injection extension methods for registering the mediator service.
/// </summary>
public static class PvNugsMediatorDi
{
    /// <summary>
    /// Registers the <see cref="IPvNugsMediator"/> implementation with specified discovery mode.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add the service to.</param>
    /// <param name="configuration">
    /// Optional configuration section to bind mediator settings from. 
    /// If null, default settings are used (<see cref="DiscoveryMode.Manual"/> and <see cref="pvNugsMediatorNc9Abstractions.ServiceLifetime.Transient"/>).
    /// </param>
    /// <returns>The same service collection so that multiple calls can be chained.</returns>
    /// <remarks>
    /// <para>
    /// This method registers the mediator as a singleton service and configures how handlers
    /// are discovered based on the specified <see cref="DiscoveryMode"/>:
    /// </para>
    /// <list type="bullet">
    /// <item><description><b>Manual</b>: Handlers must be explicitly registered (best for production)</description></item>
    /// <item><description><b>Decorated</b>: Handlers marked with <see cref="MediatorHandlerAttribute"/> are auto-discovered</description></item>
    /// <item><description><b>FullScan</b>: All handler implementations are auto-discovered via reflection</description></item>
    /// </list>
    /// <para>
    /// Before using the mediator, ensure that a logger service implementing <c>ILoggerService</c> is registered.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Using IConfiguration (from appsettings.json)
    /// // appsettings.json:
    /// // {
    /// //   "PvNugsMediatorConfig": {
    /// //     "DiscoveryMode": "FullScan",
    /// //     "ServiceLifetime": "Scoped"
    /// //   }
    /// // }
    /// services.TryAddPvNugsMediator(configuration.GetSection(PvNugsMediatorConfig.Section));
    /// 
    /// // Or with explicit discovery mode
    /// services.TryAddPvNugsMediator(DiscoveryMode.FullScan);
    /// 
    /// // Or with defaults (Manual mode)
    /// services.TryAddPvNugsMediator();
    /// services.AddTransient&lt;IPvNugsMediatorRequestHandler&lt;GetUserRequest, User&gt;, GetUserHandler&gt;();
    /// </code>
    /// </example>
    public static IServiceCollection TryAddPvNugsMediator(
        this IServiceCollection services,
        IConfiguration? configuration = null)
    {
        var config = new PvNugsMediatorConfig();
        
        // Bind configuration if provided - get the specific section first
        if (configuration != null)
        {
            var section = configuration
                .GetSection(PvNugsMediatorConfig.Section);
            section.Bind(config);
        }

        // Register the configuration
        services.Configure<PvNugsMediatorConfig>(options =>
        {
            options.DiscoveryMode = config.DiscoveryMode;
            options.ServiceLifetime = config.ServiceLifetime;
        });
        
        // Perform handler discovery based on mode
        PerformHandlerDiscovery(services, config);
        
        // Register the service collection itself so Mediator can access it for introspection
        services.TryAddSingleton(services);
        
        services.TryAddSingleton<IPvNugsMediator, Mediator>();
        return services;
    }

    /// <summary>
    /// Registers the <see cref="IPvNugsMediator"/> implementation with specified discovery mode.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add the service to.</param>
    /// <param name="discoveryMode">The discovery mode to use for finding handlers.</param>
    /// <returns>The same service collection so that multiple calls can be chained.</returns>
    /// <remarks>
    /// <para>
    /// This is a convenience overload that sets only the discovery mode with default <see cref="pvNugsMediatorNc9Abstractions.ServiceLifetime.Transient"/>.
    /// Use the overload with <see cref="IConfiguration"/> for more control via configuration files.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Quick setup for development with FullScan
    /// services.TryAddPvNugsMediator(DiscoveryMode.FullScan);
    /// 
    /// // Production with Manual mode
    /// services.TryAddPvNugsMediator(DiscoveryMode.Manual);
    /// services.AddTransient&lt;IPvNugsMediatorRequestHandler&lt;GetUserRequest, User&gt;, GetUserHandler&gt;();
    /// </code>
    /// </example>
    public static IServiceCollection TryAddPvNugsMediator(
        this IServiceCollection services,
        DiscoveryMode discoveryMode)
    {
        var config = new PvNugsMediatorConfig
        {
            DiscoveryMode = discoveryMode
        };
        
        // Register the configuration
        services.Configure<PvNugsMediatorConfig>(options =>
        {
            options.DiscoveryMode = config.DiscoveryMode;
            options.ServiceLifetime = config.ServiceLifetime;
        });
        
        PerformHandlerDiscovery(services, config);
        
        // Register the service collection itself so Mediator can access it for introspection
        services.TryAddSingleton(services);
        
        services.TryAddSingleton<IPvNugsMediator, Mediator>();
        return services;
    }

    /// <summary>
    /// Performs handler discovery based on the configured discovery mode.
    /// </summary>
    /// <param name="services">The service collection to register handlers into.</param>
    /// <param name="config">The mediator configuration containing discovery mode and lifetime settings.</param>
    private static void PerformHandlerDiscovery(
        IServiceCollection services,
        PvNugsMediatorConfig config)
    {
        switch (config.DiscoveryMode)
        {
            case DiscoveryMode.Manual:
                // In manual mode, we assume all handlers are registered manually.
                break;
            case DiscoveryMode.Decorated:
            case DiscoveryMode.FullScan:
                // Scan all loaded assemblies for handler implementations
                RegisterHandlers(services, config.DiscoveryMode, config.ServiceLifetime);
                break;
            default:
                throw new SwitchExpressionException(config.DiscoveryMode);
        }
    }

    /// <summary>
    /// Scans all loaded assemblies and automatically registers all handler implementations.
    /// </summary>
    /// <param name="services">The service collection to register handlers into.</param>
    /// <param name="discoveryMode">The discovery mode to use for finding handlers.</param>
    /// <param name="defaultLifetime">The default service lifetime to use for discovered handlers.</param>
    private static void RegisterHandlers(
        IServiceCollection services,
        DiscoveryMode discoveryMode,
        pvNugsMediatorNc9Abstractions.ServiceLifetime defaultLifetime)
    {
        var assemblies = AppDomain.CurrentDomain.GetAssemblies()
            .Where(a => !a.IsDynamic && !IsSystemAssembly(a))
            .ToList();
        var fullScan = discoveryMode == DiscoveryMode.FullScan;
        
        foreach (var assembly in assemblies)
        {
            try
            {
                var types = assembly.GetExportedTypes()
                    .Where(t => t is 
                    { 
                        IsClass: true, 
                        IsAbstract: false, 
                        IsGenericTypeDefinition: false 
                    })
                    .Where(t =>
                        fullScan ||
                        t.GetCustomAttribute<MediatorHandlerAttribute>() != null)
                    .ToList();
                foreach (var type in types)
                {
                    var attribute = type.GetCustomAttribute<MediatorHandlerAttribute>();
                    var effectiveLifetime = attribute?.Lifetime ?? defaultLifetime;
                    RegisterHandlerIfApplicable(
                        services, type, effectiveLifetime);
                }
            }
            catch
            {
                // Skip assemblies that can't be scanned
            }
        }
        
    }

    /// <summary>
    /// Registers a type as a handler if it implements any of the mediator handler interfaces.
    /// </summary>
    /// <param name="services">The service collection to register into.</param>
    /// <param name="implementationType">The concrete type to check and register.</param>
    /// <param name="lifetime">The service lifetime to use for registration.</param>
    private static void RegisterHandlerIfApplicable(
        IServiceCollection services,
        Type implementationType,
        pvNugsMediatorNc9Abstractions.ServiceLifetime lifetime)
    {
        var interfaces = implementationType.GetInterfaces();

        foreach (var @interface in interfaces)
        {
            if (!@interface.IsGenericType)
                continue;

            var genericTypeDefinition = @interface.GetGenericTypeDefinition();

            // Check for Request Handlers (with response)
            if (genericTypeDefinition == typeof(IPvNugsMediatorRequestHandler<,>))
            {
                RegisterService(services, @interface, implementationType, lifetime);
            }
            // Check for Request Handlers (without response - Unit)
            else if (genericTypeDefinition == typeof(IRequestHandler<>))
            {
                // Only register if it's a PvNugs handler (implements the 2-param version)
                var requestType = @interface.GetGenericArguments()[0];
                var twoParamInterface = typeof(IPvNugsMediatorRequestHandler<,>)
                    .MakeGenericType(requestType, typeof(Unit));
                
                if (interfaces.Any(i => i == twoParamInterface))
                {
                    RegisterService(services, @interface, implementationType, lifetime);
                }
            }
            // Check for Notification Handlers
            else if (genericTypeDefinition == typeof(IPvNugsMediatorNotificationHandler<>))
            {
                RegisterService(services, @interface, implementationType, lifetime);
            }
            // Check for Pipeline Behaviors
            else if (genericTypeDefinition == typeof(IPvNugsMediatorPipelineRequestHandler<,>))
            {
                RegisterService(services, @interface, implementationType, lifetime);
            }
            // Also check base interface versions for compatibility
            else if (genericTypeDefinition == typeof(IRequestHandler<,>))
            {
                RegisterService(services, @interface, implementationType, lifetime);
            }
            else if (genericTypeDefinition == typeof(INotificationHandler<>))
            {
                RegisterService(services, @interface, implementationType, lifetime);
            }
            else if (genericTypeDefinition == typeof(IPipelineBehavior<,>))
            {
                RegisterService(services, @interface, implementationType, lifetime);
            }
        }
    }

    /// <summary>
    /// Registers a service with the specified lifetime.
    /// </summary>
    private static void RegisterService(
        IServiceCollection services,
        Type serviceType,
        Type implementationType,
        pvNugsMediatorNc9Abstractions.ServiceLifetime lifetime)
    {
        var msLifetime = ConvertToMicrosoftServiceLifetime(lifetime);
        var descriptor = new ServiceDescriptor(serviceType, implementationType, msLifetime);
        
        // Use TryAdd to avoid duplicate registrations
        if (!services.Any(s => s.ServiceType == serviceType && s.ImplementationType == implementationType))
        {
            services.Add(descriptor);
        }
    }
    
    /// <summary>
    /// Converts the custom ServiceLifetime enum to Microsoft's ServiceLifetime enum.
    /// </summary>
    private static Microsoft.Extensions.DependencyInjection.ServiceLifetime ConvertToMicrosoftServiceLifetime(
        pvNugsMediatorNc9Abstractions.ServiceLifetime lifetime)
    {
        return lifetime switch
        {
            pvNugsMediatorNc9Abstractions.ServiceLifetime.Transient => Microsoft.Extensions.DependencyInjection.ServiceLifetime.Transient,
            pvNugsMediatorNc9Abstractions.ServiceLifetime.Scoped => Microsoft.Extensions.DependencyInjection.ServiceLifetime.Scoped,
            pvNugsMediatorNc9Abstractions.ServiceLifetime.Singleton => Microsoft.Extensions.DependencyInjection.ServiceLifetime.Singleton,
            _ => Microsoft.Extensions.DependencyInjection.ServiceLifetime.Transient
        };
    }

    /// <summary>
    /// Checks if an assembly is a system assembly that should be excluded from scanning.
    /// </summary>
    private static bool IsSystemAssembly(Assembly assembly)
    {
        var name = assembly.FullName ?? string.Empty;
        return name.StartsWith("System.") ||
               name.StartsWith("Microsoft.") ||
               name.StartsWith("netstandard") ||
               name.StartsWith("mscorlib");
    }
    
}
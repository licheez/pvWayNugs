using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using pvNugsMediatorNc9Abstractions;

namespace pvNugsMediatorNc9;

public static class PvNugsMediatorDi
{
    public static IServiceCollection TryAddPvNugsMediator(this IServiceCollection services)
    {
        services.TryAddSingleton<IPvNugsMediator, Mediator>();
        return services;
    }
}
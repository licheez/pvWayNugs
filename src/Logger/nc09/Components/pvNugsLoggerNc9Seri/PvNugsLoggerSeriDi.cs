using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using pvNugsLoggerNc9;
using pvNugsLoggerNc9Abstractions;

namespace pvNugsLoggerNc9Seri;

public static class PvNugsLoggerSeriDi
{
    public static IServiceCollection TryAddPvNugsLoggerSeriWriter(this IServiceCollection services)
    {
        services.AddSingleton<ILogWriter, SerilogConsoleWriter>();
        services.TryAddSingleton<IConsoleLogWriter, SerilogConsoleWriter>();
        return services;
    }

    public static IServiceCollection AddPvNugsLoggerSeriService(
        this IServiceCollection services,
        IConfiguration config)
    {
        services.TryAddPvNugsLoggerSeriWriter();
        
        services.Configure<PvNugsLoggerConfig>(
            config.GetSection(PvNugsLoggerConfig.Section));
        
        services.TryAddSingleton<ILoggerService, SerilogConsoleService>();
        services.TryAddSingleton<IConsoleLoggerService, SerilogConsoleService>();
        services.TryAddSingleton<ISeriConsoleLoggerService, SerilogConsoleService>();
        
        return services;
    }
}
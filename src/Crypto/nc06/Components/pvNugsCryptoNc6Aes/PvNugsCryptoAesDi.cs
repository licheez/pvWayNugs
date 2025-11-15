using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using pvNugsCryptoNc6Abstractions;

namespace pvNugsCryptoNc6Aes;

/// <summary>
/// Extension methods to register the AES-based pvNugs crypto implementation
/// and its configuration into an <see cref="IServiceCollection"/>.
/// </summary>
/// <remarks>
/// Registering this component wires up configuration binding for
/// <see cref="PvNugsCryptoAesConfig"/> from the provided <see cref="IConfiguration"/>
/// and registers the AES implementation <see cref="PvNugsCrypto"/> as the
/// service for <see cref="IPvNugsCrypto"/> using a singleton lifetime.
/// The method is idempotent and uses TryAdd semantics for the service
/// registration to avoid overwriting an existing registration.
/// </remarks>
public static class PvNugsCryptoAesDi
{
    /// <summary>
    /// Configure and register the pvNugs AES crypto implementation.
    /// </summary>
    /// <param name="services">The service collection to which the crypto services and configuration will be added.</param>
    /// <param name="config">The application configuration used to bind <see cref="PvNugsCryptoAesConfig"/> (expects a section named <see cref="PvNugsCryptoAesConfig.Section"/>).</param>
    /// <returns>The same <see cref="IServiceCollection"/> instance for chaining.
    /// </returns>
    /// <exception cref="System.ArgumentNullException">Thrown if <paramref name="services"/> or <paramref name="config"/> is <c>null</c>.</exception>
    /// <remarks>
    /// Typical usage in an application's startup code:
    /// <code>
    /// services.TryAddPvNugsCryptoAes(Configuration);
    /// </code>
    /// After calling this method, the AES implementation can be resolved as
    /// <c>IPvNugsCrypto</c> from DI. Keep your AES key and IV in a secure
    /// configuration source (for example environment variables or a secret
    /// manager) and bind them to the configuration section named by
    /// <see cref="PvNugsCryptoAesConfig.Section"/>.
    /// </remarks>
    public static IServiceCollection TryAddPvNugsCryptoAes(
        this IServiceCollection services, IConfiguration config)
    {
        if (services is null) throw new ArgumentNullException(nameof(services));
        if (config is null) throw new ArgumentNullException(nameof(config));

        services.Configure<PvNugsCryptoAesConfig>(
            config.GetSection(PvNugsCryptoAesConfig.Section));
        
        services.TryAddSingleton<IPvNugsCrypto, PvNugsCrypto>();
        return services;
    }
}
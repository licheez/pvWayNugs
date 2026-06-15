using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using pvNugsSecretManagerNc10Abstractions;

namespace pvNugsSecretManagerNc10;

/// <summary>
/// Provides dependency-injection registration helpers for <c>pvNugsSecretManagerNc10</c>.
/// </summary>
/// <remarks>
/// <para>
/// This class exposes extension methods for <see cref="IServiceCollection"/> so applications can
/// register the provider-agnostic secret manager with a single call.
/// </para>
/// <para>
/// The registration is idempotent through <c>TryAdd*</c> semantics, which means existing service
/// registrations are preserved and only missing registrations are added.
/// </para>
/// <para><c>Important:</c> This method does not register a concrete
/// <see cref="IPvNugsSecretProvider"/> implementation, cache implementation, or logger implementation.
/// Those dependencies must be registered separately by the consuming application.
/// </para>
/// </remarks>
public static class PvNugsSecretManagerDi
{
    /// <summary>
    /// Registers the secret manager service and binds <see cref="PvNugsSecretManagerConfig"/>
    /// from configuration.
    /// </summary>
    /// <param name="services">
    /// The DI service collection to update.
    /// </param>
    /// <param name="config">
    /// The application configuration root used to bind the section named
    /// <see cref="PvNugsSecretManagerConfig.Section"/>.
    /// </param>
    /// <returns>
    /// The same <paramref name="services"/> instance for fluent chaining.
    /// </returns>
    /// <remarks>
    /// <para><c>What this method registers:</c></para>
    /// <list type="bullet">
    /// <item><description><see cref="PvNugsSecretManagerConfig"/> via <c>services.Configure</c>.</description></item>
    /// <item><description><see cref="IPvNugsSecretManager"/> as singleton implemented by internal <c>SecretManager</c>.</description></item>
    /// </list>
    /// <para><c>What must already be registered:</c></para>
    /// <list type="bullet">
    /// <item><description>A concrete <see cref="IPvNugsSecretProvider"/> implementation.</description></item>
    /// <item><description>An <c>IPvNugsCache</c> implementation.</description></item>
    /// <item><description>An <c>IConsoleLoggerService</c> implementation.</description></item>
    /// </list>
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="services"/> or <paramref name="config"/> is <see langword="null"/>.
    /// </exception>
    public static IServiceCollection TryAddPvNugsSecretManager(
        this IServiceCollection services, IConfiguration config)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(config);

        services.Configure<PvNugsSecretManagerConfig>(
            config.GetSection(PvNugsSecretManagerConfig.Section));

        services.TryAddSingleton<IPvNugsSecretManager, SecretManager>();

        return services;
    }
}
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using pvNugsSecretManagerNc10Abstractions;

namespace pvNugsSecretManagerNc10ProviderEnvironment;

/// <summary>
/// Provides dependency injection extension methods for registering the Environment Variable secret provider.
/// </summary>
/// <remarks>
/// <para>
/// This class contains extension methods for configuring the Environment Variable secret provider
/// with the .NET dependency injection container. It handles registration of the provider implementation
/// and its configuration options.
/// </para>
/// <para>The provider retrieves secrets from environment variables or any configuration source
/// supported by Microsoft.Extensions.Configuration (appsettings.json, user secrets, command-line args, etc.).</para>
/// </remarks>
public static class PvNugsEnvVarSecretProviderDi
{
    /// <summary>
    /// Registers the Environment Variable secret provider implementation as <see cref="IPvNugsSecretProvider"/>
    /// using <see cref="EnvVarSecretProvider"/>.
    /// </summary>
    /// <param name="services">The service collection to register the provider with.</param>
    /// <param name="config">
    /// The configuration to bind the <see cref="PvNugsEnvVarSecretProviderConfig"/> from.
    /// The method expects a configuration section named <c>PvNugsEnvVarSecretProviderConfig</c>.
    /// </param>
    /// <returns>The service collection for method chaining.</returns>
    /// <remarks>
    /// <para>
    /// This method uses <c>TryAddSingleton</c>, meaning if an <see cref="IPvNugsSecretProvider"/> 
    /// is already registered, this registration will be skipped. This allows for provider switching
    /// or testing scenarios.
    /// </para>
    /// <para>The provider configuration is bound from the configuration section and registered
    /// using the Options pattern for strongly-typed access.</para>
    /// </remarks>
    /// <example>
    /// <para><b>Basic registration in Program.cs:</b></para>
    /// <code>
    /// var builder = WebApplication.CreateBuilder(args);
    /// 
    /// // Register the environment variable secret provider
    /// builder.Services.TryAddPvNugsEnvVarSecretProvider(builder.Configuration);
    /// 
    /// var app = builder.Build();
    /// app.Run();
    /// </code>
    /// 
    /// <para><b>Configuration in appsettings.json:</b></para>
    /// <code>
    /// {
    ///   "PvNugsEnvVarSecretProviderConfig": {
    ///     "Prefix": "MyApp"
    ///   },
    ///   "MyApp": {
    ///     "DatabasePassword": "dev_password_123",
    ///     "ApiKey": "dev_api_key_456"
    ///   }
    /// }
    /// </code>
    /// 
    /// <para><b>Usage in services:</b></para>
    /// <code>
    /// public class MyService
    /// {
    ///     private readonly IPvNugsSecretProvider _secretProvider;
    ///     
    ///     public MyService(IPvNugsSecretProvider secretProvider)
    ///     {
    ///         _secretProvider = secretProvider;
    ///     }
    ///     
    ///     public async Task&lt;string&gt; GetDatabaseConnectionString()
    ///     {
    ///         var parameters = PvNugsEnvVarSecretProviderParameters.CreateParameters("DatabasePassword");
    ///         var password = await _secretProvider.GetStaticSecretAsync(parameters);
    ///         return $"Server=localhost;Database=MyDb;Password={password}";
    ///     }
    /// }
    /// </code>
    /// </example>
    public static IServiceCollection TryAddPvNugsEnvVarSecretProvider(
        this IServiceCollection services,
        IConfiguration config)
    {
        services.Configure<PvNugsEnvVarSecretProviderConfig>(
            config.GetSection(nameof(PvNugsEnvVarSecretProviderConfig)));

        services.TryAddSingleton<IPvNugsSecretProvider, EnvVarSecretProvider>();

        return services;
    }
}
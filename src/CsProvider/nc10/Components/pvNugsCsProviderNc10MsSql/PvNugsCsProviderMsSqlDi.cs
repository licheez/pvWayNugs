using System.Runtime.CompilerServices;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using pvNugsCsProviderNc10Abstractions;
using pvNugsLoggerNc10Abstractions;
using pvNugsSecretManagerNc10Abstractions;

namespace pvNugsCsProviderNc10MsSql;

public static class PvNugsCsProviderMsSqlDi
{
    public static IServiceCollection TryAddPvNugsCsProviderMsSql(
        this IServiceCollection services, IConfiguration config)
    {
        // Configure options with validation
        services.Configure<PvNugsCsProviderMsSqlConfig>(configSection =>
        {
            config.GetSection(PvNugsCsProviderMsSqlConfig.Section)
                .Bind(configSection);
            var configRows = configSection.Rows ?? [];
            foreach (var configRow in configRows)
            {
                ValidateConfiguration(configRow);
            }
        });
        
        // Factory-based registration for mode-specific constructor selection
        services.TryAddSingleton<IPvNugsCsProvider>(serviceProvider =>
        {
            try
            {
                return CreateProvider(serviceProvider);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    "Failed to create Microsoft SQL Server connection string provider. " +
                    "Ensure all required dependencies are registered and configuration is valid.", ex);
            }
        });

        // Register specific interface
        services.TryAddSingleton<IPvNugsMsSqlCsProvider>(serviceProvider =>
            (CsProvider)serviceProvider.GetRequiredService<IPvNugsCsProvider>());
        
        return services;
    }
    
    /// <summary>
    /// Factory method that creates an instance of <see cref="CsProvider"/>
    /// based on the registered dependencies and configuration.
    /// </summary>
    /// <param name="serviceProvider">The service provider used to resolve dependencies.</param>
    /// <returns>An instance of <see cref="CsProvider"/>.</returns>
    /// <exception cref="InvalidOperationException"></exception>
    /// <exception cref="SwitchExpressionException"></exception>
    private static CsProvider CreateProvider(IServiceProvider serviceProvider)
    {
        var logger = serviceProvider.GetRequiredService<IConsoleLoggerService>();
        var options = serviceProvider.GetRequiredService<IOptions<PvNugsCsProviderMsSqlConfig>>();  
        var secretManager = serviceProvider.GetService<IPvNugsSecretManager>();
        var config = options.Value;

        // Mode-specific factory logic
        switch (config.Mode)
        {
            case CsProviderModeEnu.Config:
                return new CsProvider(logger, options);
            
            case CsProviderModeEnu.StaticSecret:
                if (secretManager == null)
                {
                    throw new InvalidOperationException(
                        $"Mode {config.Mode} requires a registered IPvNugsSecretManager. " +
                        "Register it with: services.AddSingleton<IPvNugsSecretManager, YourImplementation>()");
                }
                return new CsProvider(logger, options, secretManager);
            
            case CsProviderModeEnu.DynamicSecret:
                if (secretManager == null)
                {
                    throw new InvalidOperationException(
                        $"Mode {config.Mode} requires a registered IPvNugsSecretManager. " +
                        "Register it with: services.AddSingleton<IPvNugsSecretManager, YourImplementation>()");
                }
                if (!secretManager.SupportsDatabaseSecrets)
                {
                    throw new InvalidOperationException(
                        $"Mode {config.Mode} requires a secret manager that supports dynamic database secrets. " +
                        "Ensure your implementation of IPvNugsSecretManager has SupportsDatabaseSecrets = true.");
                }
                return new CsProvider(logger, options, secretManager);
            
            default:
                throw new SwitchExpressionException(
                    $"Unsupported mode: {config.Mode}. Valid modes are: Config, StaticSecret, DynamicSecret.");
        }
    }
   
    
    /// <summary>
    /// Validates the configuration row for required properties based on the selected mode.
    /// </summary>
    /// <param name="configRow">The configuration row to validate.</param>
    /// <exception cref="OptionsValidationException">Thrown when a required property is missing or invalid.</exception>
    private static void ValidateConfiguration(PvNugsCsProviderMsSqlConfigRow configRow)
    {
        if (string.IsNullOrWhiteSpace(configRow.Name))
            throw new OptionsValidationException(
                "Name is required for each configuration row.", 
                typeof(PvNugsCsProviderMsSqlConfigRow), ["Name"]);
        if (string.IsNullOrWhiteSpace(configRow.Server))
            throw new OptionsValidationException(
                "Server is required for each configuration row.", 
                typeof(PvNugsCsProviderMsSqlConfigRow), ["Server"]);
        if (string.IsNullOrWhiteSpace(configRow.Database))
            throw new OptionsValidationException(
                "Database is required for each configuration row.", 
                typeof(PvNugsCsProviderMsSqlConfigRow), ["Database"]);
        
        switch (configRow.Mode)
        {
            case CsProviderModeEnu.Config:
                // Username is required in Config mode
                if (string.IsNullOrWhiteSpace(configRow.Username))
                    throw new OptionsValidationException(
                        "Username is required in Config mode.", 
                        typeof(PvNugsCsProviderMsSqlConfigRow),
                        ["Username"]);
                // Password is optional
                break;
            
            case CsProviderModeEnu.StaticSecret:
                // Username is required in StaticSecret mode
                if (string.IsNullOrWhiteSpace(configRow.Username))
                    throw new OptionsValidationException(
                        "Username is required in StaticSecret mode.", 
                        typeof(PvNugsCsProviderMsSqlConfigRow),
                        ["Username"]);
                // at least one SecretParams dictionary is required
                if (configRow.ReaderSecretParams == null
                    && configRow.ApplicationSecretParams == null
                    && configRow.OwnerSecretParams == null)
                    throw new OptionsValidationException(
                        "at least one SecretParams dictionary is required in StaticSecret mode.", 
                        typeof(PvNugsCsProviderMsSqlConfigRow),
                        ["ReaderSecretParams or ApplicationSecretParams or OwnerSecretParams"]);
                break;
            
            case CsProviderModeEnu.DynamicSecret:
                // Username is ignored as it is dynamically generated
                // at least one SecretParams dictionary is required
                if (configRow.ReaderSecretParams == null
                    && configRow.ApplicationSecretParams == null
                    && configRow.OwnerSecretParams == null)
                    throw new OptionsValidationException(
                        "at least one SecretParams dictionary is required in DynamicSecret mode.", 
                        typeof(PvNugsCsProviderMsSqlConfigRow),
                        ["ReaderSecretParams or ApplicationSecretParams or OwnerSecretParams"]);
                break;
            
            default:
                throw new OptionsValidationException(
                    $"Unsupported mode: {configRow.Mode}", 
                    typeof(PvNugsCsProviderMsSqlConfigRow),
                    ["Mode"]);
        }
    }

}
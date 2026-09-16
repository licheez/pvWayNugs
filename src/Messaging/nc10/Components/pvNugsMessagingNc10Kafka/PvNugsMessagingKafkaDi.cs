using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using pvNugsMessagingNc10Abstractions;
// ReSharper disable MemberCanBePrivate.Global

namespace pvNugsMessagingNc10Kafka;

/// <summary>
/// Provides dependency injection extensions for the Kafka messaging provider.
/// </summary>
public static class PvNugsMessagingKafkaDi
{
    /// <summary>
    /// Registers the Kafka message producer and its dependencies.
    /// </summary>
    public static IServiceCollection TryAddPvNugsMessagingKafkaProducer(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        AddCommonConfiguration(
            services,
            configuration);

        services.Configure<PvNugsMessagingKafkaProducerConfig>(
            configuration.GetSection(
                PvNugsMessagingKafkaProducerConfig.Section));

        services.TryAddSingleton<IPvNugsMessagingProducer,
            PvNugsMessagingKafkaProducer>();

        return services;
    }

    /// <summary>
    /// Registers the Kafka message consumers and their dependencies.
    /// </summary>
    public static IServiceCollection TryAddPvNugsMessagingKafkaConsumer(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        AddCommonConfiguration(
            services,
            configuration);

        services.Configure<PvNugsMessagingKafkaConsumerConfig>(
            configuration.GetSection(
                PvNugsMessagingKafkaConsumerConfig.Section));

        services.TryAddSingleton<IPvNugsMessagingConsumer,
            PvNugsMessagingKafkaStringConsumer>();

        services.TryAddSingleton(
            typeof(IPvNugsMessagingConsumer<>),
            typeof(PvNugsMessagingKafkaTypedConsumer<>));

        return services;
    }

    /// <summary>
    /// Registers the Kafka messaging monitor and its dependencies.
    /// </summary>
    public static IServiceCollection TryAddPvNugsMessagingKafkaMonitor(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        AddCommonConfiguration(
            services,
            configuration);

        services.TryAddSingleton<IPvNugsMessagingMonitor,
            PvNugsMessagingKafkaMonitor>();

        return services;
    }

    /// <summary>
    /// Registers all Kafka messaging components and their dependencies.
    /// </summary>
    public static IServiceCollection TryAddPvNugsMessagingKafka(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.TryAddPvNugsMessagingKafkaProducer(
            configuration);

        services.TryAddPvNugsMessagingKafkaConsumer(
            configuration);

        services.TryAddPvNugsMessagingKafkaMonitor(
            configuration);

        return services;
    }

    /// <summary>
    /// Registers configuration and services shared by all Kafka
    /// messaging components.
    /// </summary>
    private static void AddCommonConfiguration(
        IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<PvNugsMessagingKafkaConfig>(
            configuration.GetSection(
                PvNugsMessagingKafkaConfig.Section));

        services.Configure<PvNugsMessagingKafkaSecurityConfig>(
            configuration.GetSection(
                PvNugsMessagingKafkaSecurityConfig.Section));

        services.TryAddSingleton<
            PvNugsMessagingKafkaSecurityService>();
    }
}
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using pvNugsLoggerNc10Abstractions;
using pvNugsLoggerNc10Seri;
using pvNugsMessagingNc10Kafka;
using pvNugsSecretManagerNc10Abstractions;

namespace PvNugsMessagingNc10KafkaLocal.it;

public static class AuthFreeTest
{
    public static async Task RunAsync()
    {
        var inMemSettings = new Dictionary<string, string>
        {
            // SERILOG
            { "PvNugsLoggerConfig:MinLogLevel", "trace" },

            // KAFKA
            { "PvNugsMessagingKafkaConfig:BootstrapServers", "localhost:9092" },
    
            // KAFKA SECURITY
            { "PvNugsMessagingKafkaSecurityConfig:Mode", "None" },
    
            // KAFKA PRODUCER
            { "PvNugsMessagingKafkaProducerConfig:Dummy", "false" },

            // KAFKA CONSUMER
            { "PvNugsMessagingKafkaConsumerConfig:Dummy", "false" }
        };

        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemSettings!)
            .Build();

        var services = new ServiceCollection();

        services.TryAddPvNugsLoggerSeriService(config)
            .TryAddPvNugsMessagingKafka(config)
            .AddTransient<KafkaTopicGenerator>()
            .AddTransient<ProducerRunner>()
            .AddTransient<ConsumerRunner>()
            .AddTransient<MonitorRunner>()
            .AddTransient<IPvNugsSecretManager, SimpleSecretManager>();

        var sp = services.BuildServiceProvider();

        var logger = sp.GetRequiredService<ILoggerService>();
        var kafkaTopicGenerator = sp.GetRequiredService<KafkaTopicGenerator>();
        var producerRunner = sp.GetRequiredService<ProducerRunner>();
        var consumerRunner = sp.GetRequiredService<ConsumerRunner>();
        var monitorRunner = sp.GetRequiredService<MonitorRunner>();

        await logger.LogAsync("Starting integration test for Kafka Messaging Provider .NET 10", SeverityEnu.Trace);
 
        const string topicName = "auth-free-test-topic";

        await kafkaTopicGenerator.EnsureTopicExistsAsync(topicName);
        await monitorRunner.GetStatusAsync(topicName);
        await producerRunner.PublishAsync(topicName);
        await monitorRunner.GetStatusAsync(topicName);
        await consumerRunner.LaunchConsumerAsync(topicName);
        await monitorRunner.GetStatusAsync(topicName);

        Console.WriteLine("Press any key to exit...");
        Console.ReadKey();
        await consumerRunner.StopConsumerAsync();
        await monitorRunner.GetStatusAsync(topicName);
    }
}
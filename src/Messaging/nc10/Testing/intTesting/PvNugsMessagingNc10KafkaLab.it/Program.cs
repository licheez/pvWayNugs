using Confluent.Kafka;
using Confluent.Kafka.Admin;

Console.WriteLine("Creating topic...");

await EnsureTopicExistsAsync("localhost:9092", "my-topic");

return;

static async Task EnsureTopicExistsAsync(
    string bootstrapServers,
    string topicName,
    int numPartitions = 1,
    short replicationFactor = 1)
{
    var adminConfig = new AdminClientConfig
    {
        BootstrapServers = bootstrapServers
    };

    using var adminClient = new AdminClientBuilder(adminConfig).Build();

    var metadata = adminClient.GetMetadata(TimeSpan.FromSeconds(10));
    var topicExists = metadata.Topics.Any(t => t.Topic == topicName);

    if (topicExists)
    {
        Console.WriteLine($"Topic '{topicName}' already exists.");
        return;
    }

    try
    {
        await adminClient.CreateTopicsAsync(new[]
        {
            new TopicSpecification
            {
                Name = topicName,
                NumPartitions = numPartitions,
                ReplicationFactor = replicationFactor
            }
        });

        Console.WriteLine($"Topic '{topicName}' created successfully.");
    }
    catch (CreateTopicsException ex)
    {
        if (ex.Results.Any(r => r.Error.Code == ErrorCode.TopicAlreadyExists))
        {
            Console.WriteLine($"Topic '{topicName}' was created concurrently by another process.");
            return;
        }

        throw;
    }
}
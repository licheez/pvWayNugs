using PvNugsMessagingNc10KafkaLocal.it;

Console.WriteLine("Integration Testing Console for Kafka Messaging Provider .NET 10");

while (true)
{
    Console.WriteLine("Select the tests to run:");
    Console.WriteLine("1. AuthFreeTest");
    Console.WriteLine("2. SaslPlaintextTest");
    Console.WriteLine("Enter the number of the test to run (or 'q' to quit):");
    var input = Console.ReadLine();
    if (input == "q") return;

    switch (input)
    {
        case "1":
            await AuthFreeTest.RunAsync();
            break;
        case "2":
            await SaslPlaintextTest.RunAsync();
            break;
        default:
            Console.WriteLine("Invalid selection. Please try again.");
            break;
    }
}



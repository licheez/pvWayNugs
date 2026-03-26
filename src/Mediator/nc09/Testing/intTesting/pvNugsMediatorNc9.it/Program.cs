using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using pvNugsLoggerNc9Abstractions;
using pvNugsLoggerNc9Seri;
using pvNugsMediatorNc9.it;
using pvNugsMediatorNc9.it.ScopingTests;

Console.WriteLine("Integration Testing for pvNugsMediatorNc9");

var inMemSettings = new Dictionary<string, string>
{
    // SERILOG
    { "PvNugsLoggerConfig:MinLogLevel", "trace" },
};

var config = new ConfigurationBuilder()
    .AddInMemoryCollection(inMemSettings!)
    .Build();

var services = new ServiceCollection();

services.TryAddPvNugsLoggerSeriService(config);

var sp = services.BuildServiceProvider();
var logger = sp.GetRequiredService<IConsoleLoggerService>();

while (true)
{
    Console.WriteLine("Select the test you want to run:");
    Console.WriteLine("1) Scoping");
    Console.WriteLine("2) Manual");
    Console.WriteLine("3) Decorated");
    Console.WriteLine("4) Full Scan");
    Console.WriteLine("Enter 0 to quit");

    var input = Console.ReadLine();
    if (input == "0") break;
    
    switch (input)
    {
        case "1":
            var scopingTests = new ScopingTests(logger);
            await scopingTests.RunAllTestsAsync();
            break;
        case "2":
            var manual = new Manual(logger);
            await manual.RunTestAsync();
            break;
        case "3":
            var decorated = new Decorated(logger);
            await decorated.RunTestAsync();
            break;
        case "4":
            var fullScan = new FullScan(logger);
            await fullScan.RunTestAsync();
            break;
    }
}




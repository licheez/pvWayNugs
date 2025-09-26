using pvNugsCsProviderNc9MsSql.it;

Console.WriteLine("Integration testing console for pvNugsCsProviderNc9MsSql");

await MultiConfigTester.RunAsync();
await TrustedConfigModeTester.RunAsync();
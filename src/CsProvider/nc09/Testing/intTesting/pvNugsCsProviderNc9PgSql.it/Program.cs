using pvNugsCsProviderNc9PgSql.it;

Console.WriteLine("Integration testing console for pvNugsCsProviderNc9PgSql");

await MultiConfigTester.RunAsync();
await ConfigModeTester.RunAsync();
await StaticModeTester.RunAsync();
await DynamicModeTester.RunAsync();

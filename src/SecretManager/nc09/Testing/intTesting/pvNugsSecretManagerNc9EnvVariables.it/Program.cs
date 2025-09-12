using pvNugsSecretManagerNc9EnvVariables.it;

Console.WriteLine("Integration Testing Console for pvNugsSecretManagerNc9EnvVariables");

await StaticModeTester.RunAsync();
await DynamicModeTester.RunAsync();
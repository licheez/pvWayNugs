using VaultSharp;
using VaultSharp.V1.SecretsEngines;
using VaultSharp.V1.SecretsEngines.Database;
using VaultSharp.V1.SecretsEngines.Database.Models;

namespace pvNugsSecretManagerNc10VaultLab.it;

/// <summary>
/// Provisions a Microsoft SQL Server database secrets engine
/// with dynamic credentials in HashiCorp Vault.
/// </summary>
/// <param name="vaultClient"></param>
/// <param name="defaultTtl"></param>
/// <param name="maxTtl"></param>
public sealed class MsSqlSecretProvisioner(
    VaultClient vaultClient,
    string defaultTtl,
    string maxTtl)
{
    public async Task ProvisionAsync(int portNumber)
    {
        await ProvisionDbEngineAsync(portNumber);
        await ConfigureDbConnectionAsync(portNumber);
        await ConfigureDbRoleAsync(portNumber);
        await TestProvisioningAsync(portNumber);
    }

    private async Task TestProvisioningAsync(int portNumber)
    {
        var testDbEngine = vaultClient.V1.Secrets.Database;

        var credentials = await testDbEngine.GetCredentialsAsync(
            roleName: "owner",
            mountPoint: GetMountPoint(portNumber));

        Console.WriteLine($"username: {credentials.Data.Username}");
        Console.WriteLine($"password: {credentials.Data.Password}");
        Console.WriteLine($"ttl: {credentials.LeaseDurationSeconds} seconds");
    }

    private async Task ProvisionDbEngineAsync(int portNumber)
    {
        var mountPoint = GetMountPoint(portNumber);

        Console.WriteLine("  Creating Database Secrets Engine:");
        Console.WriteLine($"    {mountPoint}");

        var dbEngine = new SecretsEngine
        {
            Type = new SecretsEngineType("database"),

            Description =
                $"Microsoft SQL Server Dynamic Credentials - ms{portNumber}",

            Path = mountPoint,

            Config = new Dictionary<string, object>
            {
                { "default_lease_ttl", defaultTtl },
                { "max_lease_ttl", maxTtl }
            }
        };

        await vaultClient.V1.System.MountSecretBackendAsync(dbEngine);
    }

    private async Task ConfigureDbConnectionAsync(int portNumber)
    {
        var connectionName = GetConnectionName(portNumber);
        var mountPoint = GetMountPoint(portNumber);

        Console.WriteLine("  Creating Microsoft SQL Server connection:");
        Console.WriteLine($"    {connectionName}");

        var connectionConfig = new ConnectionConfigModel
        {
            ConnectionUrl =
                $"sqlserver://{{{{username}}}}:{{{{password}}}}" +
                $"@mssql{portNumber}:1433",

            Username = "sa",
            Password = "YourStrong!Passw0rd",

            PluginName = "mssql-database-plugin",

            VerifyConnection = true,

            AllowedRoles = ["owner"]
        };

        await vaultClient.V1.Secrets.Database.ConfigureConnectionAsync(
            connectionName,
            connectionConfig,
            mountPoint);
    }

    private async Task ConfigureDbRoleAsync(int portNumber)
    {
        var connectionName = GetConnectionName(portNumber);
        var mountPoint = GetMountPoint(portNumber);

        Console.WriteLine("  Creating dynamic role:");
        Console.WriteLine("    owner");

        var databaseProvider =
            new DatabaseProviderType(connectionName);

        var role = new Role
        {
            DatabaseProviderType = databaseProvider,

            DefaultTimeToLive = defaultTtl,
            MaximumTimeToLive = maxTtl,

            CreationStatements =
            [
                """
                CREATE LOGIN [{{name}}]
                WITH PASSWORD = '{{password}}';
                """
            ]
        };

        await vaultClient.V1.Secrets.Database.CreateRoleAsync(
            "owner",
            role,
            mountPoint);
    }

    private static string GetMountPoint(int portNumber) =>
        $"database/mssql/ms{portNumber}";

    private static string GetConnectionName(int portNumber) =>
        $"ms{portNumber}";
}
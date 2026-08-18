using VaultSharp;
using VaultSharp.V1.SecretsEngines;
using VaultSharp.V1.SecretsEngines.Database;
using VaultSharp.V1.SecretsEngines.Database.Models;

namespace pvNugsSecretManagerNc10VaultLab.it;

/// <summary>
/// Provisions a PostgreSQL database secrets engine with dynamic credentials in HashiCorp Vault.
/// </summary>
/// <param name="vaultClient"></param>
/// <param name="defaultTtl"></param>
/// <param name="maxTtl"></param>
public sealed class PostgresSecretProvisioner(
    VaultClient vaultClient,
    string defaultTtl,
    string maxTtl)
{
    /// <summary>
    /// Provisions a PostgreSQL database secrets engine with dynamic credentials.
    /// </summary>
    /// <param name="portNumber"></param>
    public async Task ProvisionAsync(int portNumber)
    {
        await ProvisionDbEngineAsync(portNumber);
        await ConfigureDbConnectionAsync(portNumber);
        await ConfigureDbRoleAsync(portNumber);
        await TestProvisioningAsync(portNumber);
    }

    /// <summary>
    /// Tests the provisioning of dynamic credentials for a PostgreSQL database.
    /// </summary>
    /// <param name="portNumber"></param>
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

    /// <summary>
    /// Provisions a database secrets engine for PostgreSQL with dynamic credentials.
    /// </summary>
    /// <param name="portNumber"></param>
    private async Task ProvisionDbEngineAsync(
        int portNumber)
    {
        var mountPoint = GetMountPoint(portNumber);

        Console.WriteLine($"  Creating Database Secrets Engine:");
        Console.WriteLine($"    {mountPoint}");

        var dbEngine = new SecretsEngine
        {
            Type = new SecretsEngineType("database"),

            Description = $"PostgreSQL Dynamic Credentials - pg{portNumber}",

            Path = mountPoint,

            Config = new Dictionary<string, object>
            {
                { "default_lease_ttl", defaultTtl },
                { "max_lease_ttl", maxTtl }
            }
        };

        await vaultClient.V1.System.MountSecretBackendAsync(dbEngine);
    }

    /// <summary>
    /// Configures a database connection for PostgreSQL with dynamic credentials.
    /// </summary>
    /// <param name="portNumber"></param>
    private async Task ConfigureDbConnectionAsync(int portNumber)
    {
        var connectionName = GetConnectionName(portNumber);
        var mountPoint = GetMountPoint(portNumber);

        Console.WriteLine($"  Creating PostgreSQL connection:");
        Console.WriteLine($"    {connectionName}");

        var connectionConfig = new ConnectionConfigModel
        {
            ConnectionUrl =
                $"postgresql://{{{{username}}}}:{{{{password}}}}" +
                $"@postgres{portNumber}:5432/postgres?sslmode=disable",

            Username = "postgres",
            Password = "dev-only-password",

            PluginName = "postgresql-database-plugin",

            VerifyConnection = true,

            AllowedRoles = ["owner"]
        };

        await vaultClient.V1.Secrets.Database.ConfigureConnectionAsync(
            connectionName,
            connectionConfig,
            mountPoint);
    }

    /// <summary>
    /// Configures a dynamic database role for PostgreSQL with dynamic credentials.
    /// </summary>
    /// <param name="portNumber"></param>
    private async Task ConfigureDbRoleAsync(int portNumber)
    {
        var connectionName = GetConnectionName(portNumber);
        var mountPoint = GetMountPoint(portNumber);

        Console.WriteLine($"  Creating dynamic role:");
        Console.WriteLine($"    owner");

        /*
         * VaultSharp's DatabaseProviderType property represents Vault's
         * database connection name (db_name) for a database role.
         */
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
                CREATE ROLE "{{name}}"
                WITH LOGIN
                PASSWORD '{{password}}'
                VALID UNTIL '{{expiration}}';
                """
            ]
        };

        await vaultClient.V1.Secrets.Database.CreateRoleAsync(
            "owner",
            role,
            mountPoint);
    }

    /// <summary>
    /// Gets the mount point for the database secrets engine based on the port number.
    /// </summary>
    /// <param name="portNumber"></param>
    /// <returns></returns>
    private static string GetMountPoint(int portNumber) =>
        $"database/postgres/pg{portNumber}";

    /// <summary>
    /// Gets the connection name for the database connection based on the port number.
    /// </summary>
    /// <param name="portNumber"></param>
    /// <returns></returns>
    private static string GetConnectionName(int portNumber) =>
        $"pg{portNumber}";
}
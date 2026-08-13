using VaultSharp;
using VaultSharp.V1.AuthMethods.Token;
using VaultSharp.V1.SecretsEngines;
using VaultSharp.V1.SecretsEngines.Database;
using VaultSharp.V1.SecretsEngines.Database.Models;

/*
 * =============================================================================
 * HashiCorp Vault Lab - Provisioning Console
 * =============================================================================
 *
 * PURPOSE
 * -------
 *
 * This console provisions the HashiCorp Vault configuration required by the
 * pvNugs Secret Manager integration test laboratory.
 *
 * It does NOT test dynamic credentials and does NOT consume secrets.
 *
 * Infrastructure lifecycle is managed by Docker Compose.
 * Vault configuration is managed by this console using VaultSharp.
 *
 *
 * =============================================================================
 * PREREQUISITES
 * =============================================================================
 *
 * 1. Install Docker Desktop.
 *
 * 2. Open a terminal in the directory containing:
 *
 *      compose.yaml
 *      servers.json
 *
 * 3. Pull all images required by the Compose project:
 *
 *      docker compose pull
 *
 * 4. Start the laboratory:
 *
 *      docker compose up -d
 *
 * 5. Run this provisioning console.
 *
 *
 * =============================================================================
 * DOCKER COMPOSE LAB
 * =============================================================================
 *
 * Compose project:
 *
 *      vault-lab
 *
 * Services:
 *
 *      postgres5432
 *          Windows endpoint : localhost:5432
 *          Docker endpoint  : postgres5432:5432
 *
 *      postgres5433
 *          Windows endpoint : localhost:5433
 *          Docker endpoint  : postgres5433:5432
 *
 *      vault
 *          API : http://localhost:8200
 *          UI  : http://localhost:8200/ui
 *
 *      pgadmin
 *          UI  : http://localhost:5050
 *
 *
 * =============================================================================
 * PGADMIN
 * =============================================================================
 *
 * Login:
 *
 *      Email    : admin@example.com
 *      Password : dev-only-password
 *
 * PostgreSQL servers are pre-registered from servers.json:
 *
 *      Vault Lab
 *          |
 *          +-- pg5432 -> postgres5432:5432
 *          |
 *          +-- pg5433 -> postgres5433:5432
 *
 * PostgreSQL passwords are NOT pre-provisioned in pgAdmin.
 *
 * When pgAdmin asks for a PostgreSQL password, use:
 *
 *      dev-only-password
 *
 *
 * =============================================================================
 * VAULT DEVELOPMENT CONFIGURATION
 * =============================================================================
 *
 * Vault runs in DEV mode.
 *
 * API:
 *
 *      http://localhost:8200
 *
 * UI:
 *
 *      http://localhost:8200/ui
 *
 * Authentication:
 *
 *      Method : Token
 *      Token  : dev-only-token
 *
 * WARNING:
 *
 * Vault DEV mode stores its state in memory.
 *
 * Stopping this provisioning console does NOT destroy Vault configuration.
 *
 * However, destroying/recreating the Vault container destroys:
 *
 *      - Secrets engines
 *      - Database connections
 *      - Database roles
 *      - Leases
 *      - Other Vault configuration
 *
 * For example:
 *
 *      docker compose down
 *      docker compose up -d
 *
 * creates a fresh Vault instance.
 *
 * After recreating the Vault container, run THIS provisioning console again.
 *
 *
 * =============================================================================
 * PROVISIONED VAULT STRUCTURE
 * =============================================================================
 *
 * This console provisions:
 *
 *      database/postgres/pg5432
 *          |
 *          +-- connection: pg5432
 *          |       |
 *          |       +-- postgres5432:5432
 *          |
 *          +-- role: owner
 *
 *
 *      database/postgres/pg5433
 *          |
 *          +-- connection: pg5433
 *          |       |
 *          |       +-- postgres5433:5432
 *          |
 *          +-- role: owner
 *
 *
 * Having one mount point per database allows the same logical role name
 * ("owner") to exist independently for each PostgreSQL target.
 *
 *
 * =============================================================================
 * DYNAMIC ROLE
 * =============================================================================
 *
 * Role:
 *
 *      owner
 *
 * Default TTL:
 *
 *      1 hour
 *
 * Maximum TTL:
 *
 *      24 hours
 *
 * PostgreSQL creation statement:
 *
 *      CREATE ROLE "{{name}}"
 *      WITH LOGIN
 *      PASSWORD '{{password}}'
 *      VALID UNTIL '{{expiration}}';
 *
 *
 * =============================================================================
 * AFTER PROVISIONING
 * =============================================================================
 *
 * Once this console completes successfully, Vault is ready for the separate
 * Secret Manager integration-test console.
 *
 * Example:
 *
 *      GetCredentialsAsync(
 *          roleName: "owner",
 *          mountPoint: "database/postgres/pg5432");
 *
 * or:
 *
 *      GetCredentialsAsync(
 *          roleName: "owner",
 *          mountPoint: "database/postgres/pg5433");
 *
 * Credential generation, lease inspection and lease revocation belong to
 * the integration-test application and are intentionally NOT performed here.
 *
 * =============================================================================
 */

const string vaultAddress = "http://127.0.0.1:8200";
const string vaultToken = "dev-only-token";

int[] postgresPorts = [5432, 5433];

// -----------------------------------------------------------------------------
// Create Vault client
// -----------------------------------------------------------------------------

var authMethodInfo = new TokenAuthMethodInfo(vaultToken);

var clientSettings = new VaultClientSettings(
    vaultAddress,
    authMethodInfo);

var client = new VaultClient(clientSettings);


// -----------------------------------------------------------------------------
// Verify that Vault is available before provisioning
// -----------------------------------------------------------------------------

try
{
    var health = await client.V1.System.GetHealthStatusAsync();

    Console.WriteLine("Connected to HashiCorp Vault.");
    Console.WriteLine($"Initialized : {health.Initialized}");
    Console.WriteLine($"Sealed      : {health.Sealed}");
    Console.WriteLine();

    if (!health.Initialized || health.Sealed)
    {
        Console.WriteLine("Vault is not ready for provisioning.");
        return;
    }
}
catch (Exception ex)
{
    Console.WriteLine("Unable to connect to HashiCorp Vault.");
    Console.WriteLine(ex.Message);
    return;
}


// -----------------------------------------------------------------------------
// Provision each PostgreSQL target
// -----------------------------------------------------------------------------

try
{
    foreach (var port in postgresPorts)
    {
        Console.WriteLine($"Provisioning pg{port}...");
        Console.WriteLine();

        await ProvisionDbEngineAsync(client, port);
        await ConfigureDbConnectionAsync(client, port);
        await ConfigureDbRoleAsync(client, port);

        Console.WriteLine();
        Console.WriteLine($"pg{port} provisioned successfully.");
        Console.WriteLine();
    }

    Console.WriteLine("------------------------------------------------------------");
    Console.WriteLine("Vault provisioning completed successfully.");
    Console.WriteLine("The Secret Manager integration tests can now be executed.");
    Console.WriteLine("------------------------------------------------------------");
}
catch (Exception ex)
{
    Console.WriteLine();
    Console.WriteLine("Vault provisioning failed.");
    Console.WriteLine(ex.Message);
}

return;


// -----------------------------------------------------------------------------
// Database Secrets Engine
// -----------------------------------------------------------------------------

async Task ProvisionDbEngineAsync(
    VaultClient vaultClient,
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
            { "default_lease_ttl", "1h" },
            { "max_lease_ttl", "24h" }
        }
    };

    await vaultClient.V1.System.MountSecretBackendAsync(dbEngine);
}


// -----------------------------------------------------------------------------
// PostgreSQL connection
// -----------------------------------------------------------------------------

async Task ConfigureDbConnectionAsync(
    VaultClient vaultClient,
    int portNumber)
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

        VerifyConnection = true
    };

    await vaultClient.V1.Secrets.Database.ConfigureConnectionAsync(
        connectionName,
        connectionConfig,
        mountPoint);
}


// -----------------------------------------------------------------------------
// Dynamic database role
// -----------------------------------------------------------------------------

async Task ConfigureDbRoleAsync(
    VaultClient vaultClient,
    int portNumber)
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

        DefaultTimeToLive = "1h",
        MaximumTimeToLive = "24h",

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


// -----------------------------------------------------------------------------
// Naming conventions
// -----------------------------------------------------------------------------

string GetMountPoint(int portNumber) =>
    $"database/postgres/pg{portNumber}";

string GetConnectionName(int portNumber) =>
    $"pg{portNumber}";
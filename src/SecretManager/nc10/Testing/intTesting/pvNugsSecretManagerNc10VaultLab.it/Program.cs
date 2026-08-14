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
 * pvNugs Secret Manager integration-test laboratory.
 *
 * Infrastructure lifecycle is managed by Docker Compose.
 * Vault configuration is managed by this console using VaultSharp.
 *
 * The console provisions:
 *
 *   - Static secrets using the Vault KV v2 Secrets Engine.
 *   - PostgreSQL dynamic credentials using Database Secrets Engines.
 *   - One independent Database Secrets Engine per PostgreSQL target.
 *
 * A dynamic credential is generated at the end of the provisioning process
 * as a simple validation that the PostgreSQL Database Secrets Engine is
 * correctly configured.
 *
 * The actual Secret Manager integration tests are implemented separately.
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
 * However, destroying/recreating the Vault container destroys the Vault state,
 * including:
 *
 *      - Static secrets
 *      - Secrets engines
 *      - Database connections
 *      - Database roles
 *      - Dynamic credentials
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
 * STATIC SECRETS - KV v2
 * =============================================================================
 *
 * The laboratory uses the default KV v2 Secrets Engine mounted at:
 *
 *      secret/
 *
 * This console provisions the following secret:
 *
 *      secret/secret-manager-test
 *
 * containing:
 *
 *      username    = lab-user
 *      password    = lab-password
 *      api-key     = lab-api-key
 *      environment = development
 *
 * These values are intentionally non-sensitive development values and exist
 * only to support Secret Manager integration tests.
 *
 * Example VaultSharp access:
 *
 *      var secret = await client.V1.Secrets.KeyValue.V2.ReadSecretAsync(
 *          path: "secret-manager-test",
 *          mountPoint: "secret");
 *
 *
 * =============================================================================
 * DYNAMIC DATABASE SECRETS
 * =============================================================================
 *
 * Two independent PostgreSQL Database Secrets Engines are provisioned:
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
 * Having one mount point per database allows the same logical role name
 * ("owner") to exist independently for each PostgreSQL target.
 *
 *
 * =============================================================================
 * POSTGRESQL CONNECTION CONFIGURATION
 * =============================================================================
 *
 * Connection names:
 *
 *      pg5432
 *      pg5433
 *
 * Connection URLs:
 *
 *      postgresql://{{username}}:{{password}}@postgres5432:5432/postgres?sslmode=disable
 *
 *      postgresql://{{username}}:{{password}}@postgres5433:5432/postgres?sslmode=disable
 *
 * Root PostgreSQL credentials used by Vault:
 *
 *      Username : postgres
 *      Password : dev-only-password
 *
 * Database plugin:
 *
 *      postgresql-database-plugin
 *
 * Connection verification:
 *
 *      enabled
 *
 * Allowed dynamic roles:
 *
 *      owner
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
 *      1 minute
 *
 * Maximum TTL:
 *
 *      15 minutes
 *
 * Each GetCredentials request creates a new PostgreSQL login with its own
 * Vault lease.
 *
 * PostgreSQL creation statement:
 *
 *      CREATE ROLE "{{name}}"
 *      WITH LOGIN
 *      PASSWORD '{{password}}'
 *      VALID UNTIL '{{expiration}}';
 *
 * The default TTL determines the initial lifetime of a generated credential.
 *
 * The maximum TTL determines the maximum lifetime that the credential can
 * reach through lease renewal.
 *
 *
 * =============================================================================
 * PROVISIONING VALIDATION
 * =============================================================================
 *
 * After provisioning, this console requests one dynamic credential from:
 *
 *      database/postgres/pg5432
 *
 * using role:
 *
 *      owner
 *
 * Example:
 *
 *      var credentials =
 *          await client.V1.Secrets.Database.GetCredentialsAsync(
 *              roleName: "owner",
 *              mountPoint: "database/postgres/pg5432");
 *
 * The console displays:
 *
 *      - generated username
 *      - generated password
 *      - lease TTL
 *
 * This request exists only as a provisioning smoke test.
 *
 * Every GetCredentialsAsync call creates a NEW dynamic PostgreSQL credential
 * and an independent Vault lease.
 *
 *
 * =============================================================================
 * AFTER PROVISIONING
 * =============================================================================
 *
 * Once this console completes successfully, Vault is ready for the separate
 * pvNugs Secret Manager integration-test application.
 *
 * The integration tests can exercise:
 *
 *      Static secrets
 *          secret/secret-manager-test
 *
 *      Dynamic PostgreSQL credentials
 *          database/postgres/pg5432/creds/owner
 *          database/postgres/pg5433/creds/owner
 *
 * The Secret Manager is responsible for consuming and caching these secrets.
 *
 * Lease renewal is intentionally outside the scope of the current laboratory
 * implementation. When a cached dynamic credential is expired or considered
 * close to expiration, the Secret Manager can request a new credential rather
 * than renewing the existing lease.
 *
 *
 * =============================================================================
 * DEVELOPMENT ONLY
 * =============================================================================
 *
 * This entire environment is intended exclusively for local development and
 * integration testing.
 *
 * The following values are deliberately hard-coded development credentials:
 *
 *      Vault root token    : dev-only-token
 *      PostgreSQL password : dev-only-password
 *      pgAdmin password    : dev-only-password
 *
 * NONE of these settings, credentials or deployment patterns must be used
 * in a production environment.
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

await ProvisionSecretEngines();
await ConfigureStaticSecretsAsync(client);

// -----------------------------------------------------------------------------
// Testing and validation of the provisioned Vault configuration is performed
// -----------------------------------------------------------------------------

var testDbEngine = client.V1.Secrets.Database;
var credentials = await testDbEngine.GetCredentialsAsync(
    roleName: "owner",
    mountPoint: GetMountPoint(5432));
Console.WriteLine($"username: {credentials.Data.Username}");
Console.WriteLine($"password: {credentials.Data.Password}");
Console.WriteLine($"ttl: {credentials.LeaseDurationSeconds} seconds");

return;

async Task ProvisionSecretEngines()
{
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
}

async Task ConfigureStaticSecretsAsync(VaultClient vClient)
{
    Console.WriteLine("Configuring static KV secrets...");

    var secrets = new Dictionary<string, object>
    {
        ["username"] = "lab-user",
        ["password"] = "lab-password",
        ["api-key"] = "lab-api-key",
        ["environment"] = "development"
    };

    await vClient.V1.Secrets.KeyValue.V2.WriteSecretAsync(
        path: "secret-manager-test",
        data: secrets,
        mountPoint: "secret");

    Console.WriteLine("Static KV secrets provisioned successfully.");
}

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
            { "default_lease_ttl", "1m" },
            { "max_lease_ttl", "15m" }
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

        VerifyConnection = true,
        
        AllowedRoles = ["owner"]
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

        DefaultTimeToLive = "1m",
        MaximumTimeToLive = "15m",

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
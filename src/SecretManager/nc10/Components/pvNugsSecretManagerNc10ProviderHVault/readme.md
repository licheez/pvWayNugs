# 🔐 pvNugsSecretManagerNc10ProviderHVault

[![NuGet Version](https://img.shields.io/nuget/v/pvNugsSecretManagerNc10ProviderHVault.svg)](https://www.nuget.org/packages/pvNugsSecretManagerNc10ProviderHVault/)
[![NuGet Downloads](https://img.shields.io/nuget/dt/pvNugsSecretManagerNc10ProviderHVault.svg)](https://www.nuget.org/packages/pvNugsSecretManagerNc10ProviderHVault/)
[![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)
[![.NET](https://img.shields.io/badge/.NET-10.0-blue.svg)](https://dotnet.microsoft.com/download)

HashiCorp Vault provider for the pvWay Secret Manager stack on .NET 10. 🚀

This package implements `IPvNugsSecretProvider` and is intended to be used together with:

- `pvNugsSecretManagerNc10` for orchestration, caching, and exception normalization
- an application-defined `IPvNugsSecretManager` consumer such as a `callerService`

## 🏗️ Architecture

The current design is intentionally split into three layers:

1. **Caller service / application code** injects `IPvNugsSecretManager`
2. **Secret manager package** (`pvNugsSecretManagerNc10`) orchestrates calls, caching, and logging
3. **Provider package** (`pvNugsSecretManagerNc10ProviderHVault`) talks to HashiCorp Vault

```text
👤 Caller service
    ↓ injects
🔐 IPvNugsSecretManager
    ↓ delegates to
🔷 HVaultSecretProvider
    ↓ talks to
🏦 HashiCorp Vault
```

## ✨ Features

- 🔑 **Static Secrets**: Retrieve secrets from HashiCorp Vault Key-Value (v2) secrets engine
- 🎫 **Dynamic Credentials**: Generate temporary database credentials with automatic expiration
- 🔐 **Multiple Authentication**: Token-based and Kubernetes authentication
- 💾 **Caching Support**: Integrates with pvNugs Secret Manager caching layer
- 📝 **Comprehensive Logging**: Built-in logging for all operations
- 🛡️ **Type-Safe Configuration**: Strongly-typed configuration using .NET Options pattern
- ⚠️ **Exception Handling**: Custom exceptions with detailed error information

## ✅ Supported behavior

- ✔️ `GetStaticSecretAsync(...)` retrieves a single secret by name from KV v2 store
- ✔️ `GetStaticSecretsAsync(...)` retrieves all secrets at a given path in KV v2 store
- ✔️ `GetDynamicSecretAsync(...)` generates temporary database credentials with TTL

Unlike Azure Key Vault, HashiCorp Vault natively supports dynamic credential generation for databases, making it ideal for zero-trust security architectures.

---

## 📦 Installation

```powershell
Install-Package pvNugsSecretManagerNc10ProviderHVault
```

Or with the .NET CLI:

```bash
dotnet add package pvNugsSecretManagerNc10ProviderHVault
```

## 📚 Dependencies

This package depends on:

- `VaultSharp` (HashiCorp Vault client library)
- `pvNugsLoggerNc10Abstractions`
- `pvNugsSecretManagerNc10Abstractions`

The application that uses this provider should also reference:

- `pvNugsSecretManagerNc10`
- one cache implementation and one logger implementation required by the secret manager package

---

## ⚙️ Configuration

The provider is bound from the `PvNugsHVaultSecretProviderConfig` section.

### 🎫 Token Authentication

Token authentication is the simplest method, suitable for development and scenarios where you have direct access to a Vault token.

**appsettings.json:**

```json
{
  "PvNugsHVaultSecretProviderConfig": {
    "AuthMethod": "TokenAuth",
    "ServerUrl": "http://localhost:8200",
    "TokenFilePath": "vault-token.txt"
  }
}
```

**vault-token.txt:**

```
your-vault-token-here
```

### ☸️ Kubernetes Authentication

Kubernetes authentication is recommended for applications running inside a Kubernetes cluster.

**appsettings.json:**

```json
{
  "PvNugsHVaultSecretProviderConfig": {
    "AuthMethod": "Kubernetes",
    "ServerUrl": "https://vault.example.com",
    "TokenFilePath": "/var/run/secrets/kubernetes.io/serviceaccount/token",
    "KubeMountPoint": "kubernetes",
    "KubeRoleName": "my-app-role",
    "KubeNameSpace": "production"
  }
}
```

## 🔧 Service registration

```csharp
using pvNugsSecretManagerNc10;
using pvNugsSecretManagerNc10Abstractions;
using pvNugsSecretManagerNc10ProviderHVault;

var builder = WebApplication.CreateBuilder(args);

// Register the provider first
builder.Services.TryAddPvNugsHVaultSecretProvider(builder.Configuration);

// Register the provider-agnostic manager
builder.Services.TryAddPvNugsSecretManager(builder.Configuration);

var app = builder.Build();
```

---

## 💡 Usage

Your caller service should depend on `IPvNugsSecretManager`, not on the Vault provider directly.

### 🔑 Retrieving Static Secrets

Static secrets are stored in the Key-Value (v2) secrets engine.

**Example:**

```csharp
using pvNugsSecretManagerNc10Abstractions;
using pvNugsSecretManagerNc10ProviderHVault;

var secretManager = serviceProvider.GetRequiredService<IPvNugsSecretManager>();

// Retrieve a single secret
var parameters = PvNugsHVaultSecretProviderParameters.CreateStaticParameters(
    mountPoint: "secret",           // KV mount point
    path: "myapp/config",           // Path to secret
    secretName: "api-key"           // Specific secret name
);

var apiKey = await secretManager.GetStaticSecretAsync(parameters);
Console.WriteLine($"API Key: {apiKey}");

// Retrieve all secrets at a path
var allParameters = PvNugsHVaultSecretProviderParameters.CreateStaticParameters(
    mountPoint: "secret",
    path: "myapp/config",
    secretName: null                // null retrieves all secrets
);

var allSecrets = await secretManager.GetStaticSecretsAsync(allParameters);
foreach (var secret in allSecrets)
{
    Console.WriteLine($"{secret.Key}: {secret.Value}");
}
```

### 🎫 Generating Dynamic Credentials

Dynamic credentials are temporary database credentials with automatic expiration.

**Example:**

```csharp
var parameters = PvNugsHVaultSecretProviderParameters.CreateDynamicParameters(
    mountPoint: "database/postgres/pg5432",  // Database mount point
    roleName: "owner",                       // Database role
    timeToLive: "1h"                         // Optional: requested TTL
);

var credential = await secretManager.GetDynamicSecretAsync(parameters);

Console.WriteLine($"Username: {credential.Username}");
Console.WriteLine($"Password: {credential.Password}");
Console.WriteLine($"Expires: {credential.ExpirationDateUtc}");

// Use the credentials
var connectionString = $"Host=localhost;Username={credential.Username};Password={credential.Password};Database=mydb";
```

**Time-To-Live Format:**

The `timeToLive` parameter uses HashiCorp Vault's duration format (NOT .NET TimeSpan format):

- `"30s"` - 30 seconds
- `"5m"` - 5 minutes
- `"1h"` - 1 hour
- `"24h"` - 24 hours
- `"7d"` - 7 days

Valid units: `s` (seconds), `m` (minutes), `h` (hours), `d` (days)

---

## 🧪 Testing

### 📋 Prerequisites

1. **Docker Desktop** - Required for running the test environment
2. **.NET 10 SDK** - Required for building and running the test console

### 🐳 Setting Up the Test Environment

The repository includes a complete Docker-based test environment with HashiCorp Vault, PostgreSQL databases, and pgAdmin.

**Download the test files from GitHub:**

Navigate to the test directory:

```
src/SecretManager/nc10/Testing/intTesting/pvNugsSecretManagerNc10VaultLab.it/
```

**Files to download:**
- `compose.yaml` - Docker Compose configuration
- `servers.json` - pgAdmin server configuration
- `Program.cs` - Vault provisioning console

**Start the test environment:**

```bash
# Pull required Docker images
docker compose pull

# Start all services (Vault, PostgreSQL, pgAdmin)
docker compose up -d

# Verify services are running
docker compose ps
```

**Services:**

| Service | URL | Purpose |
|---------|-----|---------|
| Vault API | http://localhost:8200 | HashiCorp Vault server |
| Vault UI | http://localhost:8200/ui | Vault web interface |
| PostgreSQL (pg5432) | localhost:5432 | Test database 1 |
| PostgreSQL (pg5433) | localhost:5433 | Test database 2 |
| pgAdmin | http://localhost:5050 | PostgreSQL admin interface |

**Default Credentials:**

- **Vault Token**: `dev-only-token`
- **PostgreSQL Password**: `dev-only-password`
- **pgAdmin Email**: `admin@example.com`
- **pgAdmin Password**: `dev-only-password`

### ⚙️ Provisioning Vault

Before running tests, you must provision Vault with the required configuration:

```bash
# Navigate to the provisioning console directory
cd src/SecretManager/nc10/Testing/intTesting/pvNugsSecretManagerNc10VaultLab.it/

# Run the provisioning console
dotnet run
```

**What it does:**
- Creates Database secrets engines for both PostgreSQL instances
- Configures database connections: `pg5432` and `pg5433`
- Creates the `owner` role with 1-minute default TTL (1-hour max)
- Configures dynamic credential generation

**Provisioned Vault Structure:**

```
database/postgres/pg5432
    ├── connection: pg5432 → postgres5432:5432
    └── role: owner (creates temporary database users)

database/postgres/pg5433
    ├── connection: pg5433 → postgres5433:5432
    └── role: owner (creates temporary database users)

secret (Key-Value v2)
    └── secret-manager-test
        └── api-key: "my-super-secret-api-key"
```

### ▶️ Running the Tests

Once the environment is provisioned, run the integration tests:

```bash
# Navigate to the integration test console directory
cd src/SecretManager/nc10/Testing/intTesting/pvNugsSecretManagerNc10ProviderHVault.it/

# Ensure token.txt exists with content: dev-only-token
echo dev-only-token > token.txt

# Run the integration tests
dotnet run
```

**Expected output:**

```
Integration Testing Console for HashiCorp Vault SecretProvider .NET 10
Retrieving Static Secrets from PvNugsSecretManager
[TRACE] Static Secret Retrieved: my-super-secret-api-key
Retrieving Dynamic Secret from HashiCorp Vault with parameters: mountPoint='database/postgres/pg5432', role='owner'
[TRACE] Dynamic Secret Retrieved: v-token-owner-AbCdEfGh123 / P@ssw0rd123 / 2026-08-14 14:30:00Z
```

**Cleanup:**

```bash
# Stop and remove all containers
docker compose down

# Remove volumes (optional - clears all data)
docker compose down -v
```

### 🔍 Inspecting Dynamic Credentials

You can verify that Vault is creating temporary database users using pgAdmin:

1. Open http://localhost:5050
2. Login with `admin@example.com` / `dev-only-password`
3. Connect to `pg5432` server (password: `dev-only-password`)
4. Navigate to: **Servers → pg5432 → Login/Group Roles**
5. You'll see dynamically created users like `v-token-owner-AbCdEfGh123`

---

## 📝 API Reference

### PvNugsHVaultSecretProviderParameters

Factory methods for creating parameter dictionaries:

```csharp
// Static secrets
public static IReadOnlyDictionary<string, string> CreateStaticParameters(
    string mountPoint,      // KV mount point (e.g., "secret")
    string path,            // Path to secret (e.g., "myapp/config")
    string? secretName      // Optional: specific secret name
)

// Dynamic credentials
public static IReadOnlyDictionary<string, string> CreateDynamicParameters(
    string mountPoint,      // Database mount point (e.g., "database/postgres/pg5432")
    string roleName,        // Database role (e.g., "owner")
    string? timeToLive      // Optional: TTL in Vault format (e.g., "1h")
)
```

### HVaultDatabaseSecret

Represents a dynamic database credential:

```csharp
public class HVaultDatabaseSecret : IPvNugsDynamicCredential
{
    public string Username { get; set; }
    public string Password { get; set; }
    public TimeSpan TimeToLive { get; set; }
    public DateTime ExpiresOnUtc { get; set; }
    public DateTime ExpirationDateUtc { get; }  // Alias for ExpiresOnUtc
}
```

### PvNugsHVaultSecretProviderConfig

Configuration options:

```csharp
public class PvNugsHVaultSecretProviderConfig
{
    // Common settings
    public PvNugsHVaultSecretProviderAuthEnu AuthMethod { get; set; }
    public string ServerUrl { get; set; }
    public string TokenFilePath { get; set; }
    public string? Token { get; set; }  // Optional: set programmatically

    // Kubernetes-specific settings
    public string KubeMountPoint { get; set; }
    public string KubeRoleName { get; set; }
    public string KubeNameSpace { get; set; }
}
```

---

## 📐 Architecture Details

### Component Diagram

```
┌─────────────────────────────────────────┐
│   Application Code                      │
└─────────────────┬───────────────────────┘
                  │
                  ▼
┌─────────────────────────────────────────┐
│   IPvNugsSecretManager                  │
│   (Caching Layer)                       │
└─────────────────┬───────────────────────┘
                  │
                  ▼
┌─────────────────────────────────────────┐
│   IPvNugsSecretProvider                 │
│   (HVaultSecretProvider)                │
└─────────────────┬───────────────────────┘
                  │
                  ▼
┌─────────────────────────────────────────┐
│   HashiCorp Vault                       │
│   ├── KV Secrets Engine (v2)           │
│   └── Database Secrets Engine           │
└─────────────────────────────────────────┘
```

### Class Hierarchy

```
HVaultSecretProvider : IPvNugsSecretProvider
    ├── GetStaticSecretsAsync()   → Vault KV v2
    ├── GetStaticSecretAsync()    → Vault KV v2
    └── GetDynamicSecretAsync()   → Vault Database Engine

HVaultDatabaseSecret : IPvNugsDynamicCredential
    ├── Username
    ├── Password
    ├── TimeToLive
    └── ExpirationDateUtc

PvNugsHVaultException : Exception
```

---

## 🛡️ Security Considerations

### 🏭 Production Deployment

1. **Use HTTPS**: Always use `https://` for Vault server URLs in production
2. **Token Security**: Never commit tokens to source control
3. **File Permissions**: Ensure token files have restrictive permissions (chmod 600)
4. **Token Rotation**: Implement regular token rotation policies
5. **Credential TTL**: Set appropriate time-to-live values for dynamic credentials
6. **Network Security**: Restrict Vault access to authorized networks only

### ⚠️ Vault DEV Mode Warning

⚠️ **The Docker test environment runs Vault in DEV mode:**

- Data is stored in memory (not persistent)
- Uses a static root token (`dev-only-token`)
- TLS is disabled
- **NEVER use DEV mode in production**

For production, use:
- Sealed Vault with proper unsealing procedures
- Persistent storage backend (Consul, etcd, cloud storage)
- TLS encryption for all communication
- Proper authentication methods (AppRole, Kubernetes, OIDC)
- High availability configuration

---

## 🔧 Troubleshooting

### Common Issues

**Problem: "Unable to connect to HashiCorp Vault"**

- Verify Vault is running: `docker compose ps`
- Check Vault URL is correct: `http://localhost:8200`
- Test connectivity: `curl http://localhost:8200/v1/sys/health`

**Problem: "Token file not found"**

- Ensure `token.txt` exists in the correct directory
- Verify `TokenFilePath` in configuration matches actual file path
- Check file permissions

**Problem: "PvNugsHVaultException occurred"**

- Check Vault logs: `docker compose logs vault`
- Verify mount points and paths are correct
- Ensure Vault has been provisioned (run the provisioning console)

**Problem: Dynamic credentials fail**

- Verify the database secrets engine is enabled
- Check database connection is configured correctly
- Ensure the role exists: `vault list database/postgres/pg5432/roles`
- Verify PostgreSQL is accessible from Vault container

**Problem: "Sealed Vault"**

- In DEV mode, Vault should auto-unseal
- If using production Vault, you must unseal it manually
- Check Vault status: `curl http://localhost:8200/v1/sys/health`

### Enabling Detailed Logging

Add to `appsettings.json`:

```json
{
  "PvNugsLoggerConfig": {
    "MinLogLevel": "trace"
  }
}
```

This enables verbose logging for all Vault operations.

---

## 📦 Recommended package split

- `pvNugsSecretManagerNc10Abstractions`: interfaces
- `pvNugsSecretManagerNc10`: caching, logging, orchestration
- `pvNugsSecretManagerNc10ProviderHVault`: HashiCorp Vault provider

## 🔗 Links

- **GitHub Repository**: [licheez/pvWayNugs](https://github.com/licheez/pvWayNugs)
- **Test Environment**: [VaultLab Integration Tests](../../Testing/intTesting/pvNugsSecretManagerNc10VaultLab.it/)
- **HashiCorp Vault Documentation**: https://www.vaultproject.io/docs
- **VaultSharp Library**: https://github.com/rajanadar/VaultSharp

## 📄 License

MIT — see `LICENSE`.

---

**Happy Secret Managing! 🔐**

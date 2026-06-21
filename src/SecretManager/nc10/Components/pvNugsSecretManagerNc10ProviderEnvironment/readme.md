# 🔐 pvNugsSecretManagerNc10ProviderEnvironment
[![NuGet Version](https://img.shields.io/nuget/v/pvNugsSecretManagerNc10ProviderEnvironment.svg)](https://www.nuget.org/packages/pvNugsSecretManagerNc10ProviderEnvironment/)
[![NuGet Downloads](https://img.shields.io/nuget/dt/pvNugsSecretManagerNc10ProviderEnvironment.svg)](https://www.nuget.org/packages/pvNugsSecretManagerNc10ProviderEnvironment/)
[![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)
[![.NET](https://img.shields.io/badge/.NET-10.0-blue.svg)](https://dotnet.microsoft.com/download)
Environment Variable and Configuration provider for the pvWay Secret Manager stack on .NET 10. 🚀

This package implements `IPvNugsSecretProvider` and retrieves secrets from any source supported by `Microsoft.Extensions.Configuration`, including environment variables, appsettings.json, user secrets, and command-line arguments.

This package is intended to be used together with:
- `pvNugsSecretManagerNc10` for orchestration, caching, and exception normalization
- an application-defined `IPvNugsSecretManager` consumer such as a `callerService`
## 🏗️ Architecture
The current design is intentionally split into three layers:
1. **Caller service / application code** injects `IPvNugsSecretManager`
2. **Secret manager package** (`pvNugsSecretManagerNc10`) orchestrates calls, caching, and logging
3. **Provider package** (`pvNugsSecretManagerNc10ProviderEnvironment`) reads from configuration sources
So the main application typically registers **both**:
- `IPvNugsSecretManager` via `PvNugsSecretManagerDi.TryAddPvNugsSecretManager(...)`
- one concrete `IPvNugsSecretProvider`, here `EnvVarSecretProvider` via `PvNugsEnvVarSecretProviderDi.TryAddPvNugsEnvVarSecretProvider(...)`
```text
👤 Caller service
    ↓ injects
🔐 IPvNugsSecretManager
    ↓ delegates to
🌍 EnvVarSecretProvider
    ↓ reads from
⚙️  Configuration Sources
    (Environment Variables, appsettings.json, User Secrets, etc.)
```
## ✨ Features
- 🌍 Retrieves secrets from environment variables, JSON files, user secrets, command-line args, and more
- 📝 Optional prefix support for organizing secrets into namespaces
- 🔧 DI-friendly registration
- ⚡ Lightweight and fast - no external service calls
- 🧪 Perfect for development and testing scenarios
- ⚠️ Explicit unsupported-feature behavior for batch and dynamic secret APIs
## ✅ Supported behavior
- ✔️ `GetStaticSecretAsync(...)` retrieves one secret by the canonical parameter key `secretName`
- ❌ `GetStaticSecretsAsync(...)` is not supported and throws `PvNugsEnvVarProviderException`
- ❌ `GetDynamicSecretAsync(...)` is not supported and throws `PvNugsEnvVarProviderException`
Environment variables and configuration files are **static** by nature and do not support dynamic credential rotation or batch secret retrieval like HashiCorp Vault.
## 📦 Installation
```powershell
Install-Package pvNugsSecretManagerNc10ProviderEnvironment
```
Or with the .NET CLI:
```bash
dotnet add package pvNugsSecretManagerNc10ProviderEnvironment
```
## 📚 Dependencies
This package depends on:
- `Microsoft.Extensions.Options.ConfigurationExtensions`
- `pvNugsLoggerNc10Abstractions`
- `pvNugsSecretManagerNc10Abstractions`
The application that uses this provider should also reference:
- `pvNugsSecretManagerNc10`
- one cache implementation and one logger implementation required by the secret manager package
## ⚙️ Configuration

> **🚨 SECURITY BEST PRACTICE**
>
> The JSON examples below are for **illustrative purposes only**. In production environments:
> - **NEVER commit actual secrets to appsettings.json files**
> - **ALWAYS use environment variables** for real production secrets
> - JSON files should only contain placeholders or configuration structure
> - Use `.gitignore` to exclude any local development configuration files with secrets

The provider is bound from the `PvNugsEnvVarSecretProviderConfig` section.

### 🌍 Without prefix (root-level secrets)

```json
{
  "PvNugsEnvVarSecretProviderConfig": {
    "Prefix": null
  },
  // ⚠️ WARNING: These are EXAMPLES ONLY - never commit real secrets!
  "DatabasePassword": "REPLACE-WITH-ENV-VAR",
  "ApiKey": "REPLACE-WITH-ENV-VAR"
}
```

**✅ RECOMMENDED - Set environment variables directly:**
```bash
# Windows
set DatabasePassword=my-dev-password
set ApiKey=my-dev-api-key
# Linux/macOS
export DatabasePassword=my-dev-password
export ApiKey=my-dev-api-key
```
### 📂 With prefix (organized secrets)
```json
{
  "PvNugsEnvVarSecretProviderConfig": {
    "Prefix": "MyApp"
  },
  "MyApp": {
    // ⚠️ WARNING: These are EXAMPLES ONLY - never commit real secrets!
    "DatabasePassword": "REPLACE-WITH-ENV-VAR",
    "ApiKey": "REPLACE-WITH-ENV-VAR"
  }
}
```

**✅ RECOMMENDED - Set environment variables with hierarchical naming:**
```bash
# Windows
set MyApp__DatabasePassword=my-dev-password
set MyApp__ApiKey=my-dev-api-key
# Linux/macOS
export MyApp__DatabasePassword=my-dev-password
export MyApp__ApiKey=my-dev-api-key
```
### 🔧 Multi-environment configuration

Leverage .NET's environment-specific configuration files:

**appsettings.Development.json (local development only - DO NOT COMMIT with real secrets):**
```json
{
  "PvNugsEnvVarSecretProviderConfig": {
    "Prefix": "MyApp_Dev"
  },
  "MyApp_Dev": {
    // ⚠️ For local dev only - use User Secrets or local env vars
    "DatabasePassword": "local-dev-only",
    "ApiKey": "local-dev-only"
  }
}
```

**appsettings.Production.json (✅ BEST PRACTICE - no secrets in file):**
```json
{
  "PvNugsEnvVarSecretProviderConfig": {
    "Prefix": "MyApp_Prod"
  }
  // ✅ CORRECT: Secrets come from environment variables in production
  // ✅ NEVER include actual secret values in this file
  // ✅ Set environment variables:
  //    MyApp_Prod__DatabasePassword=<real-secret>
  //    MyApp_Prod__ApiKey=<real-secret>
}
```
## 🔧 Service registration
```csharp
using pvNugsSecretManagerNc10;
using pvNugsSecretManagerNc10Abstractions;
using pvNugsSecretManagerNc10ProviderEnvironment;
var builder = WebApplication.CreateBuilder(args);
// Register the provider first
builder.Services.TryAddPvNugsEnvVarSecretProvider(builder.Configuration);
// Register the provider-agnostic manager
builder.Services.TryAddPvNugsSecretManager(builder.Configuration);
var app = builder.Build();
```
## 💡 Usage
Your caller service should depend on `IPvNugsSecretManager`, not on the provider directly.
You can use the helper `PvNugsEnvVarSecretProviderParameters.CreateParameters(...)`
to avoid repeating dictionary boilerplate.
```csharp
using pvNugsSecretManagerNc10Abstractions;
using pvNugsSecretManagerNc10ProviderEnvironment;
public class CallerService(IPvNugsSecretManager secretManager)
{
    public async Task<string?> GetDatabasePasswordAsync(CancellationToken ct = default)
    {
        var parameters = PvNugsEnvVarSecretProviderParameters
            .CreateParameters("DatabasePassword");
        return await secretManager.GetStaticSecretAsync(parameters, ct);
    }
    public async Task<string?> GetApiKeyAsync(CancellationToken ct = default)
    {
        var parameters = PvNugsEnvVarSecretProviderParameters
            .CreateParameters("ApiKey");
        return await secretManager.GetStaticSecretAsync(parameters, ct);
    }
}
```
## 📝 Parameter contract
The canonical key for single-secret retrieval is:
```csharp
PvNugsEnvVarSecretProviderParameters.SecretName // "secretName"
```
The key is intentionally explicit so callers can build the dictionary once and reuse it consistently.
Helper available:
```csharp
PvNugsEnvVarSecretProviderParameters.CreateParameters("DatabasePassword")
```
The helper validates input and returns a read-only dictionary containing the canonical
`"secretName"` key/value pair.
## 🔍 Configuration precedence
The provider follows standard .NET configuration precedence (last wins):
1. **appsettings.json** (lowest priority)
2. **appsettings.{Environment}.json**
3. **User secrets** (development only)
4. **Environment variables**
5. **Command-line arguments** (highest priority)
This means environment variables will override values from appsettings.json.
## 💡 Use cases
### ✅ Perfect for:
- 🧪 **Development and testing** - Quick setup without external dependencies
- 🐳 **Container deployments** - Inject secrets via environment variables
- 🔒 **User Secrets** - Local development with sensitive data
- ⚡ **Simple applications** - No need for complex secret management infrastructure
- 🔄 **Provider fallback** - Graceful degradation when external secret services are unavailable
### ⚠️ Not recommended for:
- 🔄 **Dynamic credentials** - Use Azure provider or HashiCorp Vault for rotating credentials
- 🏢 **Enterprise production** - Consider Azure Key Vault, AWS Secrets Manager, or HashiCorp Vault
- 📦 **Batch secret retrieval** - This provider only supports single-secret access
## ⚠️ Notes on unsupported APIs
This provider does not support:
- Secret dictionaries by mount/path
- Dynamic database credential generation
- Secret rotation or expiration
Unsupported operations fail fast with `PvNugsEnvVarProviderException` so callers can handle the limitation explicitly.
## 🔐 Security considerations

### ⛔ CRITICAL: Never Commit Secrets to Version Control

**This is the #1 security rule when using this provider:**

- ❌ **NEVER** store real secrets in `appsettings.json`, `appsettings.Production.json`, or ANY file under version control
- ❌ **NEVER** commit files containing actual passwords, API keys, connection strings, or tokens to Git
- ❌ **NEVER** store production secrets in configuration files that could be accidentally exposed

### ✅ RECOMMENDED Secret Storage Methods

**For Production Environments:**
1. **Environment Variables** (Primary recommendation for this provider)
   - Set at the operating system level
   - Injected by deployment platform (Azure App Service, Kubernetes, Docker, etc.)
   - Never stored in files or version control

2. **Azure Key Vault Provider** (Recommended for enterprise)
   - Use `pvNugsSecretManagerNc10ProviderAzure` instead of this provider
   - Proper secret rotation and auditing
   - Better for production environments

**For Development Environments:**
1. **User Secrets** (`dotnet user-secrets`)
   - Stored outside project directory
   - Automatically excluded from source control
   - Only for local development

2. **Local Environment Variables**
   - Set in your development machine
   - Not committed to version control

### 🐳 Container & Cloud Deployment Best Practices

- **Containers (Docker/Kubernetes)**: Inject secrets via environment variables or mounted volumes
- **Azure App Service**: Use App Settings (environment variables) or Key Vault references
- **AWS**: Use Systems Manager Parameter Store or Secrets Manager
- **Cloud platforms**: Use platform-native secret injection mechanisms

### 📋 Configuration File Guidelines

If you must use JSON configuration files:

✅ **DO:**
- Store only non-sensitive configuration
- Use placeholder values like `"REPLACE-WITH-ENV-VAR"` or `"SET-VIA-ENVIRONMENT"`
- Document which values need to be set via environment variables
- Add `appsettings.*.local.json` to `.gitignore`

❌ **DON'T:**
- Store any production secrets
- Commit local development files with real credentials
- Share configuration files containing sensitive data

### 🚨 If Secrets Are Accidentally Committed

If you accidentally commit secrets to version control:

1. **Immediately rotate** all exposed secrets
2. **Do NOT** just delete the file and commit - the secret is still in Git history
3. Consider the secret **permanently compromised**
4. Use tools like `git-secrets` or `truffleHog` to scan repositories
5. Review your repository's entire Git history

### 🔐 Additional Security Measures

- **Principle of Least Privilege**: Only grant access to secrets that are absolutely necessary
- **Regular Rotation**: Change secrets periodically even if not compromised
- **Monitoring**: Log secret access attempts and investigate anomalies
- **Encryption at Rest**: Ensure storage systems encrypt data
- **CI/CD Secrets**: Use GitHub Secrets, Azure DevOps Variable Groups, or similar secure storage
## 📦 Recommended package split
- `pvNugsSecretManagerNc10Abstractions`: interfaces
- `pvNugsSecretManagerNc10`: caching, logging, orchestration
- `pvNugsSecretManagerNc10ProviderEnvironment`: Environment Variable / Configuration provider
- `pvNugsSecretManagerNc10ProviderAzure`: Azure Key Vault provider (for production)
## 🔗 Related packages
- **[pvNugsSecretManagerNc10](https://www.nuget.org/packages/pvNugsSecretManagerNc10/)** - Core secret manager orchestration
- **[pvNugsSecretManagerNc10ProviderAzure](https://www.nuget.org/packages/pvNugsSecretManagerNc10ProviderAzure/)** - Azure Key Vault provider
- **[pvNugsCacheNc10Memory](https://www.nuget.org/packages/pvNugsCacheNc10Memory/)** - In-memory cache implementation
- **[pvNugsLoggerNc10](https://www.nuget.org/packages/pvNugsLoggerNc10/)** - Logging abstractions
## 📄 License
MIT — see `LICENSE`.

> **⚠️ CRITICAL SECURITY WARNING**
>
> **DO NOT store actual secrets in appsettings.json or any files under version control!**
>
> This provider should **primarily use ENVIRONMENT VARIABLES** for production secrets. While it can read from JSON configuration files, those should only contain non-sensitive configuration or be used for local development with placeholder values.
>
> ✅ **RECOMMENDED:** Environment variables, User Secrets (dev only), Azure Key Vault
>
> ❌ **NEVER:** Real secrets in appsettings.json files committed to Git/source control


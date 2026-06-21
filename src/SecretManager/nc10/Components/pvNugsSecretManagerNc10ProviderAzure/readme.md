# 🔐 pvNugsSecretManagerNc10ProviderAzure

[![NuGet Version](https://img.shields.io/nuget/v/pvNugsSecretManagerNc10ProviderAzure.svg)](https://www.nuget.org/packages/pvNugsSecretManagerNc10ProviderAzure/)
[![NuGet Downloads](https://img.shields.io/nuget/dt/pvNugsSecretManagerNc10ProviderAzure.svg)](https://www.nuget.org/packages/pvNugsSecretManagerNc10ProviderAzure/)
[![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)
[![.NET](https://img.shields.io/badge/.NET-10.0-blue.svg)](https://dotnet.microsoft.com/download)

Azure Key Vault provider for the pvWay Secret Manager stack on .NET 10. 🚀

This package implements `IPvNugsSecretProvider` and is intended to be used together with:

- `pvNugsSecretManagerNc10` for orchestration, caching, and exception normalization
- an application-defined `IPvNugsSecretManager` consumer such as a `callerService`

## 🏗️ Architecture

The current design is intentionally split into three layers:

1. **Caller service / application code** injects `IPvNugsSecretManager`
2. **Secret manager package** (`pvNugsSecretManagerNc10`) orchestrates calls, caching, and logging
3. **Provider package** (`pvNugsSecretManagerNc10ProviderAzure`) talks to Azure Key Vault

So the main application typically registers **both**:

- `IPvNugsSecretManager` via `PvNugsSecretManagerDi.TryAddPvNugsSecretManager(...)`
- one concrete `IPvNugsSecretProvider`, here `AzureSecretProvider` via `PvNugsAzureSecretProviderDi.TryAddPvNugsAzureSecretProvider(...)`

```text
👤 Caller service
    ↓ injects
🔐 IPvNugsSecretManager
    ↓ delegates to
🔷 AzureSecretProvider
    ↓ talks to
☁️  Azure Key Vault
```

## ✨ Features

- 🔑 Azure Key Vault single-secret retrieval
- 👤 Managed Identity or Service Principal authentication
- 🔧 DI-friendly registration
- ⚠️ Explicit unsupported-feature behavior for batch and dynamic secret APIs

## ✅ Supported behavior

- ✔️ `GetStaticSecretAsync(...)` retrieves one secret by the canonical parameter key `secretName`
- ❌ `GetStaticSecretsAsync(...)` is not supported and throws `PvNugsAzureProviderException`
- ❌ `GetDynamicSecretAsync(...)` is not supported and throws `PvNugsAzureProviderException`

Azure Key Vault does **not** expose Vault-style dynamic credential leasing, so this provider keeps that limitation explicit instead of simulating it.

## 📦 Installation

```powershell
Install-Package pvNugsSecretManagerNc10ProviderAzure
```

Or with the .NET CLI:

```bash
dotnet add package pvNugsSecretManagerNc10ProviderAzure
```

## 📚 Dependencies

This package depends on:

- `Azure.Identity`
- `Azure.Security.KeyVault.Secrets`
- `pvNugsLoggerNc10Abstractions`
- `pvNugsSecretManagerNc10Abstractions`

The application that uses this provider should also reference:

- `pvNugsSecretManagerNc10`
- one cache implementation and one logger implementation required by the secret manager package

## ⚙️ Configuration

The provider is bound from the `PvNugsAzureSecretProviderConfig` section.

### 👤 Managed Identity

```json
{
  "PvNugsAzureSecretProviderConfig": {
    "KeyVaultUrl": "https://your-vault.vault.azure.net/",
    "Credential": null
  }
}
```

### 🔑 Service Principal

```json
{
  "PvNugsAzureSecretProviderConfig": {
    "KeyVaultUrl": "https://your-vault.vault.azure.net/",
    "Credential": {
      "TenantId": "12345678-1234-1234-1234-123456789012",
      "ClientId": "87654321-4321-4321-4321-210987654321",
      "ClientSecret": "your-client-secret"
    }
  }
}
```

## 🔧 Service registration

```csharp
using pvNugsSecretManagerNc10;
using pvNugsSecretManagerNc10Abstractions;
using pvNugsSecretManagerNc10ProviderAzure;

var builder = WebApplication.CreateBuilder(args);

// Register the provider first
builder.Services.TryAddPvNugsAzureSecretProvider(builder.Configuration);

// Register the provider-agnostic manager
builder.Services.TryAddPvNugsSecretManager(builder.Configuration);

var app = builder.Build();
```

## 💡 Usage

Your caller service should depend on `IPvNugsSecretManager`, not on the Azure provider directly.

If the caller is Azure-aware, you can use the helper `PvNugsAzureSecretProviderParameters.CreateParameters(...)`
to avoid repeating dictionary boilerplate.

```csharp
using pvNugsSecretManagerNc10Abstractions;
using pvNugsSecretManagerNc10ProviderAzure;

public class CallerService(IPvNugsSecretManager secretManager)
{
    public async Task<string?> GetDatabasePasswordAsync(CancellationToken ct = default)
    {
        var parameters = PvNugsAzureSecretProviderParameters
            .CreateParameters("database-password");

        return await secretManager.GetStaticSecretAsync(parameters, ct);
    }
}
```

## 📝 Parameter contract

The canonical key for single-secret retrieval is:

```csharp
PvNugsAzureSecretProviderParameters.SecretName // "secretName"
```

The key is intentionally explicit so callers can build the dictionary once and reuse it consistently.

Helper available for Azure-aware callers:

```csharp
PvNugsAzureSecretProviderParameters.CreateParameters("database-password")
```

The helper validates input and returns a read-only dictionary containing the canonical
`"secretName"` key/value pair.

## ⚠️ Notes on unsupported APIs

This provider does not mimic HashiCorp Vault semantics for:

- secret dictionaries by mount/path
- dynamic database credential generation

Instead, unsupported operations fail fast with a provider-specific exception so callers can handle the Azure limitation explicitly.

## 📦 Recommended package split

- `pvNugsSecretManagerNc10Abstractions`: interfaces
- `pvNugsSecretManagerNc10`: caching, logging, orchestration
- `pvNugsSecretManagerNc10ProviderAzure`: Azure Key Vault provider

## 📄 License

MIT — see `LICENSE`.

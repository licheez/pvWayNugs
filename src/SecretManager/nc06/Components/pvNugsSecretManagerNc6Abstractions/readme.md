# pvNugs Secret Manager NC6 Abstractions

[![NuGet Version](https://img.shields.io/nuget/v/pvNugsSecretManagerNc6Abstractions.svg)](https://www.nuget.org/packages/pvNugsSecretManagerNc6Abstractions/)
[![.NET](https://img.shields.io/badge/.NET-6.0-blue.svg)](https://dotnet.microsoft.com/download/dotnet/6.0)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

A lightweight .NET 6 (LTS) abstraction library for secret management. This package exposes interfaces and contracts suitable for implementing providers that integrate with secret stores (Azure Key Vault, environment variables, vaults, etc.). It is the NC6-targeted sibling of the NC9 abstractions and intentionally keeps the API surface compatible with projects targeting .NET 6.

## 🔐 Features

- Clear, minimal interfaces for static and dynamic secret retrieval
- CancellationToken-aware methods for graceful shutdown and testing
- Designed for easy DI registration and unit testing
- Portable: implement your provider for Azure, AWS, HashiCorp, or custom stores
- Focus on correctness and thread-safety for common usage patterns

## 📦 Installation

```bash
dotnet add package pvNugsSecretManagerNc6Abstractions
```

Or via Package Manager Console:

```powershell
Install-Package pvNugsSecretManagerNc6Abstractions
```

## 🚀 Quick Start

- Register your implementation with the DI container in a .NET 6 application (Program.cs):

```csharp
// Program.cs (.NET 6 minimal host)
var builder = WebApplication.CreateBuilder(args);

// register your implementation
builder.Services.AddSingleton<IPvNugsStaticSecretManager, YourSecretManagerImplementation>();

var app = builder.Build();
app.Run();
```

### Static Secret Retrieval (example)

```csharp
public class DatabaseService
{
    private readonly IPvNugsStaticSecretManager _secretManager;

    public DatabaseService(IPvNugsStaticSecretManager secretManager)
    {
        _secretManager = secretManager;
    }

    public async Task<string> GetConnectionStringAsync(CancellationToken ct)
    {
        var password = await _secretManager.GetStaticSecretAsync("database-password", ct);
        return $"Server=myserver;Database=mydb;Password={password};";
    }
}
```

### Dynamic Credential Retrieval (example)

```csharp
public class SecureDataService
{
    private readonly IPvNugsDynamicSecretManager _secretManager;

    public SecureDataService(IPvNugsDynamicSecretManager secretManager)
    {
        _secretManager = secretManager;
    }

    public async Task UseDynamicCredentialAsync(CancellationToken ct)
    {
        var credential = await _secretManager.GetDynamicSecretAsync("app-db", ct);
        if (credential == null || DateTime.UtcNow >= credential.ExpirationDateUtc)
            throw new InvalidOperationException("Unable to obtain valid credentials");

        // use credential.Username and credential.Password
    }
}
```

## 🔧 Dependency Injection Setup

```csharp
// Program.cs (register implementations)
services.AddSingleton<IPvNugsStaticSecretManager, YourSecretManagerImplementation>();
services.AddSingleton<IPvNugsDynamicSecretManager, YourDynamicSecretManagerImplementation>();
```

## 🛡️ Security Best Practices

- Never log secrets in plaintext
- Use managed identities or service principals when possible
- Cache secrets carefully and respect expiration for dynamic credentials
- Protect memory containing secrets and clear when finished

## 📋 Use Cases

- Centralized static secret retrieval (API keys, connection strings)
- Dynamic, short-lived database credentials
- Multi-tenant or ephemeral environments needing credential rotation

## 🎯 Target Framework

- **.NET 6 (LTS)** — designed for projects still running on .NET 6 while keeping the API consistent with the pvNugs ecosystem.

## 📚 Documentation

The package includes XML documentation for all public interfaces and members. See the repository for example implementations and tests.

## 🤝 Contributing

Part of the pvWayNugs ecosystem. Issues and pull requests are welcome: https://github.com/licheez/pvWayNugs

## 📄 License

MIT — see the LICENSE file for details.

---

**Keywords**: Secret Management, .NET 6, Abstractions, Dynamic Credentials, Static Secrets, pvWayNugs

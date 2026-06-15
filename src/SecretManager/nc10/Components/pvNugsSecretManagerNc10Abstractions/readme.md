# pvNugsSecretManagerNc10Abstractions

[![NuGet Version](https://img.shields.io/nuget/v/pvNugsSecretManagerNc10Abstractions.svg)](https://www.nuget.org/packages/pvNugsSecretManagerNc10Abstractions/)
[![.NET](https://img.shields.io/badge/.NET%20Core-10.0-blue.svg)](https://dotnet.microsoft.com/download)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

`pvNugsSecretManagerNc10Abstractions` is the contract package for the pvWay Secret Manager architecture on .NET Core 10.

It defines the interfaces used by:

- the **Secret Manager** orchestration layer (caching, cross-cutting concerns)
- one **provider implementation** (Azure Key Vault, environment variables, HashiCorp Vault, and others)
- the **consumer application** that requests secrets without coupling to a specific backend

## Features

- Static secret retrieval contracts
- Dynamic credential retrieval contracts (username/password + expiration)
- Provider-agnostic API surface
- Dictionary-based parameter model for maximum provider flexibility
- Cancellation token support on all async operations
- Rich XML documentation for IntelliSense and generated API docs

## Installation

```bash
dotnet add package pvNugsSecretManagerNc10Abstractions
```

Package Manager Console:

```powershell
Install-Package pvNugsSecretManagerNc10Abstractions
```

## Target Framework

- .NET Core 10.0+

## Core Interfaces

### `IPvNugsSecretManager`

Consumer-facing entry point to retrieve:

- multiple static secrets (`GetStaticSecretsAsync`)
- a single static secret (`GetStaticSecretAsync`)
- a dynamic credential (`GetDynamicSecretAsync`)

### `IPvNugsSecretProvider`

Provider-facing contract implemented by backend adapters (Azure, env vars, HashiCorp Vault, etc.).

### `IPvNugsDynamicCredential`

Represents short-lived access material:

- `Username`
- `Password`
- `ExpirationDateUtc`

## Parameter Dictionary Model

All retrieval methods use:

```csharp
IReadOnlyDictionary<string, string> parameters
```

This allows each provider to define its own contract. Examples:

- Azure provider: could require only `name`
- HashiCorp Vault provider: could require `mountPoint`, `path`, `key`, `roleName`

The selected provider validates the dictionary and documents required/optional keys.

## Quick Start (Consumer)

```csharp
var parameters = new Dictionary<string, string>
{
    ["name"] = "database-password"
};

var secret = await secretManager.GetStaticSecretAsync(parameters, cancellationToken);
```

Dynamic credential example:

```csharp
var parameters = new Dictionary<string, string>
{
    ["name"] = "app-database"
};

var credential = await secretManager.GetDynamicSecretAsync(parameters, cancellationToken);

if (credential == null || DateTime.UtcNow >= credential.ExpirationDateUtc)
    throw new InvalidOperationException("Unable to obtain valid credentials.");
```

## Dependency Injection Example

The abstraction package only contains contracts. Typical DI wiring is done in the implementation packages:

```csharp
services.AddSingleton<IPvNugsSecretProvider, YourProviderImplementation>();
services.AddSingleton<IPvNugsSecretManager, YourSecretManagerImplementation>();
```

## Security Guidance

- Do not log secret values or raw credential payloads
- Validate dynamic credential expiration before use
- Use secure transport/authentication to the backing secret store
- Keep provider validation strict (required keys, formats, allowed values)
- Prefer least-privilege access policies in the backend system

## Architecture Overview

Three packages are typically used together:

1. `pvNugsSecretManagerNc10Abstractions`
2. `pvNugsSecretManagerNc10`
3. One provider package (Azure / Environment Variables / HashiCorp Vault / AWS)

This split keeps consumers independent from backend details while preserving extensibility.

## License

MIT - see `LICENSE` in the repository.

# pvNugsCryptoNc6Aes

[![License: MIT](https://img.shields.io/badge/license-MIT-blue.svg)](https://opensource.org/licenses/MIT)
[![NuGet](https://img.shields.io/badge/nuget-pvNugsCryptoNc6Aes-lightgrey?logo=nuget&logoColor=white)](https://www.nuget.org)
[![Target Framework](https://img.shields.io/badge/.NET-6.0-blue.svg)](https://dotnet.microsoft.com)

Lightweight AES-based implementation for the pvNugs crypto abstraction (IPvNugsCrypto) targeting .NET 6.

Why use this package

- 🔐 AES encryption/decryption (configurable key + IV)
- ⏳ Built-in support for ephemeral (time-limited) payloads
- ⚡ Async-first API suitable for use in modern DI-enabled apps
- ♻️ Proper disposal (IDisposable / IAsyncDisposable) and consistent error wrapping

Quick install

```bash
dotnet add package pvNugsCryptoNc6Aes
```

Basic usage (DI)

```csharp
// Startup / Program.cs
services.TryAddPvNugsCryptoAes(Configuration);

// In a consuming class
public class MyService
{
    private readonly IPvNugsCrypto _crypto;

    public MyService(IPvNugsCrypto crypto)
    {
        _crypto = crypto;
    }

    public async Task Demo()
    {
        var encrypted = await _crypto.EncryptStringAsync("hello");
        var decrypted = await _crypto.DecryptStringAsync(encrypted);
        Console.WriteLine(decrypted);
    }
}
```

Ephemeral payloads example

```csharp
// create a time-limited ciphertext (5 minutes)
var ephemeral = await _crypto.EncryptEphemeralStringAsync("one-time", TimeSpan.FromMinutes(5));
// decrypt: will return null if expired
var value = await _crypto.DecryptEphemeralStringAsync(ephemeral);
```

Public surface (summary)

- IPvNugsCrypto (public abstraction provided by the crypto family)
  - EncryptStringAsync / DecryptStringAsync
  - EncryptObjectAsync<T> / DecryptObjectAsync<T>
  - EncryptEphemeralStringAsync / DecryptEphemeralStringAsync
  - EncryptEphemeralObjectAsync<T> / DecryptEphemeralObjectAsync<T>

Features

- 🔐 AES-256 (expected key length: 32 bytes)
- 🔁 JSON object serialization via System.Text.Json
- 🧾 Ephemeral payloads carry a UTC expiry timestamp
- 🛡️ Errors are wrapped in PvWayCryptoException and logged via the injected logger

Suggested NuGet tags

cryptography, encryption, aes, data-protection, ephemeral, pvNugs, security, dotnet6

Security notes

- Never check keys or IVs into source control.
- Prefer reading secrets from a secure store (Azure Key Vault, environment variables, Secret Manager) and bind them to `PvNugsCryptoAesConfig`.
- Use short DefaultValidity for sensitive payloads.

Contributing

Contributions welcome — open issues or PRs in the repository. For security-sensitive reports, use a private channel/bug bounty if necessary.

License

MIT — see the LICENSE file in the repository.

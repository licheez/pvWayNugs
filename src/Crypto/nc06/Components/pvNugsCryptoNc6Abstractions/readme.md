# pvNugsCryptoNc6Abstractions

[![License: MIT](https://img.shields.io/badge/license-MIT-blue.svg)](https://opensource.org/licenses/MIT)
[![NuGet](https://img.shields.io/badge/nuget-pvNugsCryptoNc6Abstractions-lightgrey?logo=nuget&logoColor=white)](https://www.nuget.org/packages/pvNugsCryptoNc6Abstractions)
[![Target Framework](https://img.shields.io/badge/.NET-6.0-blue)](https://dotnet.microsoft.com)

A small, dependency-light abstraction for cryptographic helpers used across the pvNugs family of libraries. It provides async-friendly operations to encrypt and decrypt strings and serializable objects, including support for ephemeral (time-limited) payloads.

Why this package?

- Secure, easy-to-use abstraction for encryption needs.
- Ephemeral payloads with optional validity.
- Minimal surface area — ideal for injecting into DI containers.

Icons / Quick Feature List

- 🔐 Encryption & Decryption
- ⏳ Ephemeral (time-limited) payloads
- ⚡ Async / Task-based API
- ♻️ Supports IDisposable and IAsyncDisposable for resource cleanup

Installing

Install from NuGet:

```bash
dotnet add package pvNugsCryptoNc6Abstractions
```

Usage (minimal example)

```csharp
using pvNugsCryptoNc6Abstractions;

// Resolve ICrypto from your DI container, or create your implementation.
public async Task ExampleAsync(ICrypto crypto)
{
    // Encrypt a string
    string encrypted = await crypto.EncryptStringAsync("Hello world");

    // Decrypt back
    string decrypted = await crypto.DecryptStringAsync(encrypted);

    Console.WriteLine(decrypted); // -> "Hello world"

    // Ephemeral example (may return null if expired)
    string ephemeral = await crypto.EncryptEphemeralStringAsync("temp", TimeSpan.FromMinutes(5));
    string? maybe = await crypto.DecryptEphemeralStringAsync(ephemeral);
}
```

API summary

- ICrypto — primary abstraction. Methods:
  - EncryptStringAsync, EncryptObjectAsync<T>
  - EncryptEphemeralStringAsync, EncryptEphemeralObjectAsync<T>
  - DecryptStringAsync, DecryptObjectAsync<T>
  - DecryptEphemeralStringAsync, DecryptEphemeralObjectAsync<T>

Packaging notes (suggested csproj additions)

To include this readme on nuget.org and provide a package icon, add the following to your component's .csproj:

```xml
<!-- Add inside a PropertyGroup -->
<PackageId>pvNugsCryptoNc6Abstractions</PackageId>
<Description>Lightweight abstractions for encryption and ephemeral payloads used by pvNugs components.</Description>
<Authors>pvWay</Authors>
<PackageTags>cryptography;encryption;ephemeral;data-protection;pvNugs</PackageTags>
<PackageLicenseExpression>MIT</PackageLicenseExpression>
<!-- Include a package icon file (add the file to the project and set <Include> to pack it) -->
<PackageIcon>pvwayLogoTextLess256.png</PackageIcon>
<PackageReadmeFile>readme.md</PackageReadmeFile>
<RepositoryUrl>https://github.com/pvWay/pvNugs</RepositoryUrl>
```

Make sure the package icon file is included in the project and marked to be packed. Example ItemGroup:

```xml
<ItemGroup>
  <None Include="..\..\..\..\..\pvwayLogoTextLess256.png" Pack="true" PackagePath="" />
</ItemGroup>
```

Suggested nuget tags

cryptography, encryption, ephemeral, data-protection, pvNugs, security, async

Contributing & Support

Contributions welcome. Please open issues or pull requests in the repository. For security-sensitive issues, use a secure channel rather than posting secrets in issue trackers.

License

This project is licensed under the MIT License - see the LICENSE file for details.

# pvNugs Secret Manager NC9 Abstractions

[![NuGet Version](https://img.shields.io/nuget/v/pvNugsSecretManagerNc9Abstractions.svg)](https://www.nuget.org/packages/pvNugsSecretManagerNc9Abstractions/)
[![.NET](https://img.shields.io/badge/.NET-9.0-blue.svg)](https://dotnet.microsoft.com/download)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

A comprehensive .NET 9.0 abstraction library for secure secret management with support for both static and dynamic credentials. This package provides interfaces and contracts for integrating with various secret management systems including Azure Key Vault, AWS Secrets Manager, and HashiCorp Vault.

## 🔐 Features

- **Static Secret Management**: Retrieve persistent secrets like API keys, connection strings, and passwords
- **Dynamic Credential Management**: Generate temporary, time-limited database credentials with automatic expiration
- **Multiple Provider Support**: Designed for Azure Key Vault, AWS Secrets Manager, HashiCorp Vault, and more
- **Thread-Safe Operations**: Full support for concurrent access across multiple application threads
- **Cancellation Support**: Proper cancellation token handling for graceful shutdowns
- **Comprehensive Documentation**: Extensive XML documentation with examples and best practices

## 📦 Installation

```bash
dotnet add package pvNugsSecretManagerNc9Abstractions
```
```


Or via Package Manager Console:

```textmate
Install-Package pvNugsSecretManagerNc9Abstractions
```


## 🚀 Quick Start

### Static Secret Management

```csharp
public class DatabaseService
{
    private readonly IPvNugsStaticSecretManager _secretManager;

    public DatabaseService(IPvNugsStaticSecretManager secretManager)
    {
        _secretManager = secretManager;
    }

    public async Task<string> GetConnectionStringAsync()
    {
        var password = await _secretManager.GetStaticSecretAsync("database-password");
        return $"Server=myserver;Database=mydb;Password={password};";
    }
}
```


### Dynamic Credential Management

```csharp
public class SecureDataService
{
    private readonly IPvNugsDynamicSecretManager _secretManager;

    public SecureDataService(IPvNugsDynamicSecretManager secretManager)
    {
        _secretManager = secretManager;
    }

    public async Task<List<User>> GetUsersAsync()
    {
        var credential = await _secretManager.GetDynamicSecretAsync("app-database");
        
        if (credential == null || DateTime.UtcNow >= credential.ExpirationDateUtc)
            throw new InvalidOperationException("Unable to obtain valid credentials");

        var connectionString = $"Server=myserver;Username={credential.Username};Password={credential.Password};";
        
        // Use connection for database operations...
        return users;
    }
}
```


## 🏗️ Core Interfaces

### IPvNugsStaticSecretManager

Provides access to persistent secrets stored in external secret management systems.

**Key Features:**
- Retrieve static secrets by name
- Thread-safe operations
- Caching support
- Proper error handling

### IPvNugsDynamicSecretManager

Extends static secret management with dynamic credential generation capabilities.

**Key Features:**
- Time-limited credentials
- Automatic credential rotation
- Enhanced security through temporary access
- Zero persistent storage

### IPvNugsDynamicCredential

Represents a temporary credential with automatic expiration.

**Properties:**
- `Username`: Dynamically generated username
- `Password`: Cryptographically secure password
- `ExpirationDateUtc`: Precise expiration timestamp

## 🔧 Dependency Injection Setup

```csharp
// Program.cs or Startup.cs
services.AddSingleton<IPvNugsStaticSecretManager, YourSecretManagerImplementation>();
services.AddSingleton<IPvNugsDynamicSecretManager, YourDynamicSecretManagerImplementation>();
```


## 🛡️ Security Best Practices

### Static Secrets
- Never log secret values in plain text
- Use secure communication channels (HTTPS/TLS)
- Implement proper caching with security considerations
- Use managed identities or service accounts for authentication

### Dynamic Credentials
- Monitor expiration times and renew proactively
- Implement credential renewal before expiration (recommended: 10-25% of lifetime)
- Handle concurrent renewal operations safely
- Clear credentials from memory when no longer needed

## 📋 Use Cases

### Static Secret Management
- Database passwords and connection strings
- API keys for external services
- Encryption keys and certificates
- Third-party service credentials
- Sensitive configuration values

### Dynamic Credentials
- Production database access with temporary users
- Multi-tenant applications requiring credential isolation
- Compliance environments with mandatory credential rotation
- Cloud-native applications using managed database services
- Zero-trust security architectures

## 🔄 Integration Examples

### With Configuration System

```csharp
services.Configure<DatabaseOptions>(async options =>
{
    var secretManager = serviceProvider.GetRequiredService<IPvNugsStaticSecretManager>();
    options.Password = await secretManager.GetStaticSecretAsync("db-password");
});
```


### Error Handling

```csharp
public async Task<ApiClient> CreateApiClientAsync(CancellationToken cancellationToken)
{
    try
    {
        var apiKey = await _secretManager.GetStaticSecretAsync("external-api-key", cancellationToken);
        if (apiKey == null)
            throw new InvalidOperationException("API key not found");
            
        return new ApiClient(apiKey);
    }
    catch (OperationCanceledException)
    {
        _logger.LogInformation("Secret retrieval was cancelled");
        throw;
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Failed to retrieve API key");
        throw;
    }
}
```


## 🎯 Target Framework

- **.NET 9.0**: Built specifically for the latest .NET platform with modern language features

## 📚 Documentation

The package includes comprehensive XML documentation with:
- Detailed interface descriptions
- Method parameter explanations
- Usage examples and best practices
- Security considerations and guidelines
- Integration patterns and common use cases

## 🤝 Contributing

This package is part of the pvWayNugs ecosystem. For issues, suggestions, or contributions, please visit the [GitHub repository](https://github.com/licheez/pvWayNugs).

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](https://opensource.org/licenses/MIT) file for details.

## 🏢 About pvWay Ltd

pvWay Ltd specializes in secure, enterprise-grade .NET solutions with a focus on security, reliability, and developer experience.

---

**Keywords**: Secret Management, Security, Azure Key Vault, AWS Secrets Manager, HashiCorp Vault, Dynamic Credentials, .NET 9, Abstractions, pvWayNugs

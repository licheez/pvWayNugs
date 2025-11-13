# PvNugs Azure Key Vault Secret Manager

[![NuGet Version](https://img.shields.io/nuget/v/pvNugsSecretManagerNc6Azure.svg)](https://www.nuget.org/packages/pvNugsSecretManagerNc6Azure/)
[![NuGet Downloads](https://img.shields.io/nuget/dt/pvNugsSecretManagerNc6Azure.svg)](https://www.nuget.org/packages/pvNugsSecretManagerNc6Azure/)
[![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)
[![.NET](https://img.shields.io/badge/.NET-6.0-blue.svg)](https://dotnet.microsoft.com/download/dotnet/6.0)

A robust, production-ready Azure Key Vault integration library for .NET applications that provides secure secret management with built-in caching, comprehensive logging, and support for both Managed Identity and Service Principal authentication.

## ✨ Features

- 🔐 **Secure Secret Retrieval**: Direct integration with Azure Key Vault for secure secret storage and retrieval
- 🚀 **Performance Optimized**: Built-in caching layer to minimize API calls and improve response times
- 🔑 **Flexible Authentication**: Support for both Azure Managed Identity (recommended) and Service Principal authentication
- 📊 **Comprehensive Logging**: Detailed logging integration with customizable log levels
- ⚡ **Async/Await Support**: Fully asynchronous operations with cancellation token support
- 🛡️ **Exception Handling**: Robust error handling with detailed exception information
- 🏗️ **Dependency Injection**: Seamless integration with .NET's built-in dependency injection container
- 🔧 **Configuration Driven**: Flexible configuration through .NET's IConfiguration system
- 🧪 **Thread Safe**: Designed for concurrent access in multi-threaded applications

## 📦 Installation

Install the package via NuGet Package Manager:
```
bash
dotnet add package pvNugsSecretManagerNc6Azure
```
Or via Package Manager Console:
```
powershell
Install-Package pvNugsSecretManagerNc6Azure
```
## 🔧 Dependencies

This package requires the following companion packages:
```
bash
# Cache provider (required)
dotnet add package pvNugsCacheNc6Local

# Logger service (required)
dotnet add package pvNugsLoggerNc6Seri
```
## 🚀 Quick Start

### 1. Configuration Setup

#### Using Managed Identity (Recommended for Azure environments)

**appsettings.json:**
```
json
{
"PvNugsAzureSecretManagerConfig": {
"KeyVaultUrl": "https://your-keyvault.vault.azure.net/"
}
}
```
#### Using Service Principal (for local development or non-Azure environments)

**appsettings.json:**
```
json
{
  "PvNugsAzureSecretManagerConfig": {
  "KeyVaultUrl": "https://your-keyvault.vault.azure.net/",
  "Credential": {
    "TenantId": "12345678-1234-1234-1234-123456789012",
    "ClientId": "87654321-4321-4321-4321-210987654321",
    "ClientSecret": "your-client-secret-here"
    }
  }
}
```
### 2. Service Registration

**Program.cs:**
```
csharp
using pvNugsSecretManagerNc6Azure;
using pvNugsCacheNc6Local;
using pvNugsLoggerNc6Seri;

var builder = WebApplication.CreateBuilder(args);

// Register dependencies
builder.Services.TryAddPvNugsCacheNc6Local(builder.Configuration);
builder.Services.TryAddPvNugsLoggerSeriService(builder.Configuration);

// Register Azure Key Vault secret manager
builder.Services.TryAddPvNugsAzureStaticSecretManager(builder.Configuration);

var app = builder.Build();
```
### 3. Usage in Your Application
```
csharp
using pvNugsSecretManagerNc6Abstractions;

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
        
        if (password == null)
            throw new InvalidOperationException("Database password not found");
            
        return $"Server=myserver;Database=mydb;Password={password};";
    }
}
```
## 🔐 Authentication Methods

### Managed Identity (Recommended)

For applications running in Azure (App Service, Functions, VMs, etc.), use Managed Identity for secure, keyless authentication:
```
json
{
  "PvNugsAzureSecretManagerConfig": {
  "KeyVaultUrl": "https://your-keyvault.vault.azure.net/",
    "Credential": null
  }
}
```
**Benefits:**
- No credentials to manage or rotate
- Automatic credential management by Azure
- Enhanced security posture
- Simplified deployment process

### Service Principal

For local development, testing, or non-Azure environments:
```
json
{
  "PvNugsAzureSecretManagerConfig": {
    "KeyVaultUrl": "https://your-keyvault.vault.azure.net/",
    "Credential": {
      "TenantId": "your-tenant-id",
      "ClientId": "your-client-id",
      "ClientSecret": "your-client-secret"
    }
  }
}
```
## 🎯 Advanced Usage Examples

### Error Handling with Retry Logic
```
csharp
public async Task<string> GetSecretWithRetryAsync(string secretName, int maxRetries = 3)
{
for (int attempt = 1; attempt <= maxRetries; attempt++)
{
try
{
var secret = await _secretManager.GetStaticSecretAsync(secretName);
return secret ?? throw new SecretNotFoundException($"Secret '{secretName}' not found");
}
catch (PvNugsStaticSecretManagerException ex) when (IsTransientError(ex) && attempt < maxRetries)
{
var delay = TimeSpan.FromSeconds(Math.Pow(2, attempt)); // Exponential backoff
_logger.LogWarning(ex, "Transient error on attempt {Attempt}, retrying in {Delay}ms",
attempt, delay.TotalMilliseconds);
await Task.Delay(delay);
}
}
throw new InvalidOperationException($"Failed to retrieve secret after {maxRetries} attempts");
}

private static bool IsTransientError(PvNugsStaticSecretManagerException ex)
{
return ex.InnerException is HttpRequestException or TimeoutException;
}
```
### Batch Secret Retrieval
```
csharp
public async Task<Dictionary<string, string>> GetMultipleSecretsAsync(
string[] secretNames,
CancellationToken cancellationToken = default)
{
var tasks = secretNames.Select(async name => new
{
Name = name,
Value = await _secretManager.GetStaticSecretAsync(name, cancellationToken)
});

    var results = await Task.WhenAll(tasks);
    
    return results
        .Where(r => r.Value != null)
        .ToDictionary(r => r.Name, r => r.Value!);
}
```
### Environment-Specific Configuration

```csharp
// Program.cs - Different configuration per environment
if (builder.Environment.IsDevelopment())
{
    // Local development with service principal
    builder.Services.Configure<PvNugsAzureSecretManagerConfig>(options =>
    {
        options.KeyVaultUrl = "https://dev-keyvault.vault.azure.net/";
        options.Credential = new PvNugsAzureServicePrincipalCredential
        {
            TenantId = builder.Configuration["AzureAd:TenantId"]!,
            ClientId = builder.Configuration["AzureAd:ClientId"]!,
            ClientSecret = builder.Configuration["AzureAd:ClientSecret"]!
        };
    });
}
else
{
    // Production with managed identity
    builder.Services.Configure<PvNugsAzureSecretManagerConfig>(options =>
    {
        options.KeyVaultUrl = builder.Configuration["KeyVault:ProductionUrl"]!;
        options.Credential = null; // Use managed identity
    });
}

builder.Services.TryAddPvNugsAzureStaticSecretManager(builder.Configuration);
```

## 🔧 Configuration Options

| Property | Type | Required | Description |
|----------|------|----------|-------------|
| `KeyVaultUrl` | string | Yes | The HTTPS URL of your Azure Key Vault (format: `https://vault-name.vault.azure.net/`) |
| `Credential` | object | No | Service principal credentials (when null, uses Managed Identity) |
| `Credential.TenantId` | string | Conditional* | Azure AD tenant ID (required when using service principal) |
| `Credential.ClientId` | string | Conditional* | Application (client) ID (required when using service principal) |
| `Credential.ClientSecret` | string | Conditional* | Client secret (required when using service principal) |

*Required only when using Service Principal authentication

## 🛡️ Security Best Practices

1. **Use Managed Identity in Production**: Always prefer Managed Identity over Service Principal in Azure environments
2. **Secure Secret Storage**: Store service principal credentials securely (Key Vault, environment variables, etc.)
3. **Regular Credential Rotation**: Implement regular rotation for service principal secrets
4. **Principle of Least Privilege**: Grant minimal required permissions to Key Vault
5. **Network Security**: Use Key Vault firewall and private endpoints when possible
6. **Audit Logging**: Enable Key Vault audit logging for compliance and monitoring

## 📊 Performance Characteristics

- **Cache Hit Response Time**: < 1ms (in-memory cache)
- **Cache Miss Response Time**: 100-500ms (depends on network latency to Azure)
- **Concurrent Request Support**: Unlimited (thread-safe singleton pattern)
- **Memory Footprint**: Minimal (lazy-loaded client, efficient caching)

## 🐛 Troubleshooting

### Common Issues and Solutions

#### Authentication Errors (401/403)

```
PvNugsStaticSecretManagerException: Unauthorized
```


**Solutions:**
- Verify Key Vault access policies or RBAC permissions
- Check service principal credentials if using Service Principal auth
- Ensure Managed Identity is enabled and properly configured

#### Secret Not Found (404)

```
PvNugsStaticSecretManagerException: Secret not found
```


**Solutions:**
- Verify the secret exists in the specified Key Vault
- Check secret name spelling and casing
- Ensure the secret is not disabled or expired

#### Network Connectivity Issues

```
PvNugsStaticSecretManagerException: Network error
```


**Solutions:**
- Check network connectivity to Azure
- Verify Key Vault URL is correct and accessible
- Check firewall and network security group rules

## 📚 API Reference

### IPvNugsStaticSecretManager

```csharp
public interface IPvNugsStaticSecretManager
{
    Task<string?> GetStaticSecretAsync(string secretName, CancellationToken cancellationToken = default);
}
```


### PvNugsStaticSecretManagerException

Custom exception thrown for all secret management errors, wrapping underlying Azure SDK exceptions while preserving original error details.

## 🤝 Contributing

We welcome contributions! Please see our [Contributing Guidelines](CONTRIBUTING.md) for details.

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🔗 Related Packages

- **[pvNugsCacheNc6Local](https://www.nuget.org/packages/pvNugsCacheNc6Local/)** - Local caching provider
- **[pvNugsLoggerNc6Seri](https://www.nuget.org/packages/pvNugsLoggerNc6Seri/)** - Serilog-based logging provider
- **[pvNugsSecretManagerNc6Abstractions](https://www.nuget.org/packages/pvNugsSecretManagerNc6Abstractions/)** - Core abstractions and interfaces

## 📞 Support

- 📖 **Documentation**: [Wiki](../../wiki)
- 🐛 **Bug Reports**: [Issues](../../issues)
- 💡 **Feature Requests**: [Discussions](../../discussions)
- 📧 **Email Support**: [Contact Us](mailto:support@pvnugs.com)

---

**Made with ❤️ by the PvNugs Team**

*Secure your secrets with confidence!* 🔐

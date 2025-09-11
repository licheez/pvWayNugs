# pvNugsSecretManagerNc9EnvVariables

[![NuGet Version](https://img.shields.io/nuget/v/pvNugsSecretManagerNc9EnvVariables)](https://www.nuget.org/packages/pvNugsSecretManagerNc9EnvVariables/)
[![.NET](https://img.shields.io/badge/.NET-9.0-blue)](https://dotnet.microsoft.com/download/dotnet/9.0)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

A .NET 9 implementation of the pvNugs Secret Manager abstraction for environment variables and configuration-based secret management. Provides both static secret retrieval and simulated dynamic credential management with expiration tracking.

## 🚀 Features

### Static Secret Management
- **Universal Configuration Support**: Works with environment variables, appsettings.json, Azure Key Vault, and any IConfiguration provider
- **Prefix Organization**: Optional prefix support for logical secret grouping
- **Async/Await Pattern**: Full async support with proper cancellation token handling
- **Comprehensive Logging**: Detailed error logging using the pvNugs logging framework

### Simulated Dynamic Credentials
- **Structured Credentials**: Username, password, and expiration date composition
- **Expiration Awareness**: Built-in expiration checking and validation
- **Development-Friendly**: Perfect for testing and development environments
- **Migration Ready**: Consistent interface for future true dynamic secret integration

### Enterprise-Ready
- **Thread-Safe**: Safe for concurrent operations across multiple threads
- **Robust Error Handling**: Comprehensive exception handling with detailed error messages
- **Dependency Injection**: Full DI support with fluent configuration extensions
- **Production Tested**: Built with enterprise-grade reliability and performance

## 📦 Installation

```shell script
dotnet add package pvNugsSecretManagerNc9EnvVariables
```


## 🎯 Prerequisites

This package requires a pvNugs logger implementation:

```shell script
# Choose one of these logger packages:
dotnet add package pvNugsLoggerNc9Console    # Console logging
dotnet add package pvNugsLoggerNc9Seri       # Serilog integration
dotnet add package pvNugsLoggerNc9MsLogger   # Microsoft.Extensions.Logging
```


## 🚀 Quick Start

### 1. Basic Setup

```csharp
using pvNugsSecretManagerNc9EnvVariables;

var builder = WebApplication.CreateBuilder(args);

// Register logger first (required dependency)
builder.Services.TryAddPvNugsConsoleLoggerService(SeverityEnu.Debug);

// Register secret manager services
builder.Services.TryAddPvNugsSecretManagerEnvVariables(builder.Configuration);

var app = builder.Build();
```


### 2. Static Secret Usage

```csharp
public class ApiService
{
    private readonly IPvNugsStaticSecretManager _secretManager;
    
    public ApiService(IPvNugsStaticSecretManager secretManager)
    {
        _secretManager = secretManager;
    }
    
    public async Task<string> GetConnectionStringAsync()
    {
        // Retrieves from environment variable or configuration
        var connectionString = await _secretManager.GetStaticSecretAsync("DatabaseConnection");
        
        if (string.IsNullOrEmpty(connectionString))
            throw new InvalidOperationException("Database connection string not configured");
            
        return connectionString;
    }
}
```


### 3. Dynamic Credential Usage

```csharp
public class DatabaseService
{
    private readonly IPvNugsDynamicSecretManager _credentialManager;
    
    public DatabaseService(IPvNugsDynamicSecretManager credentialManager)
    {
        _credentialManager = credentialManager;
    }
    
    public async Task<IDbConnection> GetConnectionAsync()
    {
        var credential = await _credentialManager.GetDynamicSecretAsync("DatabaseService");
        
        if (credential == null)
            throw new InvalidOperationException("Database credentials not configured");
            
        // Check expiration
        if (credential.ExpirationDateUtc <= DateTime.UtcNow)
            throw new InvalidOperationException("Database credentials have expired");
            
        return new SqlConnection($"Server=localhost;User={credential.Username};Password={credential.Password};");
    }
}
```


## ⚙️ Configuration

### Environment Variables

```shell script
# Without prefix
DatabaseConnection="Server=localhost;Database=MyApp;"
ApiKey="abc123xyz"

# With prefix (recommended)
MyApp__DatabaseConnection="Server=localhost;Database=MyApp;"
MyApp__ApiKey="abc123xyz"

# Dynamic credentials
MyApp__DatabaseService__username="dbuser"
MyApp__DatabaseService__password="dbsecret"
MyApp__DatabaseService__expirationDateUtc="2024-12-31T23:59:59Z"
```


### appsettings.json

```json
{
  "PvNugsSecretManagerEnvVariablesConfig": {
    "Prefix": "MyApp"
  },
  "MyApp": {
    "DatabaseConnection": "Server=localhost;Database=MyApp;",
    "ApiKey": "abc123xyz",
    "DatabaseService__username": "dbuser",
    "DatabaseService__password": "dbsecret",
    "DatabaseService__expirationDateUtc": "2024-12-31T23:59:59Z"
  }
}
```


### Advanced Configuration

```csharp
// Custom configuration
builder.Services.Configure<PvNugsSecretManagerEnvVariablesConfig>(options =>
{
    options.Prefix = "Production"; // Environment-specific prefix
});

// Multiple environments
if (builder.Environment.IsProduction())
{
    builder.Services.Configure<PvNugsSecretManagerEnvVariablesConfig>(options =>
        options.Prefix = "Prod");
}
else
{
    builder.Services.Configure<PvNugsSecretManagerEnvVariablesConfig>(options =>
        options.Prefix = "Dev");
}
```


## 🔧 Dynamic Credentials Deep Dive

### Important Note
This implementation provides **simulated** dynamic credentials, not true dynamic secret management like HashiCorp Vault or AWS Secrets Manager. It's designed for:

- Development and testing environments
- Migration scenarios from static to dynamic secrets
- Applications requiring expiration-aware credential handling
- Consistent interfaces across different deployment environments

### Configuration Structure

For a dynamic credential named "MyService", configure these three components:

```shell script
# Environment Variables
MyService__username="serviceuser"
MyService__password="servicepass"
MyService__expirationDateUtc="2024-06-30T23:59:59Z"

# Or with prefix "MyApp"
MyApp__MyService__username="serviceuser"
MyApp__MyService__password="servicepass"  
MyApp__MyService__expirationDateUtc="2024-06-30T23:59:59Z"
```


### Expiration Handling

```csharp
public async Task<bool> ValidateCredentialAsync(string serviceName)
{
    var credential = await _dynamicManager.GetDynamicSecretAsync(serviceName);
    
    if (credential == null)
    {
        _logger.LogWarning($"Credential for {serviceName} not found");
        return false;
    }
    
    var timeUntilExpiry = credential.ExpirationDateUtc - DateTime.UtcNow;
    
    if (timeUntilExpiry <= TimeSpan.Zero)
    {
        _logger.LogError($"Credential for {serviceName} expired on {credential.ExpirationDateUtc}");
        return false;
    }
    
    if (timeUntilExpiry < TimeSpan.FromDays(7))
    {
        _logger.LogWarning($"Credential for {serviceName} expires in {timeUntilExpiry.TotalDays:F1} days");
    }
    
    return true;
}
```


## 🛡️ Security Best Practices

### Environment Variables
```shell script
# Use secure environment variable injection
docker run -e MyApp__DatabasePassword="$(cat /secrets/db-password)" myapp

# Kubernetes secrets
kubectl create secret generic myapp-secrets \
  --from-literal=MyApp__DatabasePassword="supersecret"
```


### Configuration Management
```csharp
// Don't log sensitive values
try
{
    var secret = await _secretManager.GetStaticSecretAsync("Password");
    // ✅ Good: Don't log the actual secret
    _logger.LogInfo("Successfully retrieved password");
}
catch (Exception ex)
{
    // ✅ Good: Log the error without exposing secrets
    _logger.LogError(ex, "Failed to retrieve password from configuration");
}
```


## 🔍 Error Handling

### Exception Types
```csharp
try
{
    var credential = await _dynamicManager.GetDynamicSecretAsync("MyService");
}
catch (ArgumentException ex)
{
    // Invalid secret name parameter
    _logger.LogError(ex, "Invalid secret name provided");
}
catch (FormatException ex)
{
    // Invalid expiration date format
    _logger.LogError(ex, "Invalid expiration date in configuration");
}
catch (PvNugsSecretManagerException ex)
{
    // Configuration access or other system errors
    _logger.LogError(ex, "Secret manager error occurred");
    
    // Access original exception if needed
    if (ex.InnerException is ConfigurationException configEx)
    {
        _logger.LogError($"Configuration issue: {configEx.Message}");
    }
}
```


## 🧪 Testing Support

### Unit Testing
```csharp
[Test]
public async Task Should_Retrieve_Static_Secret()
{
    // Arrange
    var configuration = new ConfigurationBuilder()
        .AddInMemoryCollection(new Dictionary<string, string>
        {
            ["MyApp__ApiKey"] = "test-key-123"
        })
        .Build();
        
    var options = Options.Create(new PvNugsSecretManagerEnvVariablesConfig 
    { 
        Prefix = "MyApp" 
    });
    
    var logger = new TestLoggerService();
    var secretManager = new StaticSecretManager(logger, options, configuration);
    
    // Act
    var result = await secretManager.GetStaticSecretAsync("ApiKey");
    
    // Assert
    Assert.AreEqual("test-key-123", result);
}
```


### Integration Testing
```csharp
public class SecretManagerIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    [Fact]
    public async Task Should_Retrieve_Secrets_From_Configuration()
    {
        // Arrange
        var factory = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureAppConfiguration(config =>
            {
                config.AddInMemoryCollection(new Dictionary<string, string>
                {
                    ["TestApp__DatabasePassword"] = "integration-test-password"
                });
            });
        });
        
        // Act & Assert
        using var scope = factory.Services.CreateScope();
        var secretManager = scope.ServiceProvider.GetRequiredService<IPvNugsStaticSecretManager>();
        var password = await secretManager.GetStaticSecretAsync("DatabasePassword");
        
        Assert.Equal("integration-test-password", password);
    }
}
```


## 🚀 Migration to True Dynamic Secrets

When ready to migrate to enterprise secret management:

```csharp
// Current implementation
services.TryAddPvNugsSecretManagerEnvVariables(configuration);

// Future migration - same interface!
services.TryAddPvNugsSecretManagerVault(vaultConfig);        // HashiCorp Vault
services.TryAddPvNugsSecretManagerAzureKv(azureConfig);      // Azure Key Vault  
services.TryAddPvNugsSecretManagerAwsSecrets(awsConfig);     // AWS Secrets Manager
```


## 📚 Related Packages

- **pvNugsSecretManagerNc9Abstractions** - Core interfaces and contracts
- **pvNugsLoggerNc9Abstractions** - Logging abstractions
- **pvNugsLoggerNc9Console** - Console logging implementation
- **pvNugsLoggerNc9Seri** - Serilog integration
- **pvNugsLoggerNc9MsLogger** - Microsoft Extensions Logging integration

## 🤝 Contributing

We welcome contributions! Please see our [Contributing Guide](https://github.com/licheez/pvWayNugs/blob/main/CONTRIBUTING.md) for details.

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](https://github.com/licheez/pvWayNugs/blob/main/LICENSE) file for details.

## 🔗 Links

- **GitHub Repository**: https://github.com/licheez/pvWayNugs
- **NuGet Package**: https://www.nuget.org/packages/pvNugsSecretManagerNc9EnvVariables/
- **Documentation**: https://github.com/licheez/pvWayNugs/wiki
- **Issues**: https://github.com/licheez/pvWayNugs/issues

## 📞 Support

- **Issues**: Report bugs and request features on [GitHub Issues](https://github.com/licheez/pvWayNugs/issues)
- **Discussions**: Join the conversation on [GitHub Discussions](https://github.com/licheez/pvWayNugs/discussions)

---

**Built with ❤️ by pvWay Ltd**
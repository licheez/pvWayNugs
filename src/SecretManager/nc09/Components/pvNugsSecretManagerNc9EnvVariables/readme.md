# pvNugsSecretManagerNc9EnvVariables

[![NuGet Version](https://img.shields.io/nuget/v/pvNugsSecretManagerNc9EnvVariables)](https://www.nuget.org/packages/pvNugsSecretManagerNc9EnvVariables/)
[![.NET](https://img.shields.io/badge/.NET-9.0-blue)](https://dotnet.microsoft.com/download/dotnet/9.0)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

A robust .NET 9 implementation of the pvNugs Secret Manager abstraction that provides configuration-based secret management with support for environment variables, JSON files, Azure Key Vault, and any IConfiguration provider. Designed for enterprise applications requiring secure, flexible, and maintainable secret management across development, testing, and production environments.

## 🎯 Key Capabilities

### 🔐 Static Secret Management
- **Universal Configuration Support**: Seamlessly integrates with environment variables, appsettings.json, Azure Key Vault, AWS Parameter Store, and any IConfiguration provider
- **Intelligent Prefix Organization**: Optional hierarchical secret organization with configurable prefixes for multi-tenant, microservice, and environment-specific scenarios
- **Enterprise-Grade Error Handling**: Comprehensive exception handling with detailed logging and proper exception chaining
- **Async-First Design**: Full async/await support with proper cancellation token handling for scalable applications

### 🔄 Simulated Dynamic Credentials
- **Structured Credential Composition**: Combines username, password, and expiration metadata into cohesive credential objects
- **Expiration Intelligence**: Built-in expiration validation with configurable tolerance thresholds
- **Development & Testing Friendly**: Perfect bridge between static and true dynamic secret management
- **Migration-Ready Architecture**: Consistent interface design enables seamless future migration to enterprise secret management systems

### 🏢 Production-Ready Features
- **Thread-Safe Operations**: Designed for high-concurrency scenarios with safe multi-threaded access
- **Comprehensive Logging Integration**: Deep integration with pvNugs logging framework for operational visibility
- **Flexible Dependency Injection**: Full DI container support with fluent configuration extensions
- **Extensible Architecture**: Protected methods and inheritance-friendly design for custom implementations

## 📦 Installation
```
bash
dotnet add package pvNugsSecretManagerNc9EnvVariables
```
## 🔧 Prerequisites

This package requires a pvNugs logger implementation:
```
bash
# Choose one logging implementation:
dotnet add package pvNugsLoggerNc9Console    # Console logging
dotnet add package pvNugsLoggerNc9Seri       # Serilog integration  
dotnet add package pvNugsLoggerNc9MsLogger   # Microsoft.Extensions.Logging bridge
```
## 🚀 Quick Start Guide

### 1. Basic Service Registration
```
csharp
using pvNugsSecretManagerNc9EnvVariables;

var builder = WebApplication.CreateBuilder(args);

// Register required logger (choose your preferred implementation)
builder.Services.TryAddPvNugsConsoleLoggerService(SeverityEnu.Debug);

// Register secret manager with automatic configuration binding
builder.Services.TryAddPvNugsSecretManagerEnvVariables(builder.Configuration);

var app = builder.Build();
```
### 2. Static Secret Retrieval
```
csharp
public class DatabaseService
{
private readonly IPvNugsStaticSecretManager _secretManager;
private readonly ILogger<DatabaseService> _logger;

    public DatabaseService(IPvNugsStaticSecretManager secretManager, ILogger<DatabaseService> logger)
    {
        _secretManager = secretManager;
        _logger = logger;
    }
    
    public async Task<IDbConnection> CreateConnectionAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            // Retrieve connection string from any configured source
            var connectionString = await _secretManager.GetStaticSecretAsync("DatabaseConnection", cancellationToken);
            
            if (string.IsNullOrEmpty(connectionString))
                throw new InvalidOperationException("Database connection string not found in configuration");
                
            return new SqlConnection(connectionString);
        }
        catch (PvNugsSecretManagerException ex)
        {
            _logger.LogError(ex, "Failed to retrieve database connection string");
            throw;
        }
    }
}
```
### 3. Dynamic Credential Management

```csharp
public class ApiService
{
    private readonly IPvNugsDynamicSecretManager _credentialManager;
    private readonly ILogger<ApiService> _logger;
    
    public ApiService(IPvNugsDynamicSecretManager credentialManager, ILogger<ApiService> logger)
    {
        _credentialManager = credentialManager;
        _logger = logger;
    }
    
    public async Task<HttpClient> CreateAuthenticatedClientAsync(string serviceName)
    {
        var credential = await _credentialManager.GetDynamicSecretAsync(serviceName);
        
        if (credential == null)
        {
            throw new InvalidOperationException($"Credentials for {serviceName} not found in configuration");
        }
        
        // Validate expiration with early warning
        var timeUntilExpiry = credential.ExpirationDateUtc - DateTime.UtcNow;
        
        if (timeUntilExpiry <= TimeSpan.Zero)
        {
            throw new InvalidOperationException($"Credentials for {serviceName} expired on {credential.ExpirationDateUtc:yyyy-MM-dd HH:mm:ss} UTC");
        }
        
        if (timeUntilExpiry < TimeSpan.FromHours(24))
        {
            _logger.LogWarning("Credentials for {ServiceName} expire in {Hours:F1} hours", 
                serviceName, timeUntilExpiry.TotalHours);
        }
        
        var client = new HttpClient();
        client.DefaultRequestHeaders.Authorization = 
            new AuthenticationHeaderValue("Basic", 
                Convert.ToBase64String(Encoding.UTF8.GetBytes($"{credential.Username}:{credential.Password}")));
                
        return client;
    }
}
```

## ⚙️ Configuration Strategies

### Environment Variables (Recommended for Containers)

```bash
# Without prefix - direct access
export DatabaseConnection="Server=localhost;Database=MyApp;User Id=appuser;Password=secret123;"
export ApiKey="your-api-key-here"

# With prefix - organized secrets (recommended)
export MyApp__DatabaseConnection="Server=localhost;Database=MyApp;User Id=appuser;Password=secret123;"
export MyApp__ExternalApi__ApiKey="your-api-key-here"
export MyApp__CertificatePassword="cert-password-here"

# Dynamic credentials with expiration
export MyApp__DatabaseService__username="dynamic_user"
export MyApp__DatabaseService__password="dynamic_pass_123"  
export MyApp__DatabaseService__expirationDateUtc="2024-12-31T23:59:59Z"
```

### JSON Configuration (Development & Testing)

```json
{
  "PvNugsSecretManagerEnvVariablesConfig": {
    "Prefix": "MyApp"
  },
  "MyApp": {
    "DatabaseConnection": "Server=localhost;Database=MyApp;User Id=appuser;Password=secret123;",
    "ExternalApi": {
      "ApiKey": "your-api-key-here",
      "Timeout": "00:00:30"
    },
    "DatabaseService__username": "dynamic_user",
    "DatabaseService__password": "dynamic_pass_123",
    "DatabaseService__expirationDateUtc": "2024-12-31T23:59:59Z"
  }
}
```

### Multi-Environment Configuration

```csharp
public static class ServiceExtensions
{
    public static IServiceCollection AddSecretManagement(
        this IServiceCollection services, 
        IConfiguration configuration,
        IWebHostEnvironment environment)
    {
        // Environment-specific prefix configuration
        services.Configure<PvNugsSecretManagerEnvVariablesConfig>(options =>
        {
            options.Prefix = environment.IsDevelopment() ? "Dev" : 
                           environment.IsStaging() ? "Staging" : "Prod";
        });
        
        services.TryAddPvNugsSecretManagerEnvVariables(configuration);
        return services;
    }
}
```

## 🔐 Integration with Connection String Providers

This package seamlessly integrates with the pvNugs connection string provider ecosystem:

```csharp
// SQL Server integration with StaticSecret mode
{
  "PvNugsCsProviderMsSqlConfig": {
    "Mode": "StaticSecret",
    "Server": "localhost",
    "Database": "MyApp",
    "Username": "app_user",
    "SecretName": "MyApp-Database"
  },
  "MyApp": {
    "MyApp-Database-Owner": "owner_password_secure",
    "MyApp-Database-Application": "app_password_secure", 
    "MyApp-Database-Reader": "reader_password_secure"
  }
}

// PostgreSQL integration with DynamicSecret mode  
{
  "PvNugsCsProviderPgSqlConfig": {
    "Mode": "DynamicSecret", 
    "Server": "localhost",
    "Database": "MyApp",
    "Schema": "public",
    "SecretName": "MyApp-PostgreSQL"
  },
  "MyApp": {
    "MyApp-PostgreSQL-Owner__username": "dynamic_owner",
    "MyApp-PostgreSQL-Owner__password": "dynamic_owner_pass",
    "MyApp-PostgreSQL-Owner__expirationDateUtc": "2024-12-31T23:59:59Z",
    "MyApp-PostgreSQL-Application__username": "dynamic_app",
    "MyApp-PostgreSQL-Application__password": "dynamic_app_pass", 
    "MyApp-PostgreSQL-Application__expirationDateUtc": "2024-12-31T23:59:59Z"
  }
}
```

## 🌐 Cloud & Container Deployment

### Docker Container Integration

```dockerfile
# Dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY . .

# Secrets can be injected at runtime via environment variables
# or mounted secret files
ENTRYPOINT ["dotnet", "MyApp.dll"]
```
```


## ⚙️ Configuration Strategies

### Environment Variables (Recommended for Containers)

```shell script
# Without prefix - direct access
export DatabaseConnection="Server=localhost;Database=MyApp;User Id=appuser;Password=secret123;"
export ApiKey="your-api-key-here"

# With prefix - organized secrets (recommended)
export MyApp__DatabaseConnection="Server=localhost;Database=MyApp;User Id=appuser;Password=secret123;"
export MyApp__ExternalApi__ApiKey="your-api-key-here"
export MyApp__CertificatePassword="cert-password-here"

# Dynamic credentials with expiration
export MyApp__DatabaseService__username="dynamic_user"
export MyApp__DatabaseService__password="dynamic_pass_123"  
export MyApp__DatabaseService__expirationDateUtc="2024-12-31T23:59:59Z"
```


### JSON Configuration (Development & Testing)

```json
{
  "PvNugsSecretManagerEnvVariablesConfig": {
    "Prefix": "MyApp"
  },
  "MyApp": {
    "DatabaseConnection": "Server=localhost;Database=MyApp;User Id=appuser;Password=secret123;",
    "ExternalApi": {
      "ApiKey": "your-api-key-here",
      "Timeout": "00:00:30"
    },
    "DatabaseService__username": "dynamic_user",
    "DatabaseService__password": "dynamic_pass_123",
    "DatabaseService__expirationDateUtc": "2024-12-31T23:59:59Z"
  }
}
```


### Multi-Environment Configuration

```csharp
public static class ServiceExtensions
{
    public static IServiceCollection AddSecretManagement(
        this IServiceCollection services, 
        IConfiguration configuration,
        IWebHostEnvironment environment)
    {
        // Environment-specific prefix configuration
        services.Configure<PvNugsSecretManagerEnvVariablesConfig>(options =>
        {
            options.Prefix = environment.IsDevelopment() ? "Dev" : 
                           environment.IsStaging() ? "Staging" : "Prod";
        });
        
        services.TryAddPvNugsSecretManagerEnvVariables(configuration);
        return services;
    }
}
```


## 🔐 Integration with Connection String Providers

This package seamlessly integrates with the pvNugs connection string provider ecosystem:

```csharp
// SQL Server integration with StaticSecret mode
{
  "PvNugsCsProviderMsSqlConfig": {
    "Mode": "StaticSecret",
    "Server": "localhost",
    "Database": "MyApp",
    "Username": "app_user",
    "SecretName": "MyApp-Database"
  },
  "MyApp": {
    "MyApp-Database-Owner": "owner_password_secure",
    "MyApp-Database-Application": "app_password_secure", 
    "MyApp-Database-Reader": "reader_password_secure"
  }
}

// PostgreSQL integration with DynamicSecret mode  
{
  "PvNugsCsProviderPgSqlConfig": {
    "Mode": "DynamicSecret", 
    "Server": "localhost",
    "Database": "MyApp",
    "Schema": "public",
    "SecretName": "MyApp-PostgreSQL"
  },
  "MyApp": {
    "MyApp-PostgreSQL-Owner__username": "dynamic_owner",
    "MyApp-PostgreSQL-Owner__password": "dynamic_owner_pass",
    "MyApp-PostgreSQL-Owner__expirationDateUtc": "2024-12-31T23:59:59Z",
    "MyApp-PostgreSQL-Application__username": "dynamic_app",
    "MyApp-PostgreSQL-Application__password": "dynamic_app_pass", 
    "MyApp-PostgreSQL-Application__expirationDateUtc": "2024-12-31T23:59:59Z"
  }
}
```


## 🌐 Cloud & Container Deployment

### Docker Container Integration

```dockerfile
# Dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY . .

# Secrets can be injected at runtime via environment variables
# or mounted secret files
ENTRYPOINT ["dotnet", "MyApp.dll"]
```


```shell script
# Docker run with environment secrets
docker run -e MyApp__DatabaseConnection="$(cat /secrets/db-conn)" \
           -e MyApp__ApiKey="$(cat /secrets/api-key)" \
           myapp:latest

# Docker Compose with secrets
version: '3.8'
services:
  myapp:
    image: myapp:latest
    environment:
      - MyApp__DatabaseConnection=${DB_CONNECTION}
      - MyApp__ApiKey=${API_KEY}
    secrets:
      - db_password
      - api_key
secrets:
  db_password:
    file: ./secrets/db_password.txt
  api_key:
    file: ./secrets/api_key.txt
```


### Kubernetes Integration

```yaml
# ConfigMap for non-sensitive configuration
apiVersion: v1
kind: ConfigMap
metadata:
  name: myapp-config
data:
  appsettings.json: |
    {
      "PvNugsSecretManagerEnvVariablesConfig": {
        "Prefix": "MyApp"
      }
    }

---
# Secret for sensitive data
apiVersion: v1
kind: Secret
metadata:
  name: myapp-secrets
type: Opaque
data:
  MyApp__DatabaseConnection: U2VydmVyPWxvY2FsaG9zdDs...  # base64 encoded
  MyApp__ApiKey: eW91ci1hcGkta2V5LWhlcmU=  # base64 encoded

---
# Deployment using both ConfigMap and Secret
apiVersion: apps/v1
kind: Deployment
metadata:
  name: myapp
spec:
  template:
    spec:
      containers:
      - name: myapp
        image: myapp:latest
        envFrom:
        - configMapRef:
            name: myapp-config
        - secretRef:
            name: myapp-secrets
        volumeMounts:
        - name: config-volume
          mountPath: /app/appsettings.json
          subPath: appsettings.json
      volumes:
      - name: config-volume
        configMap:
          name: myapp-config
```


### Azure Key Vault Integration

```csharp
// Program.cs - Azure Key Vault as configuration source
var builder = WebApplication.CreateBuilder(args);

if (builder.Environment.IsProduction())
{
    // Azure Key Vault integration via configuration provider
    var keyVaultEndpoint = builder.Configuration["KeyVaultEndpoint"];
    builder.Configuration.AddAzureKeyVault(
        new Uri(keyVaultEndpoint),
        new DefaultAzureCredential());
}

// Secret manager automatically uses Key Vault values through IConfiguration
builder.Services.Configure<PvNugsSecretManagerEnvVariablesConfig>(options =>
{
    options.Prefix = "MyApp";
});

builder.Services.TryAddPvNugsSecretManagerEnvVariables(builder.Configuration);
```


## 🧪 Testing & Development

### Unit Testing Support

```csharp
[TestClass]
public class SecretManagerTests
{
    [TestMethod]
    public async Task Should_Retrieve_Static_Secret_Successfully()
    {
        // Arrange
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>
            {
                ["TestApp__DatabaseConnection"] = "Server=test;Database=TestDb;",
                ["TestApp__ApiKey"] = "test-api-key-12345"
            })
            .Build();
            
        var options = Options.Create(new PvNugsSecretManagerEnvVariablesConfig 
        { 
            Prefix = "TestApp" 
        });
        
        var mockLogger = new Mock<ILoggerService>();
        var secretManager = new StaticSecretManager(mockLogger.Object, options, configuration);
        
        // Act
        var dbConnection = await secretManager.GetStaticSecretAsync("DatabaseConnection");
        var apiKey = await secretManager.GetStaticSecretAsync("ApiKey");
        
        // Assert
        Assert.AreEqual("Server=test;Database=TestDb;", dbConnection);
        Assert.AreEqual("test-api-key-12345", apiKey);
    }
    
    [TestMethod]
    public async Task Should_Handle_Missing_Secret_Gracefully()
    {
        // Arrange
        var configuration = new ConfigurationBuilder().Build();
        var options = Options.Create(new PvNugsSecretManagerEnvVariablesConfig());
        var mockLogger = new Mock<ILoggerService>();
        var secretManager = new StaticSecretManager(mockLogger.Object, options, configuration);
        
        // Act
        var result = await secretManager.GetStaticSecretAsync("NonExistentSecret");
        
        // Assert
        Assert.IsNull(result);
    }
    
    [TestMethod]
    public async Task Should_Validate_Dynamic_Credential_Expiration()
    {
        // Arrange
        var futureExpiration = DateTime.UtcNow.AddDays(30).ToString("yyyy-MM-ddTHH:mm:ssZ");
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>
            {
                ["TestService__username"] = "testuser",
                ["TestService__password"] = "testpass",
                ["TestService__expirationDateUtc"] = futureExpiration
            })
            .Build();
            
        var options = Options.Create(new PvNugsSecretManagerEnvVariablesConfig());
        var mockLogger = new Mock<ILoggerService>();
        var credentialManager = new DynamicSecretManager(mockLogger.Object, options, configuration);
        
        // Act
        var credential = await credentialManager.GetDynamicSecretAsync("TestService");
        
        // Assert
        Assert.IsNotNull(credential);
        Assert.AreEqual("testuser", credential.Username);
        Assert.AreEqual("testpass", credential.Password);
        Assert.IsTrue(credential.ExpirationDateUtc > DateTime.UtcNow);
    }
}
```


### Integration Testing

```csharp
public class SecretManagerIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    
    public SecretManagerIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }
    
    [Fact]
    public async Task Should_Integrate_With_DI_Container()
    {
        // Arrange
        var factory = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureAppConfiguration(config =>
            {
                config.AddInMemoryCollection(new Dictionary<string, string>
                {
                    ["IntegrationTest__DatabasePassword"] = "integration-password-123",
                    ["IntegrationTest__ApiEndpoint"] = "https://api.integration-test.com"
                });
            });
            
            builder.ConfigureServices(services =>
            {
                services.Configure<PvNugsSecretManagerEnvVariablesConfig>(options =>
                {
                    options.Prefix = "IntegrationTest";
                });
            });
        });
        
        // Act & Assert
        using var scope = factory.Services.CreateScope();
        var secretManager = scope.ServiceProvider.GetRequiredService<IPvNugsStaticSecretManager>();
        
        var dbPassword = await secretManager.GetStaticSecretAsync("DatabasePassword");
        var apiEndpoint = await secretManager.GetStaticSecretAsync("ApiEndpoint");
        
        Assert.Equal("integration-password-123", dbPassword);
        Assert.Equal("https://api.integration-test.com", apiEndpoint);
    }
}
```


## 🔍 Advanced Error Handling

### Exception Hierarchy

```csharp
public class SecretService
{
    private readonly IPvNugsStaticSecretManager _secretManager;
    private readonly ILogger<SecretService> _logger;
    
    public async Task<string> GetRequiredSecretAsync(string secretName)
    {
        try
        {
            var secret = await _secretManager.GetStaticSecretAsync(secretName);
            
            if (string.IsNullOrEmpty(secret))
            {
                throw new InvalidOperationException($"Required secret '{secretName}' not found in configuration");
            }
            
            return secret;
        }
        catch (ArgumentException ex)
        {
            // Invalid parameter usage - programming error
            _logger.LogError(ex, "Invalid secret name parameter: {SecretName}", secretName);
            throw;
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("configuration section"))
        {
            // Missing configuration section - deployment/configuration error
            _logger.LogError(ex, "Configuration section missing for secret retrieval");
            throw new ConfigurationException("Secret manager configuration is invalid", ex);
        }
        catch (PvNugsSecretManagerException ex)
        {
            // Secret manager specific error - system/infrastructure error
            _logger.LogError(ex, "Secret manager failed to retrieve '{SecretName}'", secretName);
            
            // Examine inner exception for specific error handling
            switch (ex.InnerException)
            {
                case UnauthorizedAccessException _:
                    throw new SecurityException("Access denied to secret store", ex);
                case TimeoutException _:
                    throw new ServiceUnavailableException("Secret store timeout", ex);
                default:
                    throw;
            }
        }
    }
}
```


### Retry and Resilience Patterns

```csharp
public class ResilientSecretService
{
    private readonly IPvNugsStaticSecretManager _secretManager;
    private readonly ILogger<ResilientSecretService> _logger;
    
    public async Task<string> GetSecretWithRetryAsync(string secretName, int maxRetries = 3)
    {
        var retryPolicy = Policy
            .Handle<PvNugsSecretManagerException>(ex => ex.InnerException is TimeoutException)
            .WaitAndRetryAsync(
                maxRetries,
                retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)), // Exponential backoff
                onRetry: (outcome, timespan, retryCount, context) =>
                {
                    _logger.LogWarning("Retry attempt {RetryCount} for secret '{SecretName}' after {Delay}ms",
                        retryCount, secretName, timespan.TotalMilliseconds);
                });
        
        return await retryPolicy.ExecuteAsync(async () =>
        {
            var secret = await _secretManager.GetStaticSecretAsync(secretName);
            
            if (string.IsNullOrEmpty(secret))
                throw new InvalidOperationException($"Secret '{secretName}' not found");
                
            return secret;
        });
    }
}
```


## 🛡️ Security Best Practices

### Secure Configuration Management

```csharp
// ✅ Good: Structured secret access with validation
public class SecureSecretService
{
    private readonly IPvNugsStaticSecretManager _secretManager;
    private readonly ILogger<SecureSecretService> _logger;
    
    public async Task<DatabaseConfig> GetDatabaseConfigAsync()
    {
        try
        {
            var connectionString = await _secretManager.GetStaticSecretAsync("DatabaseConnection");
            ValidateConnectionString(connectionString);
            
            // ✅ Good: Log success without exposing secrets
            _logger.LogInformation("Successfully retrieved database configuration");
            
            return new DatabaseConfig { ConnectionString = connectionString };
        }
        catch (Exception ex)
        {
            // ✅ Good: Log errors without exposing secret values
            _logger.LogError(ex, "Failed to retrieve database configuration");
            throw;
        }
    }
    
    private static void ValidateConnectionString(string? connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new InvalidOperationException("Database connection string is required");
            
        // Additional validation without logging the actual value
        if (!connectionString.Contains("Server=") && !connectionString.Contains("Host="))
            throw new InvalidOperationException("Invalid database connection string format");
    }
}

// ❌ Bad: Logging sensitive information
public class InsecureSecretService
{
    public async Task<string> GetApiKeyAsync()
    {
        var apiKey = await _secretManager.GetStaticSecretAsync("ApiKey");
        
        // ❌ Bad: Logs sensitive information
        _logger.LogInformation("Retrieved API key: {ApiKey}", apiKey);
        
        return apiKey;
    }
}
```


### Environment-Specific Security

```shell script
# Production environment variables (secure injection)
# Use secure secret injection mechanisms:
docker run --env-file /secure/prod.env myapp:latest
kubectl apply -f k8s-secrets.yaml

# ✅ Good: Production secrets
export MyApp__DatabaseConnection="Server=prod-db.internal;Database=ProdApp;User Id=prod_user;Password=$(cat /vault/db-password);"
export MyApp__ApiKey="$(vault kv get -field=api_key secret/myapp/prod)"

# ❌ Bad: Hardcoded secrets in environment files
export MyApp__ApiKey="hardcoded-key-12345"  # Never do this in production
```


## 🔄 Migration to Enterprise Secret Management

### Future-Proofing Your Code

```csharp
// Current implementation - works today
services.TryAddPvNugsSecretManagerEnvVariables(configuration);

// Future migration options - same interface contract!
// services.TryAddPvNugsSecretManagerVault(vaultConfig);        // HashiCorp Vault
// services.TryAddPvNugsSecretManagerAzureKv(azureConfig);      // Azure Key Vault  
// services.TryAddPvNugsSecretManagerAwsSecrets(awsConfig);     // AWS Secrets Manager

// Application code remains unchanged - benefits of abstraction!
public class DatabaseService
{
    private readonly IPvNugsStaticSecretManager _secretManager; // Same interface
    
    public async Task<string> GetConnectionStringAsync()
    {
        // This code works with any implementation
        return await _secretManager.GetStaticSecretAsync("DatabaseConnection");
    }
}
```


### Migration Planning

1. **Phase 1**: Use this package with configuration-based secrets
2. **Phase 2**: Integrate with cloud key vaults via configuration providers
3. **Phase 3**: Migrate to true dynamic secret management when available
4. **Phase 4**: Implement automatic credential rotation and lifecycle management

## 📚 Related Packages

### Core Dependencies
- **pvNugsSecretManagerNc9Abstractions** - Core interfaces and contracts
- **pvNugsLoggerNc9Abstractions** - Logging framework abstractions

### Logger Implementations (choose one)
- **pvNugsLoggerNc9Console** - Console logging with color support
- **pvNugsLoggerNc9Seri** - Serilog integration with structured logging
- **pvNugsLoggerNc9MsLogger** - Microsoft Extensions Logging bridge

### Connection String Providers
- **pvNugsCsProviderNc9MsSql** - SQL Server connection string provider
- **pvNugsCsProviderNc9PgSql** - PostgreSQL connection string provider

## 📈 Performance Characteristics

- **Memory Efficient**: No internal secret caching - relies on configuration provider optimization
- **Thread-Safe**: Concurrent access safe with no synchronization overhead
- **Fast Configuration Access**: Leverages IConfiguration provider caching (typically sub-millisecond)
- **Minimal Allocations**: Efficient string handling with minimal garbage collection pressure
- **Scalable**: Designed for high-throughput applications with thousands of concurrent requests

## 🤝 Contributing

We welcome contributions to improve this package! Please:

1. Check existing [issues](https://github.com/licheez/pvWayNugs/issues) before creating new ones
2. Follow the existing code style and conventions
3. Add comprehensive unit tests for new functionality
4. Update documentation for any new features
5. Submit pull requests for review

See our [Contributing Guide](https://github.com/licheez/pvWayNugs/blob/main/CONTRIBUTING.md) for detailed guidelines.

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](https://github.com/licheez/pvWayNugs/blob/main/LICENSE) file for complete details.

## 🔗 Links

- **🏠 Homepage**: https://github.com/licheez/pvWayNugs
- **📦 NuGet Package**: https://www.nuget.org/packages/pvNugsSecretManagerNc9EnvVariables/
- **📖 Documentation**: https://github.com/licheez/pvWayNugs/wiki
- **🐛 Issues**: https://github.com/licheez/pvWayNugs/issues
- **💬 Discussions**: https://github.com/licheez/pvWayNugs/discussions

## 📞 Support & Community

- **🐛 Bug Reports**: [GitHub Issues](https://github.com/licheez/pvWayNugs/issues)
- **💡 Feature Requests**: [GitHub Issues](https://github.com/licheez/pvWayNugs/issues) with enhancement label
- **❓ Questions**: [GitHub Discussions](https://github.com/licheez/pvWayNugs/discussions)
- **📧 Enterprise Support**: Contact pvWay Ltd for commercial support options

---

**🏢 Built with ❤️ by pvWay Ltd** - Specialized in secure, enterprise-grade .NET solutions

**🔖 Keywords**: Secret Management, Configuration, Environment Variables, Azure Key Vault, Dynamic Credentials, .NET 9, Security, pvWayNugs, Enterprise

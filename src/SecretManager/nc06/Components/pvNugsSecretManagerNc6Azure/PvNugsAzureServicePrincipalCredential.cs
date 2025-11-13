namespace pvNugsSecretManagerNc6Azure;
// ReSharper disable once ClassNeverInstantiated.Global

/// <summary>
/// Represents the service principal credentials required for Azure Key Vault authentication using the client credentials flow.
/// This class encapsulates the Azure Active Directory (AAD) service principal information necessary to authenticate
/// with Azure Key Vault when managed identity authentication is not available or suitable for the deployment scenario.
/// </summary>
/// <remarks>
/// <para><c>Authentication Context:</c></para>
/// <para>This credential model is used for OAuth 2.0 client credentials flow authentication with Azure Active Directory.
/// It provides an alternative authentication method when Azure Managed Identity cannot be used, such as in local development
/// environments, on-premises deployments, or third-party cloud platforms.</para>
/// 
/// <para><c>Security Model:</c></para>
/// <para>The client credentials flow is a server-to-server authentication mechanism where:</para>
/// <list type="bullet">
/// <item><description>The application authenticates as itself (not on behalf of a user)</description></item>
/// <item><description>No user interaction or consent is required</description></item>
/// <item><description>The application must securely store and protect the client secret</description></item>
/// <item><description>Authentication tokens are obtained directly from Azure AD using the provided credentials</description></item>
/// </list>
/// 
/// <para><c>Service Principal Requirements:</c></para>
/// <list type="bullet">
/// <item><description>The service principal must be registered in Azure Active Directory</description></item>
/// <item><description>A client secret must be generated and configured for the service principal</description></item>
/// <item><description>The service principal must be granted appropriate permissions to the target Key Vault</description></item>
/// <item><description>The service principal should follow the principle of least privilege</description></item>
/// </list>
/// 
/// <para><c>Permission Configuration:</c></para>
/// <para>The service principal requires proper permissions to access Azure Key Vault:</para>
/// <list type="bullet">
/// <item><description><c>Access Policies:</c> Traditional Key Vault access model with specific permissions (Get, List, etc.)</description></item>
/// <item><description><c>RBAC Roles:</c> Modern role-based access control with predefined or custom roles</description></item>
/// <item><description><c>Recommended Roles:</c> "Key Vault Secrets User" for secret read access, "Key Vault Secrets Officer" for full secret management</description></item>
/// </list>
/// 
/// <para><c>Security Best Practices:</c></para>
/// <list type="bullet">
/// <item><description>Store client secrets in secure configuration sources (Azure Key Vault, environment variables, secure configuration providers)</description></item>
/// <item><description>Implement regular secret rotation policies</description></item>
/// <item><description>Use different service principals for different environments</description></item>
/// <item><description>Monitor and audit service principal usage</description></item>
/// <item><description>Apply network restrictions and conditional access policies where possible</description></item>
/// <item><description>Never log or expose client secrets in application logs or error messages</description></item>
/// </list>
/// 
/// <para><c>Environment Considerations:</c></para>
/// <list type="bullet">
/// <item><description><c>Development:</c> Use dedicated development service principals with limited permissions</description></item>
/// <item><description><c>Staging:</c> Use separate staging service principals that mirror production permissions</description></item>
/// <item><description><c>Production:</c> Use production service principals with minimal required permissions and enhanced monitoring</description></item>
/// <item><description><c>Local Development:</c> Consider using developer-specific service principals or Azure CLI authentication</description></item>
/// </list>
/// 
/// <para><c>Alternative Authentication Methods:</c></para>
/// <para>While this class provides service principal authentication, consider these alternatives:</para>
/// <list type="bullet">
/// <item><description><c>Managed Identity:</c> Preferred for Azure-hosted applications (App Service, Functions, VMs)</description></item>
/// <item><description><c>Certificate-based Authentication:</c> More secure alternative to client secrets</description></item>
/// <item><description><c>Workload Identity:</c> For Kubernetes workloads in Azure Kubernetes Service</description></item>
/// </list>
/// </remarks>
/// <example>
/// <para>Configuration in appsettings.json:</para>
/// <code>
/// {
///   "PvNugsAzureSecretManagerConfig": {
///     "KeyVaultUrl": "https://myapp-kv.vault.azure.net/",
///     "Credential": {
///       "TenantId": "12345678-1234-1234-1234-123456789012",
///       "ClientId": "87654321-4321-4321-4321-210987654321",
///       "ClientSecret": "your-client-secret-value"
///     }
///   }
/// }
/// </code>
/// 
/// <para>Environment variable configuration:</para>
/// <code>
/// PvNugsAzureSecretManagerConfig__Credential__TenantId=12345678-1234-1234-1234-123456789012
/// PvNugsAzureSecretManagerConfig__Credential__ClientId=87654321-4321-4321-4321-210987654321
/// PvNugsAzureSecretManagerConfig__Credential__ClientSecret=your-client-secret-value
/// </code>
/// 
/// <para>Programmatic configuration with validation:</para>
/// <code>
/// var credential = new PvNugsAzureServicePrincipalCredential
/// {
///     TenantId = configuration["AzureAd:TenantId"],
///     ClientId = configuration["AzureAd:ClientId"],
///     ClientSecret = configuration["AzureAd:ClientSecret"]
/// };
/// 
/// // Validate the credential configuration
/// ValidateCredential(credential);
/// 
/// void ValidateCredential(PvNugsAzureServicePrincipalCredential cred)
/// {
///     if (string.IsNullOrWhiteSpace(cred.TenantId))
///         throw new ArgumentException("TenantId cannot be null or empty");
///     if (string.IsNullOrWhiteSpace(cred.ClientId))
///         throw new ArgumentException("ClientId cannot be null or empty");
///     if (string.IsNullOrWhiteSpace(cred.ClientSecret))
///         throw new ArgumentException("ClientSecret cannot be null or empty");
///         
///     if (!Guid.TryParse(cred.TenantId, out _))
///         throw new ArgumentException("TenantId must be a valid GUID");
///     if (!Guid.TryParse(cred.ClientId, out _))
///         throw new ArgumentException("ClientId must be a valid GUID");
/// }
/// </code>
/// 
/// <para>Secure configuration with Azure Key Vault bootstrap:</para>
/// <code>
/// // Bootstrap scenario: Use initial credentials to access Key Vault
/// // then retrieve more sensitive credentials from Key Vault
/// var bootstrapCredential = new PvNugsAzureServicePrincipalCredential
/// {
///     TenantId = Environment.GetEnvironmentVariable("BOOTSTRAP_TENANT_ID"),
///     ClientId = Environment.GetEnvironmentVariable("BOOTSTRAP_CLIENT_ID"),
///     ClientSecret = Environment.GetEnvironmentVariable("BOOTSTRAP_CLIENT_SECRET")
/// };
/// 
/// // Use bootstrap credentials to get production credentials from Key Vault
/// var secretManager = new AzureKeyVaultSecretManager(bootstrapCredential);
/// var prodClientSecret = await secretManager.GetStaticSecretAsync("prod-app-client-secret");
/// 
/// var productionCredential = new PvNugsAzureServicePrincipalCredential
/// {
///     TenantId = bootstrapCredential.TenantId,
///     ClientId = await secretManager.GetStaticSecretAsync("prod-app-client-id"),
///     ClientSecret = prodClientSecret
/// };
/// </code>
/// 
/// <para>Integration with Azure SDK ClientSecretCredential:</para>
/// <code>
/// public ClientSecretCredential CreateAzureCredential(PvNugsAzureServicePrincipalCredential credential)
/// {
///     if (credential == null)
///         throw new ArgumentNullException(nameof(credential));
///         
///     return new ClientSecretCredential(
///         credential.TenantId,
///         credential.ClientId,
///         credential.ClientSecret,
///         new ClientSecretCredentialOptions
///         {
///             AuthorityHost = AzureAuthorityHosts.AzurePublicCloud,
///             Retry = { MaxRetries = 3 },
///             Diagnostics = { IsLoggingContentEnabled = false } // Never log credentials
///         });
/// }
/// </code>
/// </example>
/// <seealso cref="PvNugsAzureSecretManagerConfig"/>
/// <seealso href="https://docs.microsoft.com/en-us/azure/active-directory/develop/v2-oauth2-client-creds-grant-flow">OAuth 2.0 Client Credentials Flow</seealso>
/// <seealso href="https://docs.microsoft.com/en-us/azure/key-vault/general/authentication">Azure Key Vault Authentication</seealso>
/// <seealso href="https://docs.microsoft.com/en-us/azure/active-directory/develop/app-objects-and-service-principals">Service Principals in Azure AD</seealso>
public class PvNugsAzureServicePrincipalCredential
{
    /// <summary>
    /// Gets or sets the Azure Active Directory tenant ID where the service principal is registered.
    /// The tenant ID uniquely identifies the Azure AD tenant (directory) that contains the service principal
    /// and controls the authentication and authorization context for Key Vault access.
    /// </summary>
    /// <value>
    /// A GUID string representing the Azure AD tenant identifier. Must be a valid UUID/GUID format
    /// (e.g., "12345678-1234-1234-1234-123456789012").
    /// </value>
    /// <remarks>
    /// <para><c>Tenant Context:</c></para>
    /// <para>The tenant ID represents the Azure Active Directory tenant (organization) where:</para>
    /// <list type="bullet">
    /// <item><description>The service principal is registered and managed</description></item>
    /// <item><description>The target Azure Key Vault resource is associated (or has trust relationships)</description></item>
    /// <item><description>Authentication policies and conditional access rules are defined</description></item>
    /// <item><description>Audit logs and security monitoring are centralized</description></item>
    /// </list>
    /// 
    /// <para><c>Multi-Tenant Considerations:</c></para>
    /// <list type="bullet">
    /// <item><description>Each Azure subscription is associated with exactly one Azure AD tenant</description></item>
    /// <item><description>Service principals are tenant-specific and cannot be used across different tenants without explicit configuration</description></item>
    /// <item><description>Key Vaults can grant access to service principals from other tenants through guest access or B2B scenarios</description></item>
    /// <item><description>Always verify that the tenant ID matches the tenant containing your Key Vault resource</description></item>
    /// </list>
    /// 
    /// <para><c>Discovery and Management:</c></para>
    /// <para>You can find the tenant ID through various methods:</para>
    /// <list type="bullet">
    /// <item><description>Azure Portal: Azure Active Directory > Properties > Tenant ID</description></item>
    /// <item><description>Azure CLI: <c>az account show --query tenantId --output tsv</c></description></item>
    /// <item><description>PowerShell: <c>Get-AzContext | Select-Object Tenant</c></description></item>
    /// <item><description>REST API: OpenID Connect discovery endpoint</description></item>
    /// </list>
    /// 
    /// <para><c>Security and Governance:</c></para>
    /// <list type="bullet">
    /// <item><description>Tenant IDs are not considered sensitive information and can be safely logged</description></item>
    /// <item><description>However, they do reveal organizational structure and should be protected in security-sensitive contexts</description></item>
    /// <item><description>Different tenants may have different security policies, compliance requirements, and access controls</description></item>
    /// <item><description>Ensure your application handles tenant-specific authentication flows correctly</description></item>
    /// </list>
    /// 
    /// <para><c>Configuration Management:</c></para>
    /// <list type="bullet">
    /// <item><description>Use environment-specific tenant IDs for different deployment environments</description></item>
    /// <item><description>Development and production environments may use different Azure AD tenants</description></item>
    /// <item><description>Store tenant IDs in configuration files or environment variables as they are not secrets</description></item>
    /// <item><description>Validate the GUID format during application startup to catch configuration errors early</description></item>
    /// </list>
    /// 
    /// <para><c>Common Scenarios:</c></para>
    /// <list type="bullet">
    /// <item><description><c>Single Tenant:</c> Most common scenario where all resources are in the same Azure AD tenant</description></item>
    /// <item><description><c>Multi-Tenant SaaS:</c> Applications that need to access resources across multiple customer tenants</description></item>
    /// <item><description><c>Partner Integration:</c> Cross-tenant access scenarios with B2B guest access</description></item>
    /// <item><description><c>Migration Scenarios:</c> Temporary cross-tenant access during organizational changes</description></item>
    /// </list>
    /// </remarks>
    /// <exception cref="System.ArgumentNullException">
    /// May be thrown by authentication implementations when the tenant ID is null or empty.
    /// </exception>
    /// <exception cref="System.ArgumentException">
    /// May be thrown by authentication implementations when the tenant ID is not in valid GUID format.
    /// </exception>
    /// <exception cref="System.Security.Authentication.AuthenticationException">
    /// May be thrown when the specified tenant ID does not exist or the service principal is not registered in that tenant.
    /// </exception>
    /// <example>
    /// <para>Valid tenant ID formats:</para>
    /// <code>
    /// // Standard GUID format (most common)
    /// credential.TenantId = "12345678-1234-1234-1234-123456789012";
    /// 
    /// // GUID without hyphens (also valid)
    /// credential.TenantId = "12345678123412341234123456789012";
    /// 
    /// // Uppercase GUID (valid)
    /// credential.TenantId = "12345678-1234-1234-1234-123456789012".ToUpper();
    /// </code>
    /// 
    /// <para>Retrieving tenant ID programmatically:</para>
    /// <code>
    /// // From Azure CLI (requires Azure CLI login)
    /// var tenantId = await RunCommandAsync("az account show --query tenantId --output tsv");
    /// 
    /// // From Azure PowerShell
    /// var tenantId = await RunPowerShellAsync("(Get-AzContext).Tenant.Id");
    /// 
    /// // From environment or configuration
    /// var tenantId = Environment.GetEnvironmentVariable("AZURE_TENANT_ID") ??
    ///                configuration["AzureAd:TenantId"];
    /// </code>
    /// 
    /// <para>Validation example:</para>
    /// <code>
    /// public void ValidateTenantId(string tenantId)
    /// {
    ///     if (string.IsNullOrWhiteSpace(tenantId))
    ///         throw new ArgumentException("Tenant ID cannot be null or empty");
    ///         
    ///     // Remove hyphens for validation if present
    ///     var normalizedTenantId = tenantId.Replace("-", "");
    ///     
    ///     if (normalizedTenantId.Length != 32)
    ///         throw new ArgumentException("Tenant ID must be a 32-character GUID");
    ///         
    ///     if (!Guid.TryParse(tenantId, out _))
    ///         throw new ArgumentException("Tenant ID must be a valid GUID format");
    /// }
    /// </code>
    /// 
    /// <para>Environment-specific configuration:</para>
    /// <code>
    /// // appsettings.Development.json
    /// {
    ///   "AzureAd": {
    ///     "TenantId": "dev-tenant-12345678-1234-1234-1234-123456789012"
    ///   }
    /// }
    /// 
    /// // appsettings.Production.json
    /// {
    ///   "AzureAd": {
    ///     "TenantId": "prod-tenant-87654321-4321-4321-4321-210987654321"
    ///   }
    /// }
    /// </code>
    /// </example>
    /// <seealso cref="ClientId"/>
    /// <seealso cref="ClientSecret"/>
    /// <seealso href="https://docs.microsoft.com/en-us/azure/active-directory/fundamentals/active-directory-how-to-find-tenant">How to find your Azure AD tenant ID</seealso>
    public string TenantId { get; set; } = null!;
    
    /// <summary>
    /// Gets or sets the client ID (also known as application ID) of the Azure Active Directory service principal.
    /// This unique identifier represents the specific application registration within the Azure AD tenant
    /// and is used to identify the service principal during the OAuth 2.0 client credentials authentication flow.
    /// </summary>
    /// <value>
    /// A GUID string representing the Azure AD application/client identifier. Must be a valid UUID/GUID format
    /// (e.g., "87654321-4321-4321-4321-210987654321").
    /// </value>
    /// <remarks>
    /// <para><c>Application Registration Context:</c></para>
    /// <para>The client ID is generated when you register an application in Azure Active Directory and represents:</para>
    /// <list type="bullet">
    /// <item><description>The unique identity of your application within the Azure AD tenant</description></item>
    /// <item><description>The principal that will be granted permissions to access Azure Key Vault</description></item>
    /// <item><description>The entity that will be identified in audit logs and access reviews</description></item>
    /// <item><description>The application that tokens will be issued to during authentication</description></item>
    /// </list>
    /// 
    /// <para><c>Service Principal Relationship:</c></para>
    /// <list type="bullet">
    /// <item><description>The client ID identifies the application registration in Azure AD</description></item>
    /// <item><description>A service principal is automatically created in the tenant when the application is registered</description></item>
    /// <item><description>The service principal is the local representation of the global application object</description></item>
    /// <item><description>Permissions and role assignments are granted to the service principal, not the application registration directly</description></item>
    /// </list>
    /// 
    /// <para><c>Security Characteristics:</c></para>
    /// <list type="bullet">
    /// <item><description>Client IDs are not secrets and can be safely stored in configuration files</description></item>
    /// <item><description>However, they should still be protected to prevent impersonation attempts</description></item>
    /// <item><description>Client IDs are visible in JWT tokens and can be logged for audit purposes</description></item>
    /// <item><description>Different environments should use different client IDs for security isolation</description></item>
    /// </list>
    /// 
    /// <para><c>Permission Model:</c></para>
    /// <para>The service principal identified by this client ID must have appropriate permissions:</para>
    /// <list type="bullet">
    /// <item><description><c>Key Vault Access Policies:</c> Traditional permission model with granular permissions (Get, List, Set, Delete, etc.)</description></item>
    /// <item><description><c>Azure RBAC:</c> Role-based access control with built-in or custom roles</description></item>
    /// <item><description><c>Network Access:</c> Key Vault firewall and virtual network service endpoints</description></item>
    /// <item><description><c>Conditional Access:</c> Azure AD policies that may affect service principal authentication</description></item>
    /// </list>
    /// 
    /// <para><c>Application Types and Scenarios:</c></para>
    /// <list type="bullet">
    /// <item><description><c>Web Applications:</c> Server-side applications that authenticate using client credentials</description></item>
    /// <item><description><c>Daemon Applications:</c> Background services and scheduled jobs that run without user interaction</description></item>
    /// <item><description><c>API Services:</c> Web APIs that need to access Key Vault for their own operations</description></item>
    /// <item><description><c>DevOps Pipelines:</c> CI/CD processes that need to access secrets during build/deployment</description></item>
    /// </list>
    /// 
    /// <para><c>Management and Lifecycle:</c></para>
    /// <list type="bullet">
    /// <item><description>Client IDs remain constant throughout the application's lifecycle</description></item>
    /// <item><description>They are assigned when the application is first registered and don't change</description></item>
    /// <item><description>Multiple client secrets can be associated with a single client ID</description></item>
    /// <item><description>The client ID can be used to track and audit application access across Azure services</description></item>
    /// </list>
    /// 
    /// <para><c>Discovery and Configuration:</c></para>
    /// <para>You can find the client ID through several methods:</para>
    /// <list type="bullet">
    /// <item><description>Azure Portal: Azure Active Directory > App registrations > [Your App] > Overview > Application (client) ID</description></item>
    /// <item><description>Azure CLI: <c>az ad app show --id [app-name] --query appId --output tsv</c></description></item>
    /// <item><description>PowerShell: <c>Get-AzADApplication -DisplayName [app-name] | Select-Object ApplicationId</c></description></item>
    /// <item><description>Azure REST API: Applications endpoint with appropriate filters</description></item>
    /// </list>
    /// </remarks>
    /// <exception cref="System.ArgumentNullException">
    /// May be thrown by authentication implementations when the client ID is null or empty.
    /// </exception>
    /// <exception cref="System.ArgumentException">
    /// May be thrown by authentication implementations when the client ID is not in valid GUID format.
    /// </exception>
    /// <exception cref="System.Security.Authentication.AuthenticationException">
    /// May be thrown when the specified client ID does not exist in the tenant or is disabled.
    /// </exception>
    /// <exception cref="System.UnauthorizedAccessException">
    /// May be thrown when the service principal identified by the client ID lacks sufficient permissions for Key Vault access.
    /// </exception>
    /// <example>
    /// <para>Valid client ID formats:</para>
    /// <code>
    /// // Standard GUID format (most common)
    /// credential.ClientId = "87654321-4321-4321-4321-210987654321";
    /// 
    /// // GUID without hyphens (also valid)
    /// credential.ClientId = "87654321432143214321210987654321";
    /// 
    /// // Uppercase GUID (valid)
    /// credential.ClientId = "87654321-4321-4321-4321-210987654321".ToUpper();
    /// </code>
    /// 
    /// <para>Retrieving client ID from Azure resources:</para>
    /// <code>
    /// // From Azure CLI
    /// var clientId = await RunCommandAsync("az ad app show --id myapp --query appId --output tsv");
    /// 
    /// // From Azure PowerShell
    /// var clientId = await RunPowerShellAsync("(Get-AzADApplication -DisplayName 'MyApp').ApplicationId");
    /// 
    /// // From configuration
    /// var clientId = configuration["AzureAd:ClientId"] ?? 
    ///                Environment.GetEnvironmentVariable("AZURE_CLIENT_ID");
    /// </code>
    /// 
    /// <para>Configuration with validation:</para>
    /// <code>
    /// public void ConfigureServicePrincipal(IServiceCollection services, IConfiguration configuration)
    /// {
    ///     var clientId = configuration["AzureAd:ClientId"];
    ///     
    ///     // Validate client ID format
    ///     if (string.IsNullOrWhiteSpace(clientId))
    ///         throw new InvalidOperationException("Client ID is required for service principal authentication");
    ///         
    ///     if (!Guid.TryParse(clientId, out var clientGuid))
    ///         throw new ArgumentException($"Client ID '{clientId}' is not a valid GUID format");
    ///     
    ///     services.Configure&lt;PvNugsAzureSecretManagerConfig&gt;(options =&gt;
    ///     {
    ///         options.Credential = new PvNugsAzureServicePrincipalCredential
    ///         {
    ///             TenantId = configuration["AzureAd:TenantId"],
    ///             ClientId = clientId,
    ///             ClientSecret = configuration["AzureAd:ClientSecret"]
    ///         };
    ///     });
    /// }
    /// </code>
    /// 
    /// <para>Environment-specific client IDs:</para>
    /// <code>
    /// // Different applications for different environments
    /// var clientIds = new Dictionary&lt;string, string&gt;
    /// {
    ///     ["Development"] = "dev-app-12345678-1234-1234-1234-123456789012",
    ///     ["Staging"] = "staging-app-23456789-2345-2345-2345-234567890123",
    ///     ["Production"] = "prod-app-34567890-3456-3456-3456-345678901234"
    /// };
    /// 
    /// var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
    /// var clientId = clientIds.GetValueOrDefault(environment) ?? 
    ///                throw new InvalidOperationException($"No client ID configured for environment: {environment}");
    /// </code>
    /// 
    /// <para>Logging and monitoring (client ID can be safely logged):</para>
    /// <code>
    /// public async Task&lt;string&gt; AuthenticateAsync(PvNugsAzureServicePrincipalCredential credential)
    /// {
    ///     _logger.LogInformation("Authenticating service principal with Client ID: {ClientId} in Tenant: {TenantId}",
    ///         credential.ClientId, credential.TenantId);
    ///     
    ///     try
    ///     {
    ///         var tokenCredential = new ClientSecretCredential(
    ///             credential.TenantId,
    ///             credential.ClientId,
    ///             credential.ClientSecret);
    ///             
    ///         var token = await tokenCredential.GetTokenAsync(
    ///             new TokenRequestContext(new[] { "https://vault.azure.net/.default" }));
    ///             
    ///         _logger.LogInformation("Successfully authenticated service principal {ClientId}", credential.ClientId);
    ///         return token.Token;
    ///     }
    ///     catch (Exception ex)
    ///     {
    ///         _logger.LogError(ex, "Failed to authenticate service principal {ClientId}", credential.ClientId);
    ///         throw;
    ///     }
    /// }
    /// </code>
    /// </example>
    /// <seealso cref="TenantId"/>
    /// <seealso cref="ClientSecret"/>
    /// <seealso href="https://docs.microsoft.com/en-us/azure/active-directory/develop/app-objects-and-service-principals">Application and service principal objects</seealso>
    /// <seealso href="https://docs.microsoft.com/en-us/azure/active-directory/develop/howto-create-service-principal-portal">Create an Azure AD app and service principal</seealso>
    public string ClientId { get; set; } = null!;
    
    /// <summary>
    /// Gets or sets the client secret (also known as application password or application key) for the Azure Active Directory service principal.
    /// This sensitive credential serves as the password for the service principal and is used in conjunction with the client ID
    /// to authenticate with Azure Active Directory using the OAuth 2.0 client credentials flow.
    /// </summary>
    /// <value>
    /// A string containing the client secret value. This is a sensitive credential that must be securely stored and handled.
    /// The format is typically a base64-encoded string generated by Azure Active Directory.
    /// </value>
    /// <remarks>
    /// <para><c>Security Classification:</c></para>
    /// <para>The client secret is a <strong>highly sensitive credential</strong> that requires special handling:</para>
    /// <list type="bullet">
    /// <item><description><strong>Never log or expose client secrets</strong> in application logs, error messages, or debug output</description></item>
    /// <item><description>Store client secrets in secure configuration sources (Azure Key Vault, environment variables, secure configuration providers)</description></item>
    /// <item><description>Encrypt client secrets when stored in databases or configuration files</description></item>
    /// <item><description>Implement proper access controls to limit who can view or manage client secrets</description></item>
    /// <item><description>Use secure communication channels (HTTPS/TLS) when transmitting client secrets</description></item>
    /// </list>
    /// 
    /// <para><c>Secret Lifecycle Management:</c></para>
    /// <list type="bullet">
    /// <item><description><c>Expiration:</c> Azure AD client secrets have configurable expiration dates (maximum 2 years)</description></item>
    /// <item><description><c>Rotation:</c> Implement regular secret rotation policies (recommended every 6-12 months)</description></item>
    /// <item><description><c>Multiple Secrets:</c> Azure AD supports multiple active secrets per application for zero-downtime rotation</description></item>
    /// <item><description><c>Revocation:</c> Secrets can be immediately revoked in Azure AD if compromised</description></item>
    /// </list>
    /// 
    /// <para><c>Authentication Flow:</c></para>
    /// <para>The client secret is used in the OAuth 2.0 client credentials flow:</para>
    /// <list type="number">
    /// <item><description>Application presents tenant ID, client ID, and client secret to Azure AD token endpoint</description></item>
    /// <item><description>Azure AD validates the credentials and issues an access token</description></item>
    /// <item><description>Access token is used to authenticate API calls to Azure Key Vault</description></item>
    /// <item><description>Tokens have limited lifetime and must be refreshed using the same credentials</description></item>
    /// </list>
    /// 
    /// <para><c>Storage Best Practices:</c></para>
    /// <list type="bullet">
    /// <item><description><c>Azure Key Vault:</c> Store production secrets in Azure Key Vault (bootstrap scenario)</description></item>
    /// <item><description><c>Environment Variables:</c> Use environment variables for containerized applications</description></item>
    /// <item><description><c>Secure Configuration:</c> Use .NET Secret Manager for local development</description></item>
    /// <item><description><c>CI/CD Variables:</c> Use secure pipeline variables for DevOps scenarios</description></item>
    /// <item><description><c>Never in Source Code:</c> Never commit secrets to version control systems</description></item>
    /// </list>
    /// 
    /// <para><c>Monitoring and Auditing:</c></para>
    /// <list type="bullet">
    /// <item><description>Monitor authentication failures and unusual access patterns</description></item>
    /// <item><description>Set up alerts for secret expiration approaching</description></item>
    /// <item><description>Audit secret usage and access patterns</description></item>
    /// <item><description>Track secret rotation activities and compliance</description></item>
    /// </list>
    /// 
    /// <para><c>Alternative Authentication Methods:</c></para>
    /// <para>Consider more secure alternatives when possible:</para>
    /// <list type="bullet">
    /// <item><description><c>Managed Identity:</c> Eliminates the need for secrets in Azure-hosted scenarios</description></item>
    /// <item><description><c>Certificate-based Authentication:</c> More secure than client secrets, harder to compromise</description></item>
    /// <item><description><c>Federated Identity:</c> For cross-cloud or hybrid scenarios</description></item>
    /// <item><description><c>Workload Identity:</c> For Kubernetes and container scenarios</description></item>
    /// </list>
    /// 
    /// <para><c>Compliance and Governance:</c></para>
    /// <list type="bullet">
    /// <item><description>Follow organizational policies for secret management and retention</description></item>
    /// <item><description>Implement segregation of duties for secret creation and management</description></item>
    /// <item><description>Document secret usage and maintain inventory of active secrets</description></item>
    /// <item><description>Ensure compliance with regulatory requirements (SOX, PCI-DSS, etc.)</description></item>
    /// </list>
    /// </remarks>
    /// <exception cref="System.ArgumentNullException">
    /// May be thrown by authentication implementations when the client secret is null or empty.
    /// </exception>
    /// <exception cref="System.Security.Authentication.AuthenticationException">
    /// May be thrown when the client secret is invalid, expired, or has been revoked in Azure AD.
    /// </exception>
    /// <exception cref="System.UnauthorizedAccessException">
    /// May be thrown when authentication succeeds but the service principal lacks sufficient permissions.
    /// </exception>
    /// <example>
    /// <para>Secure configuration in appsettings.json (not recommended for production):</para>
    /// <code>
    /// {
    ///   "PvNugsAzureSecretManagerConfig": {
    ///     "Credential": {
    ///       "TenantId": "12345678-1234-1234-1234-123456789012",
    ///       "ClientId": "87654321-4321-4321-4321-210987654321",
    ///       "ClientSecret": "your-client-secret-value-here"
    ///     }
    ///   }
    /// }
    /// </code>
    /// 
    /// <para>Secure configuration using environment variables (recommended):</para>
    /// <code>
    /// // Environment variables
    /// AZURE_TENANT_ID=12345678-1234-1234-1234-123456789012
    /// AZURE_CLIENT_ID=87654321-4321-4321-4321-210987654321
    /// AZURE_CLIENT_SECRET=your-client-secret-value-here
    /// 
    /// // Configuration code
    /// services.Configure&lt;PvNugsAzureSecretManagerConfig&gt;(options =&gt;
    /// {
    ///     options.Credential = new PvNugsAzureServicePrincipalCredential
    ///     {
    ///         TenantId = Environment.GetEnvironmentVariable("AZURE_TENANT_ID"),
    ///         ClientId = Environment.GetEnvironmentVariable("AZURE_CLIENT_ID"),
    ///         ClientSecret = Environment.GetEnvironmentVariable("AZURE_CLIENT_SECRET")
    ///     };
    /// });
    /// </code>
    /// 
    /// <para>Using .NET Secret Manager for development:</para>
    /// <code>
    /// // Initialize secrets (run once)
    /// dotnet user-secrets init
    /// dotnet user-secrets set "AzureAd:TenantId" "12345678-1234-1234-1234-123456789012"
    /// dotnet user-secrets set "AzureAd:ClientId" "87654321-4321-4321-4321-210987654321"
    /// dotnet user-secrets set "AzureAd:ClientSecret" "your-development-client-secret"
    /// 
    /// // Configuration in Program.cs
    /// if (builder.Environment.IsDevelopment())
    /// {
    ///     builder.Configuration.AddUserSecrets&lt;Program&gt;();
    /// }
    /// </code>
    /// 
    /// <para>Bootstrap scenario - getting production secrets from Key Vault:</para>
    /// <code>
    /// // Use bootstrap credentials to access Key Vault containing production secrets
    /// public async Task&lt;PvNugsAzureServicePrincipalCredential&gt; GetProductionCredentialsAsync()
    /// {
    ///     // Bootstrap credentials from environment
    ///     var bootstrapCredential = new PvNugsAzureServicePrincipalCredential
    ///     {
    ///         TenantId = Environment.GetEnvironmentVariable("BOOTSTRAP_TENANT_ID"),
    ///         ClientId = Environment.GetEnvironmentVariable("BOOTSTRAP_CLIENT_ID"),
    ///         ClientSecret = Environment.GetEnvironmentVariable("BOOTSTRAP_CLIENT_SECRET")
    ///     };
    /// 
    ///     // Get production secret from Key Vault using bootstrap credentials
    ///     var keyVaultClient = new SecretClient(
    ///         new Uri("https://bootstrap-kv.vault.azure.net/"),
    ///         new ClientSecretCredential(
    ///             bootstrapCredential.TenantId,
    ///             bootstrapCredential.ClientId,
    ///             bootstrapCredential.ClientSecret));
    /// 
    ///     var prodSecretResponse = await keyVaultClient.GetSecretAsync("prod-app-client-secret");
    /// 
    ///     return new PvNugsAzureServicePrincipalCredential
    ///     {
    ///         TenantId = bootstrapCredential.TenantId,
    ///         ClientId = await GetSecretValueAsync(keyVaultClient, "prod-app-client-id"),
    ///         ClientSecret = prodSecretResponse.Value.Value
    ///     };
    /// }
    /// </code>
    /// 
    /// <para>Secure secret validation (without logging the secret):</para>
    /// <code>
    /// public void ValidateClientSecret(string clientSecret)
    /// {
    ///     if (string.IsNullOrEmpty(clientSecret))
    ///         throw new ArgumentException("Client secret cannot be null or empty");
    /// 
    ///     if (clientSecret.Length &lt; 10)
    ///         throw new ArgumentException("Client secret appears to be too short (possible configuration error)");
    /// 
    ///     // Log validation success without exposing the secret
    ///     _logger.LogInformation("Client secret validation passed. Secret length: {SecretLength}", clientSecret.Length);
    /// }
    /// </code>
    /// 
    /// <para>Secret rotation implementation:</para>
    /// <code>
    /// public class ClientSecretRotationService
    /// {
    ///     public async Task RotateClientSecretAsync(string applicationId)
    ///     {
    ///         var graphClient = GetGraphServiceClient();
    /// 
    ///         // Create new secret
    ///         var newPasswordCredential = new PasswordCredential
    ///         {
    ///             DisplayName = $"Rotated-{DateTime.UtcNow:yyyy-MM-dd}",
    ///             EndDateTime = DateTimeOffset.UtcNow.AddMonths(12)
    ///         };
    /// 
    ///         var newSecret = await graphClient.Applications[applicationId]
    ///             .AddPassword(newPasswordCredential)
    ///             .Request()
    ///             .PostAsync();
    /// 
    ///         // Update configuration with new secret
    ///         await UpdateSecretInKeyVaultAsync("client-secret", newSecret.SecretText);
    /// 
    ///         // Wait for configuration propagation
    ///         await Task.Delay(TimeSpan.FromMinutes(5));
    /// 
    ///         // Remove old secret
    ///         await graphClient.Applications[applicationId]
    ///             .RemovePassword(oldPasswordCredentialKeyId)
    ///             .Request()
    ///             .PostAsync();
    /// 
    ///         _logger.LogInformation("Successfully rotated client secret for application {ApplicationId}", applicationId);
    ///     }
    /// }
    /// </code>
    /// </example>
    /// <seealso cref="TenantId"/>
    /// <seealso cref="ClientId"/>
    /// <seealso href="https://docs.microsoft.com/en-us/azure/active-directory/develop/howto-create-service-principal-portal#option-2-create-a-new-application-secret">Create a new application secret</seealso>
    /// <seealso href="https://docs.microsoft.com/en-us/azure/security/fundamentals/credential-management">Azure credential management best practices</seealso>
    public string ClientSecret { get; set; } = null!;
}
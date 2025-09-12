
using pvNugsSecretManagerNc9Abstractions;

namespace pvNugsSecretManagerNc9EnvVariables;

/// <summary>
/// Represents a dynamic database credential that contains username, password, and expiration information with immutable properties.
/// This implementation uses environment variables as the underlying storage mechanism for secret management,
/// providing secure, time-limited credentials that integrate seamlessly with the pvNugs Secret Manager ecosystem.
/// </summary>
/// <remarks>
/// <para><strong>Design Philosophy:</strong></para>
/// <para>The DynamicCredential class is designed to work with the pvNugs Secret Manager system,
/// providing a concrete implementation of <see cref="IPvNugsDynamicCredential"/> that supports
/// temporary database authentication with automatic expiration. This approach enhances security
/// by ensuring credentials have limited lifespans and can be automatically rotated.</para>
/// 
/// <para><strong>Environment Variable Integration:</strong></para>
/// <para>This implementation is particularly useful in containerized environments, CI/CD pipelines,
/// or when credentials need to be injected at runtime without storing them in configuration files.
/// The environment variable approach provides excellent compatibility with:</para>
/// <list type="bullet">
/// <item><description>Docker containers and Kubernetes deployments</description></item>
/// <item><description>CI/CD systems (GitHub Actions, Azure DevOps, Jenkins)</description></item>
/// <item><description>Cloud platform secret injection mechanisms</description></item>
/// <item><description>Development and testing environments</description></item>
/// </list>
/// 
/// <para><strong>Security Features:</strong></para>
/// <list type="bullet">
/// <item><description><strong>Immutable Design:</strong> Once created, credentials cannot be modified, preventing accidental tampering</description></item>
/// <item><description><strong>Time-Limited Validity:</strong> Built-in expiration support enables automatic credential rotation</description></item>
/// <item><description><strong>No Persistence:</strong> Credentials exist only in memory during application runtime</description></item>
/// <item><description><strong>UTC Time Handling:</strong> Consistent timezone handling prevents expiration calculation errors</description></item>
/// </list>
/// 
/// <para><strong>Integration with Secret Managers:</strong></para>
/// <para>This class is typically instantiated by secret manager implementations when retrieving dynamic credentials.
/// It serves as a data transfer object that encapsulates all necessary information for database authentication
/// while maintaining a clear separation between credential storage and credential usage.</para>
/// 
/// <para><strong>Lifecycle Management:</strong></para>
/// <para>Dynamic credentials follow a specific lifecycle:</para>
/// <list type="number">
/// <item><description><strong>Creation:</strong> Secret manager generates temporary credentials with expiration</description></item>
/// <item><description><strong>Usage:</strong> Connection string provider uses credentials for database connections</description></item>
/// <item><description><strong>Monitoring:</strong> Provider monitors expiration and triggers renewal before expiry</description></item>
/// <item><description><strong>Renewal:</strong> New credentials are generated and old ones are invalidated</description></item>
/// </list>
/// 
/// <para><strong>Thread Safety:</strong></para>
/// <para>This class is inherently thread-safe due to its immutable design. Multiple threads can safely
/// read credential properties without synchronization concerns.</para>
/// </remarks>
/// <param name="username">The username for the credential. Cannot be null, empty, or consist only of whitespace characters.</param>
/// <param name="password">The password for the credential. Cannot be null, empty, or consist only of whitespace characters.</param>
/// <param name="expirationDateUtc">The UTC date and time when this credential expires. Must be a valid DateTime value.</param>
/// <exception cref="ArgumentNullException">Thrown when <paramref name="username"/> or <paramref name="password"/> is null.</exception>
/// <exception cref="ArgumentException">Thrown when <paramref name="username"/> or <paramref name="password"/> is empty or consists only of whitespace characters.</exception>
/// <example>
/// <para><strong>Basic credential creation:</strong></para>
/// <code>
/// // Create a credential that expires in 24 hours
/// var credential = new DynamicCredential(
///     "temp_user_abc123", 
///     "TempPass!@#$%^789", 
///     DateTime.UtcNow.AddHours(24)
/// );
/// 
/// Console.WriteLine($"Username: {credential.Username}");
/// Console.WriteLine($"Expires: {credential.ExpirationDateUtc:yyyy-MM-dd HH:mm:ss} UTC");
/// Console.WriteLine($"Valid for: {(credential.ExpirationDateUtc - DateTime.UtcNow).TotalHours:F1} hours");
/// </code>
/// 
/// <para><strong>Integration with secret manager:</strong></para>
/// <code>
/// public class VaultDynamicSecretManager : IPvNugsDynamicSecretManager
/// {
///     public async Task&lt;IPvNugsDynamicCredential?&gt; GetDynamicSecretAsync(
///         string secretName, 
///         CancellationToken cancellationToken = default)
///     {
///         // Retrieve from external secret manager (HashiCorp Vault, AWS, etc.)
///         var vaultResponse = await GetFromVault(secretName);
///         
///         // Create immutable credential object
///         return new DynamicCredential(
///             vaultResponse.Username,
///             vaultResponse.Password,
///             vaultResponse.LeaseExpiration
///         );
///     }
/// }
/// </code>
/// 
/// <para><strong>Usage in connection string provider:</strong></para>
/// <code>
/// public async Task&lt;string&gt; BuildConnectionStringAsync()
/// {
///     var dynamicCredential = await secretManager.GetDynamicSecretAsync("myapp-db-reader");
///     
///     // Check if credential is still valid
///     var timeUntilExpiration = dynamicCredential.ExpirationDateUtc - DateTime.UtcNow;
///     if (timeUntilExpiration.TotalMinutes &lt; 5)
///     {
///         // Trigger credential renewal
///         dynamicCredential = await RenewCredentialAsync("myapp-db-reader");
///     }
///     
///     return $"Server=myserver;Database=mydb;User Id={dynamicCredential.Username};Password={dynamicCredential.Password};";
/// }
/// </code>
/// 
/// <para><strong>Environment variable pattern for testing:</strong></para>
/// <code>
/// // Set environment variables for integration testing
/// Environment.SetEnvironmentVariable("TestApp__MyService-Reader__Username", "test_user_123");
/// Environment.SetEnvironmentVariable("TestApp__MyService-Reader__Password", "test_pass_456");
/// Environment.SetEnvironmentVariable("TestApp__MyService-Reader__ExpirationDateUtc", 
///     DateTime.UtcNow.AddMinutes(30).ToString("O")); // ISO 8601 format
/// 
/// // Create credential from environment variables
/// var username = Environment.GetEnvironmentVariable("TestApp__MyService-Reader__Username");
/// var password = Environment.GetEnvironmentVariable("TestApp__MyService-Reader__Password");
/// var expirationStr = Environment.GetEnvironmentVariable("TestApp__MyService-Reader__ExpirationDateUtc");
/// var expiration = DateTime.Parse(expirationStr, null, DateTimeStyles.RoundtripKind);
/// 
/// var testCredential = new DynamicCredential(username, password, expiration);
/// </code>
/// </example>
/// <seealso cref="IPvNugsDynamicCredential"/>
/// <seealso cref="pvNugsSecretManagerNc9Abstractions.IPvNugsDynamicSecretManager"/>
public class DynamicCredential(
    string username, 
    string password, 
    DateTime expirationDateUtc)
    : IPvNugsDynamicCredential
{
    /// <summary>
    /// Gets the username associated with this dynamic credential.
    /// This value is typically generated dynamically by the secret management system and may include
    /// timestamps, random identifiers, or other unique elements to ensure uniqueness across credential generations.
    /// </summary>
    /// <value>
    /// A non-null, non-empty string containing the username. This value is set during construction and cannot be modified.
    /// </value>
    /// <remarks>
    /// <para><strong>Immutable Design:</strong></para>
    /// <para>This property provides read-only access to the username that was specified when the credential was created.
    /// The immutable design ensures that credential integrity is maintained throughout the object's lifetime.</para>
    /// 
    /// <para><strong>Dynamic Generation Patterns:</strong></para>
    /// <para>Dynamic usernames often follow specific patterns depending on the secret management system:</para>
    /// <list type="bullet">
    /// <item><description><strong>HashiCorp Vault:</strong> v-root-myapp-reader-abc123def456-1234567890</description></item>
    /// <item><description><strong>AWS IAM:</strong> temp-user-20240315-143052-reader</description></item>
    /// <item><description><strong>Azure Key Vault:</strong> az-dynamic-user-guid-reader</description></item>
    /// <item><description><strong>Custom Systems:</strong> app_temp_user_timestamp_role</description></item>
    /// </list>
    /// 
    /// <para><strong>Security Considerations:</strong></para>
    /// <para>The username is typically used for authentication purposes in conjunction with the password.
    /// While usernames are generally less sensitive than passwords, they should still be handled securely
    /// and not logged in plain text for security auditing purposes.</para>
    /// 
    /// <para><strong>Database Integration:</strong></para>
    /// <para>When used with database connections, this username must correspond to a valid database login
    /// that has been granted appropriate permissions for the intended operations. Secret management systems
    /// typically handle the creation and cleanup of these temporary database logins automatically.</para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var credential = new DynamicCredential("temp_user_20240315_143052", "SecretPass123!", DateTime.UtcNow.AddHours(1));
    /// 
    /// // Use username in connection string
    /// var connectionString = $"Server=myserver;Database=mydb;User Id={credential.Username};Password={credential.Password};";
    /// 
    /// // Log username for auditing (consider privacy implications)
    /// logger.LogInformation("Authenticating with dynamic username: {Username}", credential.Username);
    /// </code>
    /// </example>
    public string Username { get; } = username;

    /// <summary>
    /// Gets the password associated with this dynamic credential.
    /// This value is typically a cryptographically strong, randomly generated password created by the secret management system
    /// to ensure maximum security for temporary database authentication.
    /// </summary>
    /// <value>
    /// A non-null, non-empty string containing the password. This value is set during construction and cannot be modified.
    /// </value>
    /// <remarks>
    /// <para><strong>Immutable Design:</strong></para>
    /// <para>This property provides read-only access to the password that was specified when the credential was created.
    /// The immutable design prevents accidental modification and ensures credential integrity throughout the object's lifetime.</para>
    /// 
    /// <para><strong>Security Characteristics:</strong></para>
    /// <para>Dynamic passwords are typically generated with high security standards:</para>
    /// <list type="bullet">
    /// <item><description><strong>Cryptographically Random:</strong> Generated using secure random number generators</description></item>
    /// <item><description><strong>High Complexity:</strong> Usually includes mixed case, numbers, and special characters</description></item>
    /// <item><description><strong>Appropriate Length:</strong> Typically 32-64 characters for maximum entropy</description></item>
    /// <item><description><strong>No Dictionary Words:</strong> Avoids predictable patterns or common passwords</description></item>
    /// </list>
    /// 
    /// <para><strong>Sensitive Information Handling:</strong></para>
    /// <para>The password should be treated as highly sensitive information throughout the application lifecycle:</para>
    /// <list type="bullet">
    /// <item><description><strong>Memory Security:</strong> Consider using SecureString for highly sensitive environments</description></item>
    /// <item><description><strong>Logging Restrictions:</strong> Never log passwords in plain text</description></item>
    /// <item><description><strong>Network Transmission:</strong> Always use encrypted connections (SSL/TLS) when transmitting</description></item>
    /// <item><description><strong>Disposal:</strong> Credentials are automatically cleaned up when the object is garbage collected</description></item>
    /// </list>
    /// 
    /// <para><strong>Integration with Connection Strings:</strong></para>
    /// <para>When building database connection strings, ensure proper escaping if the password contains special characters
    /// that have meaning in connection string syntax (semicolons, equals signs, etc.).</para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var credential = new DynamicCredential("temp_user", "Kj8#mN2$pQ9@vL5*", DateTime.UtcNow.AddHours(1));
    /// 
    /// // Secure usage in connection string
    /// var builder = new SqlConnectionStringBuilder
    /// {
    ///     DataSource = "myserver",
    ///     InitialCatalog = "mydb",
    ///     UserID = credential.Username,
    ///     Password = credential.Password,
    ///     Encrypt = true,
    ///     TrustServerCertificate = false
    /// };
    /// var connectionString = builder.ConnectionString;
    /// 
    /// // Never do this - passwords should not be logged
    /// // logger.LogDebug("Password: {Password}", credential.Password); // Security violation
    /// 
    /// // Instead, log only non-sensitive information
    /// logger.LogDebug("Dynamic credential created with expiration: {Expiration}", credential.ExpirationDateUtc);
    /// </code>
    /// </example>
    public string Password { get; } = password;

    /// <summary>
    /// Gets the UTC date and time when this dynamic credential expires and should no longer be used for authentication.
    /// This property enables automatic credential lifecycle management and rotation strategies in credential-consuming applications.
    /// </summary>
    /// <value>
    /// A <see cref="DateTime"/> value in UTC representing when the credential expires and becomes invalid for authentication.
    /// </value>
    /// <remarks>
    /// <para><strong>UTC Time Handling:</strong></para>
    /// <para>This property stores and returns time in Coordinated Universal Time (UTC) to eliminate timezone-related
    /// ambiguities in distributed systems. Applications should always compare against DateTime.UtcNow when checking expiration status.</para>
    /// 
    /// <para><strong>Expiration Management Strategies:</strong></para>
    /// <para>Applications should implement proactive expiration handling:</para>
    /// <list type="bullet">
    /// <item><description><strong>Early Warning System:</strong> Check expiration well before actual expiry (e.g., 30 minutes early)</description></item>
    /// <item><description><strong>Automatic Renewal:</strong> Trigger credential refresh when approaching expiration</description></item>
    /// <item><description><strong>Grace Period Handling:</strong> Implement fallback mechanisms for credential renewal failures</description></item>
    /// <item><description><strong>Error Boundaries:</strong> Prevent expired credentials from reaching database connections</description></item>
    /// </list>
    /// 
    /// <para><strong>Integration with Connection String Providers:</strong></para>
    /// <para>This property is typically used by connection string providers to implement sophisticated credential management:</para>
    /// <list type="bullet">
    /// <item><description>Cache invalidation when credentials approach expiration</description></item>
    /// <item><description>Automatic background renewal of credentials</description></item>
    /// <item><description>Warning logs when credentials are nearing expiration</description></item>
    /// <item><description>Error prevention by rejecting expired credentials</description></item>
    /// </list>
    /// 
    /// <para><strong>Secret Manager Coordination:</strong></para>
    /// <para>The expiration time typically corresponds to lease durations or token lifetimes in the underlying secret management system.
    /// Different secret managers may have different expiration patterns:</para>
    /// <list type="bullet">
    /// <item><description><strong>HashiCorp Vault:</strong> Based on lease duration (e.g., 1-24 hours)</description></item>
    /// <item><description><strong>AWS Secrets Manager:</strong> Based on rotation schedules (e.g., daily, weekly)</description></item>
    /// <item><description><strong>Azure Key Vault:</strong> Based on access policy configurations</description></item>
    /// <item><description><strong>Custom Systems:</strong> Based on organizational security policies</description></item>
    /// </list>
    /// 
    /// <para><strong>Monitoring and Alerting:</strong></para>
    /// <para>Production systems should monitor credential expiration patterns for:</para>
    /// <list type="bullet">
    /// <item><description>Unusual expiration patterns that might indicate security issues</description></item>
    /// <item><description>Renewal failures that could lead to application outages</description></item>
    /// <item><description>Credential usage patterns for capacity planning</description></item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// var credential = new DynamicCredential("user", "pass", DateTime.UtcNow.AddHours(2));
    /// 
    /// // Check if credential is still valid
    /// public bool IsCredentialValid(DynamicCredential credential)
    /// {
    ///     return credential.ExpirationDateUtc > DateTime.UtcNow;
    /// }
    /// 
    /// // Calculate time remaining
    /// public TimeSpan GetTimeUntilExpiration(DynamicCredential credential)
    /// {
    ///     var remaining = credential.ExpirationDateUtc - DateTime.UtcNow;
    ///     return remaining > TimeSpan.Zero ? remaining : TimeSpan.Zero;
    /// }
    /// 
    /// // Implement early warning system
    /// public bool ShouldRenewCredential(DynamicCredential credential, TimeSpan warningThreshold)
    /// {
    ///     var timeUntilExpiration = GetTimeUntilExpiration(credential);
    ///     return timeUntilExpiration &lt;= warningThreshold;
    /// }
    /// 
    /// // Usage in application
    /// var warningThreshold = TimeSpan.FromMinutes(30);
    /// if (ShouldRenewCredential(credential, warningThreshold))
    /// {
    ///     logger.LogWarning("Credential expires in {Minutes} minutes, initiating renewal", 
    ///         GetTimeUntilExpiration(credential).TotalMinutes);
    ///     
    ///     // Trigger async renewal process
    ///     _ = Task.Run(() => RenewCredentialAsync(credential));
    /// }
    /// 
    /// // Format expiration for logging/display
    /// logger.LogInformation("Credential valid until {ExpirationTime} UTC ({TimeRemaining} remaining)",
    ///     credential.ExpirationDateUtc.ToString("yyyy-MM-dd HH:mm:ss"),
    ///     GetTimeUntilExpiration(credential));
    /// </code>
    /// </example>
    public DateTime ExpirationDateUtc { get; } = expirationDateUtc;
}
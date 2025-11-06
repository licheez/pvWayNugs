namespace pvNugsSecretManagerNc6Abstractions;

/// <summary>
/// Represents a temporary database credential with automatic expiration, providing time-limited access to secure resources.
/// This interface encapsulates dynamically generated username and password pairs that are designed for high-security environments
/// where credential rotation and zero-trust principles are required.
/// </summary>
/// <remarks>
/// <para><c>Purpose and Design:</c></para>
/// <para>Dynamic credentials are temporary authentication tokens generated on-demand by secret management systems
/// such as HashiCorp Vault, AWS Secrets Manager, or Azure Key Vault. Unlike static credentials stored in configuration
/// or secret stores, dynamic credentials are created with a predetermined lifespan and automatically become invalid
/// after their expiration time.</para>
/// 
/// <para><c>Security Benefits:</c></para>
/// <list type="bullet">
/// <item><description><c>Time-Limited Exposure:</c> Credentials automatically expire, reducing the window of vulnerability if compromised.</description></item>
/// <item><description><c>Automatic Rotation:</c> No manual intervention required for credential updates.</description></item>
/// <item><description><c>Zero Persistent Storage:</c> No long-lived credentials stored in configuration files or databases.</description></item>
/// <item><description><c>Audit Trail:</c> Each credential generation event can be logged and monitored.</description></item>
/// <item><description><c>Principle of Least Privilege:</c> Credentials can be generated with specific permissions for limited time periods.</description></item>
/// </list>
/// 
/// <para><c>Lifecycle Management:</c></para>
/// <list type="number">
/// <item><description><c>Generation:</c> Credentials are created by the secret management system with a specific expiration time.</description></item>
/// <item><description><c>Distribution:</c> The credential object is returned to the requesting application.</description></item>
/// <item><description><c>Usage:</c> Application uses the username and password for database connections or API calls.</description></item>
/// <item><description><c>Monitoring:</c> Application monitors expiration time and requests renewal before expiry.</description></item>
/// <item><description><c>Expiration:</c> Credentials become invalid automatically at the specified expiration time.</description></item>
/// <item><description><c>Cleanup:</c> Secret management system revokes access and removes the credential from active systems.</description></item>
/// </list>
/// 
/// <para><c>Implementation Considerations:</c></para>
/// <list type="bullet">
/// <item><description>Implementations should be immutable to prevent accidental credential modification.</description></item>
/// <item><description>Credentials should not be serialized or logged to prevent exposure.</description></item>
/// <item><description>The expiration time should be checked before each use to ensure validity.</description></item>
/// <item><description>Applications should implement renewal logic well before expiration (e.g., 5-10 minutes before).</description></item>
/// </list>
/// 
/// <para><c>Common Usage Patterns:</c></para>
/// <list type="bullet">
/// <item><description>Database connection establishment with temporary users</description></item>
/// <item><description>API authentication with time-limited tokens</description></item>
/// <item><description>Multi-tenant applications requiring credential isolation</description></item>
/// <item><description>Compliance environments requiring regular credential rotation</description></item>
/// </list>
/// </remarks>
/// <example>
/// <para>Basic credential usage with expiration check:</para>
/// <code>
/// public async Task&lt;List&lt;Customer&gt;&gt; GetCustomersAsync()
/// {
///     var credential = await _secretManager.GetDynamicSecretAsync("customer-db");
///     
///     if (credential == null)
///         throw new InvalidOperationException("Failed to obtain database credentials");
///     
///     // Always check expiration before use
///     if (DateTime.UtcNow >= credential.ExpirationDateUtc)
///         throw new InvalidOperationException("Database credential has expired");
///     
///     var connectionString = $"Server=myserver;Database=customers;Username={credential.Username};Password={credential.Password};";
///     
///     // Use the credential for database operations
///     await using var connection = new NpgsqlConnection(connectionString);
///     await connection.OpenAsync();
///     
///     // Perform database operations...
///     return customers;
/// }
/// </code>
/// 
/// <para>Advanced usage with proactive renewal:</para>
/// <code>
/// public class DatabaseService
/// {
///     private readonly IPvNugsDynamicSecretManager _secretManager;
///     private readonly TimeSpan _renewalBuffer = TimeSpan.FromMinutes(5);
///     
///     public async Task&lt;T&gt; ExecuteWithCredentialAsync&lt;T&gt;(Func&lt;IPvNugsDynamicCredential, Task&lt;T&gt;&gt; operation)
///     {
///         var credential = await _secretManager.GetDynamicSecretAsync("app-database");
///         
///         if (credential == null)
///             throw new InvalidOperationException("Unable to obtain credentials");
///         
///         // Check if credential needs renewal soon
///         var timeToExpiry = credential.ExpirationDateUtc - DateTime.UtcNow;
///         if (timeToExpiry &lt;= _renewalBuffer)
///         {
///             _logger.LogWarning("Credential expires in {TimeToExpiry}, requesting renewal", timeToExpiry);
///             // Trigger background renewal or get fresh credential
///             credential = await _secretManager.GetDynamicSecretAsync("app-database");
///         }
///         
///         return await operation(credential);
///     }
/// }
/// </code>
/// 
/// <para>Credential validation helper:</para>
/// <code>
/// public static class CredentialValidator
/// {
///     public static bool IsValid(IPvNugsDynamicCredential credential, TimeSpan? buffer = null)
///     {
///         if (credential == null) return false;
///         
///         var effectiveExpiration = credential.ExpirationDateUtc;
///         if (buffer.HasValue)
///         {
///             effectiveExpiration = effectiveExpiration.Subtract(buffer.Value);
///         }
///         
///         return DateTime.UtcNow &lt; effectiveExpiration;
///     }
///     
///     public static TimeSpan? GetTimeToExpiry(IPvNugsDynamicCredential credential)
///     {
///         if (credential == null) return null;
///         
///         var timeToExpiry = credential.ExpirationDateUtc - DateTime.UtcNow;
///         return timeToExpiry > TimeSpan.Zero ? timeToExpiry : TimeSpan.Zero;
///     }
/// }
/// </code>
/// </example>
/// <seealso cref="IPvNugsDynamicSecretManager"/>
/// <seealso cref="IPvNugsDynamicSecretManager.GetDynamicSecretAsync(string, CancellationToken)"/>
public interface IPvNugsDynamicCredential
{
    /// <summary>
    /// Gets the dynamically generated username for authentication.
    /// This username is typically unique for each credential generation request and may include
    /// timestamps, random identifiers, or role-specific prefixes depending on the secret management system configuration.
    /// </summary>
    /// <value>
    /// A string containing the temporary username for database or service authentication.
    /// The format and length may vary depending on the underlying secret management system and configured policies.
    /// </value>
    /// <remarks>
    /// <para><c>Characteristics:</c></para>
    /// <list type="bullet">
    /// <item><description><c>Uniqueness:</c> Each generated credential typically has a unique username to avoid conflicts.</description></item>
    /// <item><description><c>Temporary Nature:</c> The username is valid only until <see cref="ExpirationDateUtc"/>.</description></item>
    /// <item><description><c>Format Variation:</c> May include prefixes, suffixes, or identifiers based on the generation policy.</description></item>
    /// <item><description><c>Security:</c> Should be treated as sensitive information and not logged in plain text.</description></item>
    /// </list>
    /// 
    /// <para><c>Common Formats:</c></para>
    /// <list type="bullet">
    /// <item><description><c>HashiCorp Vault:</c> "v-role-uuid-timestamp" (e.g., "v-readonly-abc123-1234567890")</description></item>
    /// <item><description><c>AWS RDS:</c> Role-based with random suffix (e.g., "app-reader-xyz789")</description></item>
    /// <item><description><c>Azure Database:</c> Service principal based with timestamp (e.g., "sp-app-20231201-123456")</description></item>
    /// </list>
    /// 
    /// <para><c>Usage Considerations:</c></para>
    /// <list type="bullet">
    /// <item><description>Always verify the credential hasn't expired before using the username.</description></item>
    /// <item><description>The username may be subject to database-specific character limitations.</description></item>
    /// <item><description>Some systems may reuse usernames after expiration, but this should not be assumed.</description></item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// public string BuildConnectionString(IPvNugsDynamicCredential credential)
    /// {
    ///     // Always validate expiration before using credentials
    ///     if (DateTime.UtcNow >= credential.ExpirationDateUtc)
    ///         throw new InvalidOperationException("Cannot use expired credential");
    ///         
    ///     return $"Server=myserver;Database=mydb;Username={credential.Username};Password={credential.Password};";
    /// }
    /// </code>
    /// </example>
    string Username { get; }

    /// <summary>
    /// Gets the dynamically generated password for authentication.
    /// This password is cryptographically generated and typically has high entropy to ensure security.
    /// The password is valid only in combination with the corresponding <see cref="Username"/> and only until <see cref="ExpirationDateUtc"/>.
    /// </summary>
    /// <value>
    /// A string containing the temporary password for database or service authentication.
    /// The password is generated according to the security policies configured in the secret management system.
    /// </value>
    /// <remarks>
    /// <para><c>Security Characteristics:</c></para>
    /// <list type="bullet">
    /// <item><description><c>High Entropy:</c> Generated using cryptographically secure random number generators.</description></item>
    /// <item><description><c>Policy Compliance:</c> Meets the password complexity requirements of the target system.</description></item>
    /// <item><description><c>Unique Generation:</c> Each password is unique and not predictable from previous generations.</description></item>
    /// <item><description><c>Time-Limited:</c> Valid only until the expiration time specified in <see cref="ExpirationDateUtc"/>.</description></item>
    /// </list>
    /// 
    /// <para><c>Handling Requirements:</c></para>
    /// <list type="bullet">
    /// <item><description><c>No Logging:</c> Must never be logged, serialized, or stored in plain text.</description></item>
    /// <item><description><c>Memory Management:</c> Should be cleared from memory when no longer needed.</description></item>
    /// <item><description><c>Transmission Security:</c> Always transmitted over secure channels (TLS/SSL).</description></item>
    /// <item><description><c>Access Control:</c> Only accessible to authorized components of the application.</description></item>
    /// </list>
    /// 
    /// <para><c>Common Characteristics by Provider:</c></para>
    /// <list type="bullet">
    /// <item><description><c>HashiCorp Vault:</c> Configurable length and complexity, typically 20-64 characters</description></item>
    /// <item><description><c>AWS Secrets Manager:</c> Base64 encoded, 32-128 characters depending on configuration</description></item>
    /// <item><description><c>Azure Key Vault:</c> Policy-driven complexity, usually includes mixed case, numbers, and symbols</description></item>
    /// </list>
    /// 
    /// <para><c>Best Practices:</c></para>
    /// <list type="bullet">
    /// <item><description>Always validate credential expiration before using the password.</description></item>
    /// <item><description>Use secure string handling techniques when possible to minimize memory exposure.</description></item>
    /// <item><description>Implement proper error handling that doesn't expose the password in exception messages.</description></item>
    /// <item><description>Consider implementing password zeroization in disposal patterns.</description></item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// public async Task&lt;bool&gt; ValidateConnectionAsync(IPvNugsDynamicCredential credential)
    /// {
    ///     // Check credential validity first
    ///     if (DateTime.UtcNow >= credential.ExpirationDateUtc)
    ///     {
    ///         _logger.LogWarning("Attempted to use expired credential");
    ///         return false;
    ///     }
    ///     
    ///     try
    ///     {
    ///         var connectionString = $"Server=testserver;Database=testdb;Username={credential.Username};Password={credential.Password};ConnectTimeout=5;";
    ///         
    ///         await using var connection = new NpgsqlConnection(connectionString);
    ///         await connection.OpenAsync();
    ///         return connection.State == ConnectionState.Open;
    ///     }
    ///     catch (Exception ex)
    ///     {
    ///         // Never log the actual password
    ///         _logger.LogError(ex, "Connection validation failed for user {Username}", credential.Username);
    ///         return false;
    ///     }
    /// }
    /// </code>
    /// </example>
    string Password { get; }

    /// <summary>
    /// Gets the UTC date and time when this credential expires and becomes invalid.
    /// After this time, both the <see cref="Username"/> and <see cref="Password"/> will be rejected by the target system,
    /// and any authentication attempts using these credentials will fail.
    /// </summary>
    /// <value>
    /// A <see cref="DateTime"/> in UTC representing the exact moment when the credential becomes invalid.
    /// This value is set by the secret management system based on configured policies and cannot be modified after creation.
    /// </value>
    /// <remarks>
    /// <para><c>Expiration Behavior:</c></para>
    /// <list type="bullet">
    /// <item><description><c>Precise Timing:</c> The credential becomes invalid at the exact UTC time specified.</description></item>
    /// <item><description><c>Automatic Revocation:</c> The secret management system automatically revokes access at expiration.</description></item>
    /// <item><description><c>No Grace Period:</c> There is typically no grace period after expiration - credentials become immediately invalid.</description></item>
    /// <item><description><c>UTC Timezone:</c> Always expressed in UTC to avoid timezone-related issues in distributed systems.</description></item>
    /// </list>
    /// 
    /// <para><c>Typical Expiration Timeframes:</c></para>
    /// <list type="bullet">
    /// <item><description><c>Short-lived (15 minutes - 1 hour):</c> High-security environments, temporary access</description></item>
    /// <item><description><c>Medium-lived (1-8 hours):</c> Application workloads, batch processing</description></item>
    /// <item><description><c>Long-lived (24-72 hours):</c> Development environments, less critical systems</description></item>
    /// </list>
    /// 
    /// <para><c>Monitoring and Renewal:</c></para>
    /// <list type="bullet">
    /// <item><description>Applications should monitor expiration and request renewal well before expiry (recommended: 10-25% of total lifetime).</description></item>
    /// <item><description>Implement background renewal processes to avoid service interruptions.</description></item>
    /// <item><description>Log upcoming expirations for monitoring and alerting purposes.</description></item>
    /// <item><description>Consider implementing circuit breaker patterns for credential renewal failures.</description></item>
    /// </list>
    /// 
    /// <para><c>Time Synchronization:</c></para>
    /// <para>Accurate time synchronization is critical when working with expiration times. Applications should:</para>
    /// <list type="bullet">
    /// <item><description>Ensure system clocks are synchronized with NTP servers.</description></item>
    /// <item><description>Account for potential clock drift in distributed systems.</description></item>
    /// <item><description>Consider implementing time skew tolerance in expiration checks.</description></item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <para>Basic expiration checking:</para>
    /// <code>
    /// public bool IsCredentialValid(IPvNugsDynamicCredential credential)
    /// {
    ///     return credential != null &amp;&amp; DateTime.UtcNow &lt; credential.ExpirationDateUtc;
    /// }
    /// 
    /// public TimeSpan GetTimeUntilExpiry(IPvNugsDynamicCredential credential)
    /// {
    ///     if (credential == null)
    ///         return TimeSpan.Zero;
    ///         
    ///     var timeLeft = credential.ExpirationDateUtc - DateTime.UtcNow;
    ///     return timeLeft &gt; TimeSpan.Zero ? timeLeft : TimeSpan.Zero;
    /// }
    /// </code>
    /// 
    /// <para>Proactive renewal logic:</para>
    /// <code>
    /// public async Task&lt;IPvNugsDynamicCredential&gt; EnsureValidCredentialAsync(
    ///     IPvNugsDynamicCredential currentCredential, 
    ///     string secretName,
    ///     TimeSpan renewalBuffer = default)
    /// {
    ///     if (renewalBuffer == default)
    ///         renewalBuffer = TimeSpan.FromMinutes(5);
    ///         
    ///     var now = DateTime.UtcNow;
    ///     var renewalThreshold = currentCredential?.ExpirationDateUtc.Subtract(renewalBuffer);
    ///     
    ///     if (currentCredential == null || now &gt;= renewalThreshold)
    ///     {
    ///         _logger.LogInformation("Renewing credential that expires at {ExpirationTime}", 
    ///             currentCredential?.ExpirationDateUtc);
    ///             
    ///         var newCredential = await _secretManager.GetDynamicSecretAsync(secretName);
    ///         
    ///         if (newCredential != null)
    ///         {
    ///             _logger.LogInformation("New credential obtained, expires at {ExpirationTime}", 
    ///                 newCredential.ExpirationDateUtc);
    ///         }
    ///         
    ///         return newCredential ?? currentCredential;
    ///     }
    ///     
    ///     return currentCredential;
    /// }
    /// </code>
    /// 
    /// <para>Expiration monitoring and alerting:</para>
    /// <code>
    /// public class CredentialMonitor
    /// {
    ///     private readonly ILogger _logger;
    ///     
    ///     public void MonitorExpiration(IPvNugsDynamicCredential credential, string credentialName)
    ///     {
    ///         if (credential == null) return;
    ///         
    ///         var timeUntilExpiry = credential.ExpirationDateUtc - DateTime.UtcNow;
    ///         
    ///         if (timeUntilExpiry &lt;= TimeSpan.FromMinutes(5))
    ///         {
    ///             _logger.LogWarning("Credential {CredentialName} expires in {TimeLeft} minutes", 
    ///                 credentialName, timeUntilExpiry.TotalMinutes);
    ///         }
    ///         else if (timeUntilExpiry &lt;= TimeSpan.FromMinutes(15))
    ///         {
    ///             _logger.LogInformation("Credential {CredentialName} expires in {TimeLeft} hours", 
    ///                 credentialName, timeUntilExpiry.TotalHours);
    ///         }
    ///         
    ///         // Schedule renewal if needed
    ///         if (timeUntilExpiry &lt;= TimeSpan.FromMinutes(10))
    ///         {
    ///             _ = Task.Run(async () =&gt; await TriggerRenewalAsync(credentialName));
    ///         }
    ///     }
    /// }
    /// </code>
    /// </example>
    DateTime ExpirationDateUtc { get; }
}
using System.Text.Json.Serialization;
using pvNugsSecretManagerNc10Abstractions;

namespace pvNugsSecretManagerNc10ProviderHVault;

/// <summary>
/// Represents a dynamic database credential retrieved from HashiCorp Vault's Database secrets engine.
/// Implements <see cref="IPvNugsDynamicCredential"/> to provide temporary database credentials with automatic expiration.
/// </summary>
public class HVaultDatabaseSecret: IPvNugsDynamicCredential
{
    /// <summary>
    /// Gets or sets the dynamically generated database username.
    /// </summary>
    public string Username { get; set; } = null!;
    
    /// <summary>
    /// Gets or sets the dynamically generated database password.
    /// </summary>
    public string Password { get; set; } = null!;
    
    /// <summary>
    /// Gets the UTC expiration date for this credential.
    /// This property implements the <see cref="IPvNugsDynamicCredential.ExpirationDateUtc"/> interface.
    /// </summary>
    [JsonIgnore] public DateTime ExpirationDateUtc => ExpiresOnUtc;
    
    /// <summary>
    /// Gets or sets the time-to-live duration for this credential.
    /// Indicates how long the credential is valid from the time of creation.
    /// </summary>
    public TimeSpan TimeToLive { get; set; } = TimeSpan.FromHours(1);
    
    /// <summary>
    /// Gets or sets the UTC date and time when this credential expires.
    /// Calculated as creation time plus the time-to-live duration.
    /// </summary>
    public DateTime ExpiresOnUtc { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="HVaultDatabaseSecret"/> class.
    /// Default parameterless constructor for serialization purposes.
    /// </summary>
    public HVaultDatabaseSecret()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="HVaultDatabaseSecret"/> class with the specified credentials and TTL.
    /// </summary>
    /// <param name="username">The dynamically generated database username.</param>
    /// <param name="password">The dynamically generated database password.</param>
    /// <param name="timeToLive">The duration for which this credential is valid.</param>
    public HVaultDatabaseSecret(
        string username, string password, TimeSpan timeToLive)
    {
        Username = username;
        Password = password;
        TimeToLive = timeToLive;
        ExpiresOnUtc = DateTime.UtcNow.Add(timeToLive);
    }

    /// <summary>
    /// Returns a string representation of the credential with the password masked for security.
    /// </summary>
    /// <returns>A formatted string containing the username, masked password, TTL, and expiration time.</returns>
    public override string ToString()
    {
        return $"Username: {Username}, " +
               $"Password: '*****', " +
               $"TimeToLive: {TimeToLive}, " +
               $"ExpiresOnUtc: {ExpiresOnUtc}";
    }
}
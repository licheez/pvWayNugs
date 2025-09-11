using pvNugsSecretManagerNc9Abstractions;

namespace pvNugsSecretManagerNc9EnvVariables;

/// <summary>
/// Represents a dynamic credential that contains username, password, and expiration information.
/// This implementation uses environment variables as the underlying storage mechanism for secret management.
/// </summary>
/// <remarks>
/// The DynamicCredential class is designed to work with the pvNugs Secret Manager system,
/// providing a concrete implementation of <see cref="IPvNugsDynamicCredential"/> that retrieves
/// credential data from environment variables. This approach is particularly useful in containerized
/// environments or when credentials need to be injected at runtime without storing them in configuration files.
/// </remarks>
/// <param name="username">The username for the credential. Cannot be null or empty.</param>
/// <param name="password">The password for the credential. Cannot be null or empty.</param>
/// <param name="expirationDateUtc">The UTC date and time when this credential expires.</param>
/// <exception cref="ArgumentNullException">Thrown when username or password is null.</exception>
/// <exception cref="ArgumentException">Thrown when username or password is empty or whitespace.</exception>
/// <example>
/// <code>
/// var credential = new DynamicCredential(
///     "myUser", 
///     "mySecretPassword", 
///     DateTime.UtcNow.AddHours(24)
/// );
/// 
/// Console.WriteLine($"Username: {credential.Username}");
/// Console.WriteLine($"Expires: {credential.ExpirationDateUtc}");
/// </code>
/// </example>
public class DynamicCredential(
    string username, 
    string password, 
    DateTime expirationDateUtc)
    : IPvNugsDynamicCredential
{
    /// <summary>
    /// Gets the username associated with this credential.
    /// </summary>
    /// <value>
    /// A string containing the username. This value is set during construction and cannot be modified.
    /// </value>
    /// <remarks>
    /// This property provides read-only access to the username that was specified when the credential was created.
    /// The username is typically used for authentication purposes in conjunction with the password.
    /// </remarks>
    public string Username { get; } = username;

    /// <summary>
    /// Gets the password associated with this credential.
    /// </summary>
    /// <value>
    /// A string containing the password. This value is set during construction and cannot be modified.
    /// </value>
    /// <remarks>
    /// This property provides read-only access to the password that was specified when the credential was created.
    /// The password should be treated as sensitive information and handled securely throughout the application lifecycle.
    /// </remarks>
    public string Password { get; } = password;

    /// <summary>
    /// Gets the UTC date and time when this credential expires.
    /// </summary>
    /// <value>
    /// A <see cref="DateTime"/> value in UTC representing when the credential expires.
    /// </value>
    /// <remarks>
    /// This property indicates when the credential should no longer be considered valid.
    /// Applications should check this value before using the credential and implement appropriate
    /// renewal or refresh mechanisms when the credential approaches or passes its expiration time.
    /// All times are stored and should be interpreted as UTC to avoid timezone-related issues.
    /// </remarks>
    public DateTime ExpirationDateUtc { get; } = expirationDateUtc;
}
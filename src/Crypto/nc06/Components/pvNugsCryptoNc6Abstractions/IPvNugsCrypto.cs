namespace pvNugsCryptoNc6Abstractions;

/// <summary>
/// Abstraction for a cryptographic service used to encrypt and decrypt
/// strings and objects. Implementations are expected to provide both
/// synchronous disposal via <see cref="IDisposable"/> and asynchronous
/// disposal via <see cref="IAsyncDisposable"/> to allow releasing any
/// unmanaged or managed resources held by the implementation.
/// </summary>
/// <remarks>
/// Methods that produce "ephemeral" payloads include an optional
/// validity duration. When the validity period is exceeded, the
/// corresponding decrypt methods MAY return <c>null</c> (for ephemeral
/// variants) or throw a specific exception depending on the
/// implementation. Callers should consult the concrete implementation
/// documentation for exact behavior.
/// </remarks>
public interface IPvNugsCrypto: IAsyncDisposable, IDisposable
{
    /// <summary>
    /// Encrypts a plain text string and returns the encrypted payload as a
    /// Base64-encoded string.
    /// </summary>
    /// <param name="text">The plain text to encrypt. Must not be <c>null</c>.</param>
    /// <returns>A task that resolves to a Base64-encoded encrypted string.</returns>
    /// <exception cref="System.ArgumentNullException">Thrown if <paramref name="text"/> is <c>null</c>.</exception>
    Task<string> EncryptStringAsync(string text);

    /// <summary>
    /// Serializes and encrypts an object, returning the encrypted payload
    /// as a Base64-encoded string.
    /// </summary>
    /// <typeparam name="T">The reference type of the object to encrypt.</typeparam>
    /// <param name="data">The object to encrypt. Must not be <c>null</c>.</param>
    /// <returns>A task that resolves to a Base64-encoded encrypted string representing the object.</returns>
    /// <exception cref="System.ArgumentNullException">Thrown if <paramref name="data"/> is <c>null</c>.</exception>
    Task<string> EncryptObjectAsync<T>(T data) where T: class;
        
    /// <summary>
    /// Encrypts a plain text string into an ephemeral payload with an optional validity period.
    /// </summary>
    /// <param name="text">The plain text to encrypt. Must not be <c>null</c>.</param>
    /// <param name="validity">Optional time span after which the payload is considered expired. If <c>null</c>, the implementation's default lifetime is used.</param>
    /// <returns>A task that resolves to a Base64-encoded ephemeral encrypted string.</returns>
    /// <exception cref="System.ArgumentNullException">Thrown if <paramref name="text"/> is <c>null</c>.</exception>
    /// <remarks>
    /// When the validity period has elapsed, calls to the corresponding
    /// ephemeral decrypt methods may return <c>null</c> to indicate expiry.
    /// </remarks>
    Task<string> EncryptEphemeralStringAsync(
        string text, TimeSpan? validity = null);

    /// <summary>
    /// Serializes and encrypts an object into an ephemeral payload with an optional validity period.
    /// </summary>
    /// <typeparam name="T">The reference type of the object to encrypt.</typeparam>
    /// <param name="data">The object to encrypt. Must not be <c>null</c>.</param>
    /// <param name="validity">Optional time span after which the payload is considered expired. If <c>null</c>, the implementation's default lifetime is used.</param>
    /// <returns>A task that resolves to a Base64-encoded ephemeral encrypted string representing the object.</returns>
    /// <exception cref="System.ArgumentNullException">Thrown if <paramref name="data"/> is <c>null</c>.</exception>
    Task<string> EncryptEphemeralObjectAsync<T>(
        T data, TimeSpan? validity = null) 
        where T : class;
        
    /// <summary>
    /// Decrypts a Base64-encoded encrypted payload into a plain text string.
    /// </summary>
    /// <param name="base64Str">The Base64-encoded encrypted payload. Must not be <c>null</c> or empty.</param>
    /// <returns>A task that resolves to the decrypted plain text string.</returns>
    /// <exception cref="System.ArgumentNullException">Thrown if <paramref name="base64Str"/> is <c>null</c>.</exception>
    Task<string> DecryptStringAsync(string base64Str);
    
    /// <summary>
    /// Decrypts a Base64-encoded encrypted payload into an instance of <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The reference type to deserialize the decrypted payload into.</typeparam>
    /// <param name="base64Str">The Base64-encoded encrypted payload. Must not be <c>null</c> or empty.</param>
    /// <returns>
    /// A task that resolves to an instance of <typeparamref name="T"/> deserialized from the decrypted payload.
    /// </returns>
    /// <exception cref="System.ArgumentNullException">Thrown if <paramref name="base64Str"/> is <c>null</c>.</exception>
    /// <remarks>
    /// Implementations may throw a cryptographic exception if the payload is tampered with or cannot be decrypted.
    /// </remarks>
    Task<T> DecryptObjectAsync<T>(string base64Str) where T: class;
        
    /// <summary>
    /// Attempts to decrypt an ephemeral Base64-encoded payload into a plain text string.
    /// </summary>
    /// <param name="base64Str">The Base64-encoded ephemeral encrypted payload. Must not be <c>null</c> or empty.</param>
    /// <returns>
    /// A task that resolves to the decrypted string, or <c>null</c> if the payload has expired or is otherwise unavailable.
    /// </returns>
    /// <exception cref="System.ArgumentNullException">Thrown if <paramref name="base64Str"/> is <c>null</c>.</exception>
    Task<string?> DecryptEphemeralStringAsync(string base64Str);

    /// <summary>
    /// Attempts to decrypt an ephemeral Base64-encoded payload into an object of type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The reference type to deserialize the decrypted ephemeral payload into.</typeparam>
    /// <param name="base64Str">The Base64-encoded ephemeral encrypted payload. Must not be <c>null</c> or empty.</param>
    /// <returns>
    /// A task that resolves to an instance of <typeparamref name="T"/>, or <c>null</c> if the payload has expired or is otherwise unavailable.
    /// </returns>
    /// <exception cref="System.ArgumentNullException">Thrown if <paramref name="base64Str"/> is <c>null</c>.</exception>
    Task<T?> DecryptEphemeralObjectAsync<T>(string base64Str) where T : class;
}
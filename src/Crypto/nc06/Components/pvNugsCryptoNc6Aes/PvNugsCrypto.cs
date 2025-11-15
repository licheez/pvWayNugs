using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using pvNugsCryptoNc6Abstractions;
using pvNugsLoggerNc6Abstractions;

namespace pvNugsCryptoNc6Aes;

/// <summary>
/// AES-based implementation of the <c>IPvNugsCrypto</c> abstraction.
/// </summary>
/// <remarks>
/// This internal implementation uses a provided key and initialization
/// vector (IV) to perform AES encryption and decryption. The constructor
/// expects the key and IV as character strings: the implementation converts
/// them to bytes using UTF8/ASCII encoding (key uses UTF8, IV uses ASCII).
/// Ephemeral payloads are supported via wrapper types that include a
/// validity timestamp.
/// </remarks>
internal sealed class PvNugsCrypto: IPvNugsCrypto
{
    private readonly ILoggerService _logger;
    private readonly TimeSpan _defaultValidity;
    private readonly byte[] _iv;
    private readonly byte[] _key;
    private readonly Aes _aes;

    /// <summary>
    /// Create a new <see cref="PvNugsCrypto"/> instance using the provided logger and AES configuration.
    /// </summary>
    /// <param name="logger">Logger service used for recording exceptions and diagnostics.</param>
    /// <param name="options">Configuration options bound to <see cref="PvNugsCryptoAesConfig"/> containing the key, IV and default validity.</param>
    /// <exception cref="System.ArgumentNullException">Thrown when required configuration values (KeyString or InitializationVectorString) are <c>null</c>.</exception>
    /// <exception cref="PvWayCryptoException">Thrown when the provided key or IV have invalid lengths, or when an AES instance cannot be created.</exception>
    /// <remarks>
    /// The constructor reads the AES key and IV from <paramref name="options"/> and converts them into byte arrays.
    /// The expected lengths are: 32 characters for the AES key (AES-256) and 16 characters for the IV (AES block size).
    /// Store these values securely (for example in a secret store) and do not check them into source control.
    /// </remarks>
    public PvNugsCrypto(
        ILoggerService logger,
        IOptions<PvNugsCryptoAesConfig> options)
    {
        _logger = logger;
        var config = options.Value;
        var keyString = config.KeyString;
        var initializationVectorString = config.InitializationVectorString;
        if (keyString is null) 
            throw new ArgumentNullException(nameof(keyString));
        if (initializationVectorString is null) 
            throw new ArgumentNullException(nameof(initializationVectorString));
        
        if (keyString.Length != 32) 
            throw new PvWayCryptoException(
                "invalid key (should be 32  char long");
        if (initializationVectorString.Length != 16) 
            throw new PvWayCryptoException(
                "invalid initialization vector (should be 16 char long");
        _defaultValidity = config.DefaultValidity;
        _aes = Aes.Create();
        if (_aes == null) throw 
            new PvWayCryptoException(
                "aes should not be null");

        _key = Encoding.UTF8.GetBytes(keyString);
        _iv = Encoding.ASCII.GetBytes(initializationVectorString);
    }
        
    /// <summary>
    /// Encrypts the provided plain text and returns a Base64-encoded ciphertext.
    /// </summary>
    /// <param name="text">Plain text to encrypt. Must not be null.</param>
    /// <returns>A task that resolves to the Base64-encoded ciphertext.</returns>
    /// <remarks>
    /// The text is written into a <see cref="CryptoStream"/> which performs AES encryption.
    /// The resulting ciphertext bytes are Base64-encoded before being returned.
    /// On failure the exception is logged and wrapped in a <see cref="PvWayCryptoException"/>.
    /// </remarks>
    /// <exception cref="System.ArgumentNullException">Thrown when <paramref name="text"/> is <c>null</c>.</exception>
    /// <exception cref="PvWayCryptoException">Thrown when an internal cryptographic or IO error occurs; the original exception is wrapped.</exception>
    public async Task<string> EncryptStringAsync(string text)
    {
        if (text is null) throw new ArgumentNullException(nameof(text));
        try
        {
            var ct = _aes.CreateEncryptor(_key, _iv);
            await using var ms = new MemoryStream();
            await using var cs = new CryptoStream(ms, ct, CryptoStreamMode.Write);
            await using var sw = new StreamWriter(cs);
            await sw.WriteAsync(text);
            
            sw.Close();
            cs.Close();
            ms.Close();

            var buffer = ms.ToArray();
            var b64Str = Convert.ToBase64String(buffer);
            return b64Str;
        }
        catch (Exception e)
        {
            await _logger.LogAsync(e);
            throw new PvWayCryptoException(e);
        }
    }

    /// <summary>
    /// Serializes an object to JSON and encrypts the resulting JSON string.
    /// </summary>
    /// <typeparam name="T">The reference type of the object to encrypt.</typeparam>
    /// <param name="data">The object to serialize and encrypt.</param>
    /// <returns>A task that resolves to the Base64-encoded ciphertext of the serialized object.</returns>
    /// <remarks>
    /// Uses <see cref="JsonSerializer"/> for object serialization. Exceptions are logged and wrapped.
    /// </remarks>
    /// <exception cref="System.ArgumentNullException">Thrown when <paramref name="data"/> is <c>null</c>.</exception>
    /// <exception cref="PvWayCryptoException">Thrown when serialization or encryption fails; the original exception is wrapped.</exception>
    public async Task<string> EncryptObjectAsync<T>(T data) where T: class
    {
        if (data is null) throw new ArgumentNullException(nameof(data));
        try
        {
            var json = JsonSerializer.Serialize(data);
            return await EncryptStringAsync(json);
        }
        catch (Exception e)
        {
            await _logger.LogAsync(e);
            throw new PvWayCryptoException(e);
        }
    }

        
    /// <summary>
    /// Encrypts a plain text into an ephemeral payload. The payload will be valid for the provided <paramref name="validity"/> or the default validity passed to the constructor.
    /// </summary>
    /// <param name="text">Plain text to encrypt.</param>
    /// <param name="validity">Optional validity period. If null the instance default is used.</param>
    /// <returns>A task that resolves to the Base64-encoded ciphertext representing the ephemeral payload.</returns>
    /// <remarks>
    /// The returned ciphertext encodes an internal ephemeral wrapper that includes an expiry timestamp. Consumers should use the corresponding decrypt ephemeral methods which will return <c>null</c> if the payload has expired.
    /// </remarks>
    /// <exception cref="System.ArgumentNullException">Thrown when <paramref name="text"/> is <c>null</c>.</exception>
    /// <exception cref="PvWayCryptoException">Thrown when creating or encrypting the ephemeral payload fails.</exception>
    public async Task<string> EncryptEphemeralStringAsync(
        string text, TimeSpan? validity = null)
    {
        if (text is null) throw new ArgumentNullException(nameof(text));
        try
        {
            var ce = new CryptoEphemeral<string>(
                text, validity??_defaultValidity);
            return await EncryptObjectAsync(ce);
        }
        catch (Exception e)
        {
            await _logger.LogAsync(e);
            throw new PvWayCryptoException(e);
        }
    }

    /// <summary>
    /// Encrypts an object into an ephemeral payload.
    /// </summary>
    /// <typeparam name="T">Type of the object to encrypt.</typeparam>
    /// <param name="data">The object to encrypt.</param>
    /// <param name="validity">Optional validity period; if null the instance default is used.</param>
    /// <returns>A task that resolves to the Base64-encoded ciphertext representing the ephemeral payload.</returns>
    /// <remarks>
    /// See <see cref="EncryptEphemeralStringAsync(string,TimeSpan?)"/> for behavior details; exceptions are logged and wrapped.
    /// </remarks>
    /// <exception cref="System.ArgumentNullException">Thrown when <paramref name="data"/> is <c>null</c>.</exception>
    /// <exception cref="PvWayCryptoException">Thrown when creating or encrypting the ephemeral payload fails.</exception>
    public async Task<string> EncryptEphemeralObjectAsync<T>(
        T data, TimeSpan? validity = null) 
        where T : class
     {
         if (data is null) throw new ArgumentNullException(nameof(data));
         try
         {
             var ce = new CryptoEphemeral<T>(
                 data, validity??_defaultValidity);
             return await EncryptObjectAsync(ce);
         }
         catch (Exception e)
         {
             await _logger.LogAsync(e);
             throw new PvWayCryptoException(e); 
         }
     }


    /// <summary>
    /// Decrypts a Base64-encoded ciphertext to a plain text string.
    /// </summary>
    /// <param name="base64Str">Base64-encoded ciphertext to decrypt.</param>
    /// <returns>A task that resolves to the decrypted plain text string.</returns>
    /// <remarks>
    /// The input is expected to be a valid Base64 string containing AES-encrypted bytes. Exceptions are logged and wrapped.
    /// </remarks>
    /// <exception cref="System.ArgumentNullException">Thrown when <paramref name="base64Str"/> is <c>null</c>.</exception>
    /// <exception cref="System.FormatException">Thrown when <paramref name="base64Str"/> is not valid Base64.</exception>
    /// <exception cref="PvWayCryptoException">Thrown when decryption or I/O fails; the original exception is wrapped.</exception>
    public async Task<string> DecryptStringAsync(string base64Str)
    {
        if (base64Str is null) throw new ArgumentNullException(nameof(base64Str));

        try
        {
            var buffer = Convert.FromBase64String(base64Str);
            var dt = _aes.CreateDecryptor(_key, _iv);
            await using var ms = new MemoryStream(buffer);
            await using var cs = new CryptoStream(ms, dt, CryptoStreamMode.Read);
            using var sr = new StreamReader(cs);
            var text = await sr.ReadToEndAsync();
            return text;
        }
        catch (Exception e)
        {
            await _logger.LogAsync(e);
            throw new PvWayCryptoException(e);
        }
    }

    /// <summary>
    /// Decrypts a Base64-encoded ciphertext into an object of type <typeparamref name="T"/> by first converting to JSON and then deserializing.
    /// </summary>
    /// <typeparam name="T">The reference type to deserialize into.</typeparam>
    /// <param name="base64Str">Base64-encoded ciphertext containing serialized JSON.</param>
    /// <returns>A task that resolves to the deserialized object instance.</returns>
    /// <remarks>
    /// Deserialization uses <see cref="JsonSerializer"/>; exceptions are logged and wrapped.
    /// </remarks>
    /// <exception cref="System.ArgumentNullException">Thrown when <paramref name="base64Str"/> is <c>null</c>.</exception>
    /// <exception cref="PvWayCryptoException">Thrown when decryption or deserialization fails; the original exception is wrapped.</exception>
    public async Task<T> DecryptObjectAsync<T>(string base64Str) 
        where T:class
    {
        if (base64Str is null) throw new ArgumentNullException(nameof(base64Str));

        try
        {
            var json = await DecryptStringAsync(base64Str);
            return JsonSerializer.Deserialize<T>(json)!;
        }
        catch (Exception e)
        {
            await _logger.LogAsync(e);
            throw new PvWayCryptoException(e);
        }
    }

        
    /// <summary>
    /// Decrypts an ephemeral ciphertext representing a <c>string</c> value and returns the inner data if still valid; otherwise returns <c>null</c>.
    /// </summary>
    /// <param name="base64Str">Base64-encoded ciphertext representing an ephemeral payload.</param>
    /// <returns>A task that resolves to the inner string if the payload is still valid; otherwise <c>null</c>.</returns>
    /// <remarks>
    /// The ephemeral payload includes a validity timestamp; when expired the method returns <c>null</c>.
    /// Exceptions are logged and wrapped.
    /// </remarks>
    /// <exception cref="System.ArgumentNullException">Thrown when <paramref name="base64Str"/> is <c>null</c>.</exception>
    /// <exception cref="PvWayCryptoException">Thrown when decryption or payload parsing fails; the original exception is wrapped.</exception>
    public async Task<string?> DecryptEphemeralStringAsync(string base64Str)
    {
        if (base64Str is null) throw new ArgumentNullException(nameof(base64Str));

        try
        {
            var ce = await DecryptObjectAsync<CryptoEphemeral<string>>(base64Str);
            return ce.ValidUntil > DateTime.UtcNow 
                ? ce.Data : null;
        }
        catch (Exception e)
        {
            await _logger.LogAsync(e);
            throw new PvWayCryptoException(e);
        }
    }

    /// <summary>
    /// Decrypts an ephemeral ciphertext representing an object of type <typeparamref name="T"/> and returns the inner data if still valid; otherwise returns <c>null</c>.
    /// </summary>
    /// <typeparam name="T">The reference type contained in the ephemeral payload.</typeparam>
    /// <param name="base64Str">Base64-encoded ciphertext representing an ephemeral payload.</param>
    /// <returns>A task that resolves to the inner object if the payload is still valid; otherwise <c>null</c>.</returns>
    /// <remarks>
    /// Exceptions are logged and wrapped in <see cref="PvWayCryptoException"/>.
    /// </remarks>
    /// <exception cref="System.ArgumentNullException">Thrown when <paramref name="base64Str"/> is <c>null</c>.</exception>
    /// <exception cref="PvWayCryptoException">Thrown when decryption or payload parsing fails; the original exception is wrapped.</exception>
    public async Task<T?> DecryptEphemeralObjectAsync<T>(string base64Str) where T: class
    {
        if (base64Str is null) throw new ArgumentNullException(nameof(base64Str));

        try
        {
            var ce = await DecryptObjectAsync<CryptoEphemeral<T>>(base64Str);
            return ce.ValidUntil > DateTime.UtcNow ? ce.Data : null;
        }
        catch (Exception e)
        {
            await _logger.LogAsync(e);
            throw new PvWayCryptoException(e);
        }
    }

    /// <summary>
    /// Disposes the underlying AES resources.
    /// </summary>
    /// <remarks>
    /// This method synchronously disposes the internal <see cref="Aes"/> instance.
    /// </remarks>
    public void Dispose()
    {
        _aes.Dispose();
    }

    /// <summary>
    /// Asynchronously disposes the underlying AES resources.
    /// </summary>
    /// <returns>A value task that completes when disposal has finished.</returns>
    public async ValueTask DisposeAsync()
    {
        await Task.Run(Dispose);
    }
}
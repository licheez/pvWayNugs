using System.ComponentModel.DataAnnotations;

namespace pvNugsCryptoNc6Aes;

/// <summary>
/// Configuration settings for the AES-based implementation of the pvNugs
/// cryptography component.
/// </summary>
/// <remarks>
/// This configuration object holds the string representations of the AES
/// key and initialization vector (IV) used by the AES implementation.
/// The concrete implementation determines how these strings are decoded
/// into raw key/IV byte arrays (for example: UTF-8 bytes, Base64, or hex).
/// For AES-256 the key material must correspond to 32 bytes; the IV must
/// correspond to the AES block size of 16 bytes. Store these settings in a
/// secure secrets store (for example Azure Key Vault or environment
/// variables) and never check secrets into source control.
/// </remarks>
public sealed class PvNugsCryptoAesConfig
{
    /// <summary>
    /// Configuration section name used to bind this object from configuration
    /// providers (for example IConfiguration.GetSection).
    /// </summary>
    public const string Section = nameof(PvNugsCryptoAesConfig);
    
    /// <summary>
    /// The AES key represented as a string. The maximum string length is 32
    /// characters as constrained by <see cref="MaxLengthAttribute"/> on this
    /// property. The exact encoding (UTF-8, Base64, hex) and required length
    /// in bytes is implementation-specific; ensure the decoded value yields
    /// the correct key length for the chosen AES variant (e.g. 32 bytes for AES-256).
    /// </summary>
    /// <remarks>
    /// Security: Keep this value secret. Prefer retrieving this value from a
    /// secrets manager at runtime rather than embedding it in configuration
    /// files or source control.
    /// </remarks>
    [MaxLength(32)]
    public string KeyString { get; set; } = null!;
    
    /// <summary>
    /// The AES initialization vector (IV) represented as a string. The
    /// maximum string length is 16 characters as constrained by
    /// <see cref="MaxLengthAttribute"/> on this property. The concrete
    /// implementation determines the decoding; ensure the decoded value
    /// yields 16 bytes (AES block size).
    /// </summary>
    /// <remarks>
    /// Security: The IV does not need to be secret in most modes, but it
    /// must be unique and unpredictable where required. Do not reuse an IV
    /// improperly across different encryption operations.
    /// </remarks>
    [MaxLength(16)]
    public string InitializationVectorString { get; set; } = null!;
    
    /// <summary>
    /// Default time span used for ephemeral payload validity when no explicit
    /// validity is supplied to the ephemeral encryption methods.
    /// </summary>
    /// <remarks>
    /// This value is applied as a duration (for example: <c>DateTime.UtcNow + DefaultValidity</c>)
    /// when constructing internal ephemeral payloads. Keep this duration
    /// reasonably short for security-sensitive payloads; the default is
    /// one hour (<c>TimeSpan.FromHours(1)</c>).
    /// </remarks>
    public TimeSpan DefaultValidity { get; set; } = TimeSpan.FromHours(1);
    
}
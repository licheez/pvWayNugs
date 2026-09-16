using Microsoft.Extensions.Options;
using pvNugsLoggerNc10Abstractions;
using pvNugsSecretManagerNc10Abstractions;

namespace pvNugsMessagingNc10Kafka;

/// <summary>
/// Resolves and caches the security information required by the Kafka provider.
/// </summary>
/// <remarks>
/// Security credentials are retrieved from the configured
/// <see cref="IPvNugsSecretManager"/> using the parameters defined in
/// <see cref="PvNugsMessagingKafkaSecurityConfig"/>.
///
/// Once successfully resolved, the security information is cached for the
/// lifetime of this service instance.
/// </remarks>
internal sealed class PvNugsMessagingKafkaSecurityService(
    ILoggerService logger,
    IOptions<PvNugsMessagingKafkaSecurityConfig> options)
{
    private readonly PvNugsMessagingKafkaSecurityConfig _config = options.Value;

    private readonly SemaphoreSlim _securityLock = new(1, 1);

    private PvNugsMessagingKafkaSecurity? _cachedSecurity;

    private readonly IPvNugsSecretManager? _secretManager;
    
    /// <summary>
    /// Gets a value indicating whether SSL/TLS encryption is enabled
    /// for SASL-authenticated Kafka connections.
    /// </summary>
    public bool EnableSsl => _config.EnableSsl;
    
    /// <summary>
    /// Initializes a new instance of the
    /// <see cref="PvNugsMessagingKafkaSecurityService"/> class with a secret
    /// manager used to resolve Kafka security credentials.
    /// </summary>
    /// <param name="logger">
    /// The logger used by the service.
    /// </param>
    /// <param name="options">
    /// The Kafka security configuration.
    /// </param>
    /// <param name="secretManager">
    /// The secret manager used to retrieve the SASL credentials.
    /// </param>
    public PvNugsMessagingKafkaSecurityService(
        ILoggerService logger,
        IOptions<PvNugsMessagingKafkaSecurityConfig> options,
        IPvNugsSecretManager secretManager)
        : this(logger, options)
    {
        _secretManager = secretManager;
    }

    /// <summary>
    /// Retrieves the security information required to connect to Kafka.
    /// </summary>
    /// <param name="cancellationToken">
    /// A token that can be used to cancel the asynchronous operation.
    /// </param>
    /// <returns>
    /// A task representing the asynchronous operation. The task result contains
    /// the resolved Kafka security information.
    /// </returns>
    /// <remarks>
    /// Credentials are retrieved from the configured secret manager on the
    /// first successful call and are subsequently returned from the local cache.
    /// </remarks>
    /// <exception cref="InvalidOperationException">
    /// Thrown when no secret manager is available or when a required credential
    /// cannot be retrieved.
    /// </exception>
    public async Task<PvNugsMessagingKafkaSecurity> GetKafkaSecurityAsync(
        CancellationToken cancellationToken = default)
    {
        if (_cachedSecurity is not null)
        {
            return _cachedSecurity;
        }

        if (_secretManager is null)
        {
            throw new InvalidOperationException(
                "Secret manager is not initialized.");
        }

        await _securityLock.WaitAsync(cancellationToken);

        try
        {
            // Another caller may have populated the cache while this caller
            // was waiting for the lock.
            if (_cachedSecurity is not null)
            {
                return _cachedSecurity;
            }

            await logger.LogAsync(
                "Retrieving Kafka security credentials from secret manager.",
                SeverityEnu.Trace);

            var saslUsername = await _secretManager.GetStaticSecretAsync(
                _config.SaslUsernameParams,
                cancellationToken);

            var saslPassword = await _secretManager.GetStaticSecretAsync(
                _config.SaslPasswordParams,
                cancellationToken);

            if (string.IsNullOrWhiteSpace(saslUsername))
            {
                throw new InvalidOperationException(
                    "Kafka SASL username could not be retrieved from the secret manager.");
            }

            if (string.IsNullOrWhiteSpace(saslPassword))
            {
                throw new InvalidOperationException(
                    "Kafka SASL password could not be retrieved from the secret manager.");
            }

            _cachedSecurity = new PvNugsMessagingKafkaSecurity(
                saslUsername,
                saslPassword,
                _config.EnableSslCertificateVerification);

            return _cachedSecurity;
        }
        finally
        {
            _securityLock.Release();
        }
    }
}
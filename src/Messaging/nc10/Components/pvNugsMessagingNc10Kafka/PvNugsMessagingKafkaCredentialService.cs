using Microsoft.Extensions.Options;
using pvNugsLoggerNc10Abstractions;
using pvNugsSecretManagerNc10Abstractions;

namespace pvNugsMessagingNc10Kafka;

internal sealed class PvNugsMessagingKafkaCredentialService(
    ILoggerService logger,
    IOptions<PvNugsMessagingKafkaCredentialConfig> options)
{
    private readonly PvNugsMessagingKafkaCredentialConfig _config = options.Value;
    
    private PvNugsMessagingKafkaCredential? _cachedCredential = null;

    private readonly IPvNugsSecretManager? _secretManager = null;

    public PvNugsMessagingKafkaCredentialService(
        ILoggerService logger,
        IOptions<PvNugsMessagingKafkaCredentialConfig> options,
        IPvNugsSecretManager secretManager)
        : this(logger, options)
    {
        _secretManager = secretManager;
    }
    
    public async Task<PvNugsMessagingKafkaCredential> GetKafkaCredentialAsync(
        CancellationToken cancellationToken = default)
    {
        if (_cachedCredential is not null)
        {
            return _cachedCredential;
        }
        
        if (_secretManager is null)
        {
            throw new InvalidOperationException("Secret manager is not initialized.");
        }

        var saslUsername = await _secretManager.GetStaticSecretAsync(
            _config.SaslUserNameParams, cancellationToken);
        var saslPassword = await _secretManager.GetStaticSecretAsync(
            _config.SaslPasswordParams, cancellationToken);

        return new PvNugsMessagingKafkaCredential(
            saslUsername!, saslPassword!,
            _config.SecurityProtocol, _config.SaslMechanism, 
            _config.EnableSslCertificateVerification);
    }
}
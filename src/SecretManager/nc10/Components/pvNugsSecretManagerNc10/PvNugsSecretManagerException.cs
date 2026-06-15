using pvNugsLoggerNc10Abstractions;

namespace pvNugsSecretManagerNc10;

public class PvNugsSecretManagerException(Exception e) :
    Exception($"pvNugsSecretManager {e.GetDeepMessage()}", e);

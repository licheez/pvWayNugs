namespace pvNugsSecretManagerNc10ProviderHVault;

/// <summary>
/// Provides parameter names and factory methods for HashiCorp Vault secret provider operations.
/// </summary>
public static class PvNugsHVaultSecretProviderParameters
{
    /// <summary>
    /// The mount point path for the Vault secrets engine (e.g., "database/postgres/pg5432").
    /// </summary>
    public const string MountPoint = "mountPoint";
    
    /// <summary>
    /// The path to the secret within the Key-Value secrets engine.
    /// </summary>
    public const string Path = "path";
    
    /// <summary>
    /// The name of a specific secret within a Key-Value secret collection.
    /// </summary>
    public const string SecretName = "secretName";
    
    /// <summary>
    /// The name of the database role for dynamic credential generation.
    /// </summary>
    public const string Role = "role";
    
    /// <summary>
    /// Creates a parameter dictionary for retrieving static secrets from the Key-Value secrets engine.
    /// </summary>
    /// <param name="mountPoint">The mount point path for the Key-Value secrets engine.</param>
    /// <param name="path">The path to the secret within the Key-Value store.</param>
    /// <param name="secretName">Optional. The name of a specific secret to retrieve. If null, all secrets at the path are returned.</param>
    /// <returns>A read-only dictionary of parameters for the secret provider.</returns>
    /// <exception cref="ArgumentNullException">Thrown when mountPoint or path is null or empty.</exception>
    public static IReadOnlyDictionary<string, string> CreateStaticParameters(
        string mountPoint, string path, 
        string? secretName)
    {
        if (string.IsNullOrEmpty(mountPoint))
        {
            throw new ArgumentNullException(nameof(mountPoint));
        }
        if (string.IsNullOrEmpty(path))
        {
            throw new ArgumentNullException(nameof(path));
        }
        return new Dictionary<string, string>
        {
            { MountPoint, mountPoint },
            { Path, path },
            { SecretName, secretName ?? string.Empty },
        };
    }

    /// <summary>
    /// Creates a parameter dictionary for retrieving dynamic credentials from the Database secrets engine.
    /// </summary>
    /// <param name="mountPoint">The mount point path for the Database secrets engine (e.g., "database/postgres/pg5432").</param>
    /// <param name="roleName">The name of the database role for credential generation (e.g., "owner").</param>
    /// <returns>A read-only dictionary of parameters for the secret provider.</returns>
    /// <exception cref="ArgumentNullException">Thrown when mountPoint or roleName is null or empty.</exception>
    public static IReadOnlyDictionary<string, string> CreateDynamicParameters(
        string mountPoint, string roleName)
    {
        if (string.IsNullOrEmpty(mountPoint))
        {
            throw new ArgumentNullException(nameof(mountPoint));
        }
        if (string.IsNullOrEmpty(roleName))
        {
            throw new ArgumentNullException(nameof(roleName));
        }
        return new Dictionary<string, string>
        {
            { MountPoint, mountPoint },
            { Role, roleName }
        };
    }
}
namespace pvNugsSecretManagerNc9Abstractions;

public interface IPvNugsDynamicCredential
{
    string Username { get; }
    string Password { get; }
    DateTime ExpirationDateUtc { get; }
}
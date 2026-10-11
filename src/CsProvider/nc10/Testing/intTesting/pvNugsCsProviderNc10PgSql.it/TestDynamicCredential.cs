using pvNugsSecretManagerNc10Abstractions;

namespace pvNugsCsProviderNc10PgSql.it;

public class TestDynamicCredential: IPvNugsDynamicCredential
{
    public string Username => "username";
    public string Password => "password";
    public DateTime ExpirationDateUtc => DateTime.UtcNow.AddHours(1);
}
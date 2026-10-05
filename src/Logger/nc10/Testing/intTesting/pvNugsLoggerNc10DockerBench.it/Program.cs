using Npgsql;

Console.WriteLine("Testing Npgsql connection to PostgreSQL database...");

var cn = new NpgsqlConnection(
    "Host=localhost;" +
    "Port=5432;" +
    "Username=postgres;" +
    "Password=dev-only-password;" +
    "Database=postgres"
);

await cn.OpenAsync();
await cn.CloseAsync();
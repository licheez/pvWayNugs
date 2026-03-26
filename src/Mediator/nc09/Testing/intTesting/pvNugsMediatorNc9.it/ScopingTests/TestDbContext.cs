namespace pvNugsMediatorNc9.it.ScopingTests;

/// <summary>
/// Simulates a database context with instance tracking to verify scoping behavior.
/// </summary>
public class TestDbContext : IDisposable
{
    public Guid InstanceId { get; } = Guid.NewGuid();
    public List<string> Data { get; } = [];
    public bool IsDisposed { get; private set; }
    
    public void Dispose()
    {
        IsDisposed = true;
        Console.WriteLine($"  [DbContext {InstanceId:N}] DISPOSED");
    }
}


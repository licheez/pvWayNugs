using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using pvNugsLoggerNc10Abstractions;

namespace pvNugsLoggerNc10Seri;

/// <summary>
/// Creates <see cref="ILogger"/> instances backed by the Serilog console writer.
/// </summary>
/// <remarks>
/// <para>
/// This factory is used by the Microsoft logging infrastructure to produce category-specific logger instances.
/// It reads the configured minimum log level from <see cref="PvNugsLoggerConfig"/> and reuses the configured
/// <see cref="IConsoleLogWriter"/> instance for all created loggers.
/// </para>
/// <para>
/// The implementation keeps a list of registered providers for disposal and supports the standard
/// <see cref="ILoggerFactory"/> contract used by dependency injection and logging extensions.
/// </para>
/// </remarks>
internal sealed class SeriLogLoggerFactory(
    IOptions<PvNugsLoggerConfig> options,
    IConsoleLogWriter logWriter)
    : ILoggerFactory
{
    private readonly Lock _syncRoot = new();
    private readonly List<ILoggerProvider> _providers = [];
    private readonly IOptions<PvNugsLoggerConfig> _options =
        options ?? throw new ArgumentNullException(nameof(options));
    private readonly IConsoleLogWriter _logWriter =
        logWriter ?? throw new ArgumentNullException(nameof(logWriter));

    /// <summary>
    /// Releases all registered logging providers.
    /// </summary>
    public void Dispose()
    {
        lock (_syncRoot)
        {
            foreach (var provider in _providers)
            {
                provider.Dispose();
            }

            _providers.Clear();
        }
    }

    /// <summary>
    /// Creates a new logger instance for the specified category.
    /// </summary>
    /// <param name="categoryName">
    /// The logging category name supplied by the Microsoft logging system.
    /// </param>
    /// <returns>
    /// A logger instance configured with the current minimum severity and the console writer.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="categoryName"/> is null, empty, or whitespace.
    /// </exception>
    public ILogger CreateLogger(string categoryName)
    {
        ArgumentException.ThrowIfNullOrEmpty(categoryName);

        return new SerilogConsoleService(
            _options.Value.MinLevel, _logWriter);
    }

    /// <summary>
    /// Registers an additional logging provider to be disposed with the factory.
    /// </summary>
    /// <param name="provider">
    /// The logging provider to add to the factory-managed provider list.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="provider"/> is <see langword="null"/>.
    /// </exception>
    public void AddProvider(ILoggerProvider provider)
    {
        ArgumentNullException.ThrowIfNull(provider);

        lock (_syncRoot)
        {
            if (!_providers.Contains(provider))
            {
                _providers.Add(provider);
            }
        }
    }
}
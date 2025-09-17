using pvNugsLoggerNc9Abstractions;

namespace pvNugsLoggerNc9MsSql;

/// <summary>
/// Defines the contract for a Microsoft SQL Server-specific log writer implementation.
/// </summary>
/// <remarks>
/// <para>
/// This interface extends <see cref="ISqlLogWriter"/> to provide SQL Server-specific logging capabilities.
/// It serves as a marker interface that allows for dependency injection scenarios where SQL Server-specific
/// implementations need to be distinguished from other SQL database implementations.
/// </para>
/// <para>
/// Implementations of this interface should provide robust, secure, and performant logging to SQL Server databases,
/// including features such as:
/// </para>
/// <list type="bullet">
/// <item>Parameterized queries to prevent SQL injection attacks</item>
/// <item>Automatic table creation and schema validation</item>
/// <item>Thread-safe lazy initialization</item>
/// <item>Comprehensive error handling and logging</item>
/// <item>Log purging capabilities based on retention policies</item>
/// </list>
/// </remarks>
/// <example>
/// <code>
/// // Registration in DI container
/// services.AddSingleton&lt;IMsSqlLogWriter, MsSqlLogWriter&gt;();
/// 
/// // Usage in a service
/// public class LoggingService
/// {
///     private readonly IMsSqlLogWriter _logWriter;
///     
///     public LoggingService(IMsSqlLogWriter logWriter)
///     {
///         _logWriter = logWriter;
///     }
///     
///     public async Task LogMessageAsync(string message)
///     {
///         await _logWriter.WriteLogAsync(
///             userId: "user123",
///             companyId: "company456", 
///             topic: "Application",
///             severity: SeverityEnu.Info,
///             machineName: Environment.MachineName,
///             memberName: nameof(LogMessageAsync),
///             filePath: __FILE__,
///             lineNumber: __LINE__,
///             message: message,
///             dateUtc: DateTime.UtcNow);
///     }
/// }
/// </code>
/// </example>
/// <seealso cref="ISqlLogWriter"/>
/// <seealso cref="MsSqlLogWriter"/>
public interface IMsSqlLogWriter: ISqlLogWriter;
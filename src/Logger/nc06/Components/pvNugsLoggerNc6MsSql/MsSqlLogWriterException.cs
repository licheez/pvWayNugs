using Microsoft.Data.SqlClient;

namespace pvNugsLoggerNc6MsSql;

/// <summary>
/// Represents errors that occur during Microsoft SQL Server log writing operations.
/// </summary>
/// <remarks>
/// <para>
/// This exception is thrown by <see cref="MsSqlLogWriter"/> when database operations fail,
/// such as connection issues, table creation failures, schema validation errors, or
/// SQL execution problems during log writing or purging operations.
/// </para>
/// <para>
/// The exception provides a consistent way to handle SQL Server-specific logging errors
/// and includes the original exception details when wrapping lower-level database exceptions.
/// All exception messages are prefixed with "MsSqlLogWriterException:" for easy identification
/// in logs and error handling code.
/// </para>
/// <para>
/// Common scenarios that may trigger this exception include:
/// </para>
/// <list type="bullet">
/// <item>Database connection failures</item>
/// <item>Invalid connection strings</item>
/// <item>Table creation or schema validation failures</item>
/// <item>SQL command execution errors</item>
/// <item>Database permission issues</item>
/// <item>Transaction rollback scenarios during log purging</item>
/// </list>
/// </remarks>
/// <example>
/// <code>
/// try
/// {
///     await logWriter.WriteLogAsync(userId, companyId, topic, severity, 
///                                   machineName, memberName, filePath, 
///                                   lineNumber, message, dateUtc);
/// }
/// catch (MsSqlLogWriterException ex)
/// {
///     // Handle SQL Server-specific logging errors
///     Console.WriteLine($"Logging failed: {ex.Message}");
///     
///     // Access the original database exception if available
///     if (ex.InnerException != null)
///     {
///         Console.WriteLine($"Underlying error: {ex.InnerException.Message}");
///     }
///     
///     // Implement fallback logging or error recovery
/// }
/// catch (Exception ex)
/// {
///     // Handle other unexpected errors
///     Console.WriteLine($"Unexpected error: {ex.Message}");
/// }
/// </code>
/// </example>
public class MsSqlLogWriterException: Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MsSqlLogWriterException"/> class with a specified error message.
    /// </summary>
    /// <param name="message">
    /// The message that describes the error. The message will be automatically prefixed with
    /// "MsSqlLogWriterException:" to provide consistent error identification.
    /// </param>
    /// <remarks>
    /// <para>
    /// Use this constructor when you need to throw a new exception with a descriptive message
    /// about what went wrong during the SQL Server logging operation.
    /// </para>
    /// <para>
    /// The resulting exception message will be in the format: "MsSqlLogWriterException: {message}"
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// if (string.IsNullOrEmpty(connectionString))
    /// {
    ///     throw new MsSqlLogWriterException("Connection string is required for SQL Server logging");
    /// }
    /// </code>
    /// </example>
    public MsSqlLogWriterException(string message):
        base($"MsSqlLogWriterException: {message}")
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MsSqlLogWriterException"/> class with a message 
    /// derived from an inner exception.
    /// </summary>
    /// <param name="e">
    /// The exception that is the cause of the current exception. This is typically a database-related
    /// exception such as <see cref="SqlException"/> or <see cref="System.InvalidOperationException"/>.
    /// Cannot be null.
    /// </param>
    /// <remarks>
    /// <para>
    /// Use this constructor when wrapping lower-level exceptions (such as SQL Server database exceptions)
    /// to provide a consistent exception type while preserving the original error details.
    /// </para>
    /// <para>
    /// The resulting exception message will be in the format: "MsSqlLogWriterException: {innerException.Message}"
    /// and the original exception will be available through the <see cref="Exception.InnerException"/> property.
    /// </para>
    /// <para>
    /// This constructor is commonly used in catch blocks within <see cref="MsSqlLogWriter"/> methods
    /// to wrap database exceptions before re-throwing them to calling code.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// try
    /// {
    ///     await cmd.ExecuteNonQueryAsync();
    /// }
    /// catch (SqlException sqlEx)
    /// {
    ///     // Wrap the SQL Server exception
    ///     throw new MsSqlLogWriterException(sqlEx);
    /// }
    /// catch (InvalidOperationException invalidEx)
    /// {
    ///     // Wrap other database-related exceptions
    ///     throw new MsSqlLogWriterException(invalidEx);
    /// }
    /// </code>
    /// </example>
    public MsSqlLogWriterException(Exception e):
        base($"MsSqlLogWriterException: {e.Message}", e)
    {
    }
}
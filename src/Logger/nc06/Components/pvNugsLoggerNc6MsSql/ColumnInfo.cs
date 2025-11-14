using System.Data;

namespace pvNugsLoggerNc6MsSql;

/// <summary>
/// Represents metadata information about a database table column retrieved from SQL Server's information schema.
/// </summary>
/// <remarks>
/// <para>
/// This internal class is used by <see cref="MsSqlLogWriter"/> during table schema validation to ensure
/// that the logging table structure matches expected requirements. It encapsulates column metadata
/// retrieved from the <c>INFORMATION_SCHEMA.COLUMNS</c> system view.
/// </para>
/// <para>
/// The class is designed to work specifically with the column information returned by queries against
/// SQL Server's information schema views and expects specific column names in the result set.
/// </para>
/// </remarks>
internal class ColumnInfo
{
    /// <summary>
    /// Gets the name of the database column.
    /// </summary>
    /// <value>
    /// The column name as stored in the database schema. This value is retrieved from the
    /// <c>column_name</c> field in the <c>INFORMATION_SCHEMA.COLUMNS</c> view.
    /// </value>
    public string ColumnName { get; }

    /// <summary>
    /// Gets the data type of the database column.
    /// </summary>
    /// <value>
    /// The SQL Server data type name (e.g., "varchar", "nvarchar", "int", "datetime").
    /// This value is retrieved from the <c>data_type</c> field in the <c>INFORMATION_SCHEMA.COLUMNS</c> view.
    /// </value>
    public string Type { get; }

    /// <summary>
    /// Gets the maximum character length for character-based columns, or null for non-character columns.
    /// </summary>
    /// <value>
    /// The maximum number of characters that can be stored in character-based columns (varchar, nvarchar, char, nchar).
    /// Returns null for numeric, date, or other non-character data types.
    /// For columns with unlimited length (e.g., varchar(MAX)), this may return -1.
    /// This value is retrieved from the <c>character_maximum_length</c> field in the <c>INFORMATION_SCHEMA.COLUMNS</c> view.
    /// </value>
    public int? Length { get; }

    /// <summary>
    /// Gets a value indicating whether the database column allows NULL values.
    /// </summary>
    /// <value>
    /// <c>true</c> if the column allows NULL values; otherwise, <c>false</c>.
    /// This value is determined by checking if the <c>is_nullable</c> field in the 
    /// <c>INFORMATION_SCHEMA.COLUMNS</c> view contains "YES".
    /// </value>
    public bool IsNullable { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ColumnInfo"/> class from a database query result.
    /// </summary>
    /// <param name="dr">
    /// The data record containing column metadata from an <c>INFORMATION_SCHEMA.COLUMNS</c> query.
    /// Must contain the following fields: <c>column_name</c>, <c>data_type</c>, 
    /// <c>character_maximum_length</c>, and <c>is_nullable</c>.
    /// </param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="dr"/> is null.</exception>
    /// <exception cref="IndexOutOfRangeException">
    /// Thrown when the expected column names are not found in the data record.
    /// </exception>
    /// <exception cref="InvalidCastException">
    /// Thrown when the data types of the fields in the data record don't match the expected types.
    /// </exception>
    /// <remarks>
    /// <para>
    /// This constructor extracts column metadata from a database query result that should be obtained
    /// by querying the <c>INFORMATION_SCHEMA.COLUMNS</c> system view. The expected query structure is:
    /// </para>
    /// <code>
    /// SELECT column_name, data_type, character_maximum_length, is_nullable
    /// FROM INFORMATION_SCHEMA.COLUMNS
    /// WHERE table_schema = @schemaName AND table_name = @tableName
    /// </code>
    /// <para>
    /// The constructor handles NULL values appropriately, particularly for the <c>character_maximum_length</c>
    /// field which is NULL for non-character data types.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Typical usage within MsSqlLogWriter.CheckTable method
    /// using var reader = await cmd.ExecuteReaderAsync();
    /// var columns = new Dictionary&lt;string, ColumnInfo&gt;();
    /// 
    /// while (await reader.ReadAsync())
    /// {
    ///     var columnInfo = new ColumnInfo(reader);
    ///     columns.Add(columnInfo.ColumnName, columnInfo);
    /// }
    /// </code>
    /// </example>
    public ColumnInfo(IDataRecord dr)
    {
        var iName = dr.GetOrdinal("column_name");
        ColumnName = dr.GetString(iName);

        var iDataType = dr.GetOrdinal("data_type");
        Type = dr.GetString(iDataType);

        var iLength = dr.GetOrdinal("character_maximum_length");

        Length = dr.IsDBNull(iLength)
            ? null : dr.GetInt32(iLength);

        var iIsNullable = dr.GetOrdinal("is_nullable");
        IsNullable = dr.GetString(iIsNullable) == "YES";
    }
}
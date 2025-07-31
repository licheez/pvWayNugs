using System.Net;
using pvNugsEnumConvNc9;
using pvNugsLoggerNc9Abstractions;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable MemberCanBeProtected.Global

namespace pvNugsLoggerNc9ResWrapper;

/// <summary>
/// Represents an HTTP response wrapper for downstream operations.
/// Provides status information, notifications, and mutation tracking.
/// </summary>
public class DsoHttpResult 
{
    /// <summary>
    /// Gets the HTTP status code based on the severity status.
    /// Returns InternalServerError for Fatal or Error severity, OK otherwise.
    /// </summary>
    public HttpStatusCode HttpStatusCode =>
        Status == SeverityEnu.Fatal
        || Status == SeverityEnu.Error
            ? HttpStatusCode.InternalServerError
            : HttpStatusCode.OK;

    /// <summary>
    /// Gets the status code string representation.
    /// </summary>
    public string StatusCode { get; }

    /// <summary>
    /// Gets the severity status converted from the status code.
    /// </summary>
    internal SeverityEnu Status => EnumConvert.GetValue<SeverityEnu>(StatusCode);   

    /// <summary>
    /// Gets the code representing the type of mutation performed.
    /// </summary>
    public string MutationCode { get; }

    /// <summary>
    /// Gets the collection of notifications associated with the result.
    /// </summary>
    public ICollection<DsoHttpResultNotification> Notifications { get; }

    /// <summary>
    /// Gets a value indicating whether more results are available.
    /// </summary>
    public bool HasMoreResults { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="DsoHttpResult"/> class with OK status.
    /// </summary>
    public DsoHttpResult()
    {
        StatusCode = SeverityEnu.Ok.GetCode();
        Notifications = new List<DsoHttpResultNotification>();
        MutationCode = DsoHttpResultMutationEnu.None.GetCode();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DsoHttpResult"/> class with specified severity.
    /// </summary>
    /// <param name="severity">The severity level for the result.</param>
    public DsoHttpResult(SeverityEnu severity)
    {
        StatusCode = severity.GetCode();
        Notifications = new List<DsoHttpResultNotification>();
        MutationCode = DsoHttpResultMutationEnu.None.GetCode();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DsoHttpResult"/> class with specified mutation type.
    /// </summary>
    /// <param name="mutation">The type of mutation performed.</param>
    public DsoHttpResult(DsoHttpResultMutationEnu mutation) :
        this(SeverityEnu.Ok, false, mutation)
    {
    }

    /// <summary>
    /// Protected constructor for initializing the result with all parameters.
    /// </summary>
    /// <param name="severity">The severity level for the result.</param>
    /// <param name="hasMoreResults">Indicates if more results are available.</param>
    /// <param name="mutation">The type of mutation performed.</param>
    protected DsoHttpResult(
        SeverityEnu severity,
        bool hasMoreResults,
        DsoHttpResultMutationEnu mutation = DsoHttpResultMutationEnu.None) :
        this()
    {
        StatusCode = severity.GetCode();
        MutationCode = mutation.GetCode();
        HasMoreResults = hasMoreResults;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DsoHttpResult"/> class from a method result.
    /// </summary>
    /// <param name="res">The method result to convert.</param>
    public DsoHttpResult(IMethodResult res) :
        this(res.Severity, false)
    {
        Notifications = res.Notifications.Select(
                x => new DsoHttpResultNotification(x.Severity, x.Message))
            .ToList();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DsoHttpResult"/> class from an exception.
    /// </summary>
    /// <param name="e">The exception to convert.</param>
    public DsoHttpResult(Exception e) :
        this(new MethodResult(e))
    {
    }
}

/// <summary>
/// Represents a generic HTTP response wrapper for downstream operations that includes data of type T.
/// </summary>
/// <typeparam name="T">The type of data contained in the result.</typeparam>
public class DsoHttpResult<T> : DsoHttpResult
{
    /// <summary>
    /// Gets the data payload of the result.
    /// </summary>
    public T Data { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="DsoHttpResult{T}"/> class with data.
    /// </summary>
    /// <param name="data">The data to include in the result.</param>
    public DsoHttpResult(T data)
    {
        Data = data;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DsoHttpResult{T}"/> class with data and pagination info.
    /// </summary>
    /// <param name="data">The data to include in the result.</param>
    /// <param name="hasMoreResults">Indicates if more results are available.</param>
    public DsoHttpResult(T data, bool hasMoreResults) :
        base(SeverityEnu.Ok, hasMoreResults)
    {
        Data = data;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DsoHttpResult{T}"/> class with data and mutation info.
    /// </summary>
    /// <param name="data">The data to include in the result.</param>
    /// <param name="mutation">The type of mutation performed.</param>
    public DsoHttpResult(T data, DsoHttpResultMutationEnu mutation) :
        base(SeverityEnu.Ok, false, mutation)
    {
        Data = data;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DsoHttpResult{T}"/> class from a method result.
    /// </summary>
    /// <param name="res">The method result to convert.</param>
    public DsoHttpResult(IMethodResult res) :
        base(res)
    {
        Data = default!;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DsoHttpResult{T}"/> class from an exception.
    /// </summary>
    /// <param name="e">The exception to convert.</param>
    public DsoHttpResult(Exception e) :
        base(e)
    {
        Data = default!;
    }
}
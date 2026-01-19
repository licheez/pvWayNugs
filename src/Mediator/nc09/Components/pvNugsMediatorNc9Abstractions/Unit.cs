using pvNugsMediatorNc9Abstractions.Mediator;

namespace pvNugsMediatorNc9Abstractions;

/// <summary>
/// Represents a void-like return type for mediator requests that don't need to return a meaningful value.
/// </summary>
/// <remarks>
/// <para>
/// The <see cref="Unit"/> type is a functional programming concept that represents "no value" or "void"
/// in a type-safe way. It allows methods to return a value while semantically indicating that
/// no meaningful data is being returned.
/// </para>
/// <para>
/// Use <see cref="Unit"/> as the response type for <see cref="IRequest{TResponse}"/>
/// when the request performs an action but doesn't need to return data. This is similar to
/// using <c>void</c> in synchronous methods, but works with generic type constraints.
/// </para>
/// <para>
/// All instances of <see cref="Unit"/> are considered equal, as they represent the absence of value.
/// Use the static <see cref="Value"/> property to obtain an instance.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Request that returns Unit (no meaningful value)
/// public class DeleteUserRequest : IRequest&lt;Unit&gt;
/// {
///     public int UserId { get; init; }
/// }
/// 
/// public class DeleteUserHandler : IRequestHandler&lt;DeleteUserRequest, Unit&gt;
/// {
///     public async Task&lt;Unit&gt; Handle(
///         DeleteUserRequest request, 
///         CancellationToken cancellationToken)
///     {
///         await _userRepository.DeleteAsync(request.UserId, cancellationToken);
///         return Unit.Value; // Return Unit to indicate completion
///     }
/// }
/// 
/// // Usage:
/// await _mediator.Send(new DeleteUserRequest { UserId = 123 });
/// </code>
/// </example>
public readonly struct Unit : IEquatable<Unit>
{
    /// <summary>
    /// Gets the singleton instance of <see cref="Unit"/>.
    /// </summary>
    /// <remarks>
    /// Since all <see cref="Unit"/> instances are equal, this static property provides
    /// a convenient way to return a <see cref="Unit"/> value without creating new instances.
    /// </remarks>
    public static readonly Unit Value = new ();

    /// <summary>
    /// Determines whether the specified object is a <see cref="Unit"/> instance.
    /// </summary>
    /// <param name="obj">The object to compare with the current instance.</param>
    /// <returns><c>true</c> if the specified object is a <see cref="Unit"/> instance; otherwise, <c>false</c>.</returns>
    public override bool Equals(object? obj) => obj is Unit;
    
    /// <summary>
    /// Determines whether the specified <see cref="Unit"/> instance is equal to the current instance.
    /// </summary>
    /// <param name="other">The <see cref="Unit"/> instance to compare with the current instance.</param>
    /// <returns>Always returns <c>true</c> because all <see cref="Unit"/> instances are equal.</returns>
    public bool Equals(Unit other) => true;
    
    /// <summary>
    /// Returns the hash code for this instance.
    /// </summary>
    /// <returns>Always returns 0 because all <see cref="Unit"/> instances are equal.</returns>
    public override int GetHashCode() => 0;
    
    /// <summary>
    /// Returns a string representation of this instance.
    /// </summary>
    /// <returns>A string representation of the unit value, represented as "()".</returns>
    public override string ToString() => "()";
    
    /// <summary>
    /// Determines whether two <see cref="Unit"/> instances are equal.
    /// </summary>
    /// <param name="left">The first <see cref="Unit"/> instance to compare.</param>
    /// <param name="right">The second <see cref="Unit"/> instance to compare.</param>
    /// <returns>Always returns <c>true</c> because all <see cref="Unit"/> instances are equal.</returns>
    public static bool operator ==(Unit left, Unit right) => true;
    
    /// <summary>
    /// Determines whether two <see cref="Unit"/> instances are not equal.
    /// </summary>
    /// <param name="left">The first <see cref="Unit"/> instance to compare.</param>
    /// <param name="right">The second <see cref="Unit"/> instance to compare.</param>
    /// <returns>Always returns <c>false</c> because all <see cref="Unit"/> instances are equal.</returns>
    public static bool operator !=(Unit left, Unit right) => false;
}


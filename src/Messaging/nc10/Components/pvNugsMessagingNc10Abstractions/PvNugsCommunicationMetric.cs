namespace pvNugsMessagingNc10Abstractions;

/// <summary>
/// Represents an observable metric provided by the underlying
/// communication infrastructure.
/// </summary>
/// <typeparam name="T">
/// The type of the value returned by the metric.
/// </typeparam>
/// <remarks>
/// A metric may not always provide a value. The <see cref="Status"/>
/// property indicates whether the value is available, not applicable
/// in the current context, not supported by the underlying communication
/// technology, or temporarily unavailable.
/// </remarks>
// ReSharper disable once ClassNeverInstantiated.Global
public class PvNugsCommunicationMetric<T>
{
    /// <summary>
    /// Gets the current availability status of the metric.
    /// </summary>
    public PvNugsCommunicationMetricStatusEnu Status { get; init; }

    /// <summary>
    /// Gets the current value of the metric when available.
    /// </summary>
    /// <remarks>
    /// This property should only be considered meaningful when
    /// <see cref="Status"/> is
    /// <see cref="PvNugsCommunicationMetricStatusEnu.Available"/>.
    /// </remarks>
    public T? Value { get; init; }

    /// <summary>
    /// Gets an optional human-readable explanation providing additional
    /// information about the metric status.
    /// </summary>
    /// <remarks>
    /// This property can be used, for example, to explain why a metric is
    /// not applicable, not supported, or temporarily unavailable.
    /// </remarks>
    public string? Reason { get; init; }
}
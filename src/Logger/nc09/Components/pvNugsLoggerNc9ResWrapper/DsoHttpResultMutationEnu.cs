namespace pvNugsLoggerNc9ResWrapper;

/// <summary>
/// Defines the types of mutations that can be performed in a downstream HTTP operation.
/// Each enum value is associated with a single-character description code.
/// </summary>
public enum DsoHttpResultMutationEnu
{
    /// <summary>
    /// Represents no mutation operation.
    /// Description code: "N"
    /// </summary>
    [System.ComponentModel.Description("N")]
    None,

    /// <summary>
    /// Represents a create operation.
    /// Description code: "C"
    /// </summary>
    [System.ComponentModel.Description("C")]
    Create,

    /// <summary>
    /// Represents an update operation.
    /// Description code: "U"
    /// </summary>
    [System.ComponentModel.Description("U")]
    Update,

    /// <summary>
    /// Represents a delete operation.
    /// Description code: "D"
    /// </summary>
    [System.ComponentModel.Description("D")]
    Delete
}
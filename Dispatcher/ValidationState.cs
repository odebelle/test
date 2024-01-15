namespace Dispatcher;

/// <summary>
/// ValidationState
/// </summary>
public enum ValidationState
{
    /// <summary>
    /// Unvalidated
    /// </summary>
    Unvalidated = 0,

    /// <summary>
    /// Invalid
    /// </summary>
    Invalid = 1,

    /// <summary>
    /// Valid
    /// </summary>
    Valid = 2,

    /// <summary>
    /// Skipped
    /// </summary>
    Skipped = 3
}
using System.Runtime.Serialization;

namespace Shared.Enums;

[Serializable]
public enum TransitStatus
{
    /// <summary>
    /// Sent
    /// </summary>
    [EnumMember(Value = "Sent")]
    Sent,
    /// <summary>
    /// 
    /// </summary>
    [EnumMember(Value = "Confirmed")]
    Confirmed,
    /// <summary>
    /// Accepted
    /// </summary>
    [EnumMember(Value = "Accepted")]
    Accepted,
    /// <summary>
    /// Redirect
    /// </summary>
    [EnumMember(Value = "Redirect")]
    Redirect,
    /// <summary>
    /// Unprocessable
    /// </summary>
    [EnumMember(Value = "Unprocessed")]
    Unprocessed
}
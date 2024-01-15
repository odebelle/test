using Shared.Enums;
using Shared.Models;

namespace Dispatcher;

public class MessageResponse
{
    /// <summary>
    /// Message
    /// </summary>
    public string Message { get; set; } = null!;

    /// <summary>
    /// MessageState
    /// </summary>
    public TransitStatus TransitStatus { get; set; } = TransitStatus.Accepted;

    /// <summary>
    /// ValidationState
    /// </summary>
    public ValidationState ValidationState { get; set; } = ValidationState.Skipped;

    /// <summary>
    /// DeliveryTag
    /// </summary>
    public ulong DeliveryTag { get; set; }
}
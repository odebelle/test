using Shared.Enums;

namespace Shared.Models;

public interface IConsumerElement: IMqttElement
{
    string Id { get; set; }
    string Group { get; set; }
    string Route { get; set; }
    string TopicName { get; set; }
    string? Description { get; set; }
    DateTime? LastConsumption { get; set; }
    string? Information { get; set; }
    ConsumerStatus ConsumerStatus { get; set; }
    RunningStatus RunningStatus { get; set; }
    DateTime? FirstConsumption { get; set; }
    DateTime? LastSuccessfulConsumption { get; set; }
    bool ActionDisabled { get; set; }
    string IconCss { get; set; }
    string? Host { get; set; }
}
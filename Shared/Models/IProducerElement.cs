using Shared.Enums;

namespace Shared.Models;

public interface IProducerElement : IMqttElement
{
    string Id { get; set; }
    string Group { get; set; }
    string Route { get; set; }
    string TopicName { get; set; }
    string? Information { get; set; }
    DateTime FirstRun { get; set; }
    DateTime? LastRun { get; set; }
    DateTime? LastSuccessfulRun { get; set; }
    DateTime? NextRun { get; set; }
    RunningStatus RunningStatus { get; set; }
    TransitStatus PublishStatus { get; set; }
    bool? RunSuccessful { get; set; }
    string IconCss { get; set; }
    bool ActionDisabled { get; set; }
    string CronExpression { get; set; }
    string? Host { get; set; }
    void EnsureIsCorrectlySet();
}
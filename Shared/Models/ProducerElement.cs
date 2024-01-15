using MQTTnet.Client;
using Shared.Enums;

namespace Shared.Models;

public class ProducerElement : IProducerElement
{
    private RunningStatus _runningStatus;

    public ProducerElement()
    {
    }

    /// <summary>
    /// String Id (GUID)
    /// </summary>
    public string Id { get; set; } = null!;
    public string? Information { get; set; }
    public TransitStatus PublishStatus { get; set; }
    public string? LastErrorMessage { get; set; }
    public DateTime? LastError { get; set; }
    public DateTime FirstRun { get; set; }
    public DateTime? LastRun { get; set; }
    public DateTime? LastSuccessfulRun { get; set; }
    public DateTime? NextRun { get; set; }

    public RunningStatus RunningStatus
    {
        get => _runningStatus;
        set
        {
            if (value == _runningStatus) return;
            IconCss = value switch
            {
                RunningStatus.OutOfControl => "warning",
                RunningStatus.Running => "warning",
                RunningStatus.Paused => "play_arrow",
                RunningStatus.Pending => "pause",
                RunningStatus.OnHold => "warning",
                RunningStatus.Unknown => "warning",
                _ => IconCss
            };
            _runningStatus = value;
        }
    }

    public bool? RunSuccessful { get; set; }
    public string IconCss { get; set; } = "pause";
    public bool ActionDisabled { get; set; }
    public string CronExpression { get; set; } = null!;
    public string? Host { get; set; }
    public MqttClientDisconnectReason? MqttClientDisconnectReason { get; set; }
    public string MqttClientStatusReasonString { get; set; } = null!;
    public MqttClientPublishReasonCode MqttClientPublishReasonCode { get; set; }
    public string Group { get; set; } = "null!";
    public string Route { get; set; } = "null!";
    public string TopicName { get; set; } = "null!";

    public void EnsureIsCorrectlySet()
    {
        ThrowMissingMemberException(nameof(Group), Group);
        ThrowMissingMemberException(nameof(TopicName), TopicName);
        ThrowMissingMemberException(nameof(TopicName), TopicName);
    }

    private void ThrowMissingMemberException(string memberName, string content)
    {
        if (string.IsNullOrEmpty(content))
            throw new MissingMemberException(this.GetType().Name, memberName);
    }
}
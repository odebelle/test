using System.ComponentModel;
using System.Runtime.CompilerServices;
using MQTTnet.Client;
using Shared.Enums;

namespace Shared.Models;

public class ConsumerElement : IConsumerElement
{
    public ConsumerElement()
    {
    }

    public ConsumerElement(IConsumerElement element, string route, bool isDeadletter = false)
    {
        var dl = isDeadletter ? "dl." : string.Empty;
        Group = element.Group;
        TopicName = $"{dl}{element.TopicName}";
        Route = route;
        Information = element.Information;
    }

    public string Id { get; set; } = null!;
    public string Group { get; set; } = null!;
    public string Route { get; set; } = null!;
    public string TopicName { get; set; } = null!;
    public string? Description { get; set; }

    public DateTime? LastConsumption { get; set; }

    public string? Information { get; set; }

    public ConsumerStatus ConsumerStatus { get; set; }
    public RunningStatus RunningStatus { get; set; }
    public DateTime? FirstConsumption { get; set; }
    public string? LastErrorMessage { get; set; }
    public DateTime? LastError { get; set; }
    public DateTime? LastSuccessfulConsumption { get; set; }
    public bool IsMapSuccessful { get; set; }
    public bool IsDispatchSuccessful { get; set; }
    public bool ActionDisabled { get; set; }
    public string IconCss { get; set; } = "e-icons e-pause";
    public string? Host { get; set; }
    public string? Marshal { get; set; }
    public MqttClientDisconnectReason MqttBrokerStatus { get; set; }

    #region Intelligence

    public void EnsureIsCorrectlySet()
    {
        ThrowMissingMemberException(nameof(Group), Group);
        ThrowMissingMemberException(nameof(TopicName), TopicName);
        ThrowMissingMemberException(nameof(Route), Route);
    }

    private void ThrowMissingMemberException(string memberName, string content)
    {
        if (string.IsNullOrEmpty(content))
            throw new MissingMemberException(this.GetType().Name, memberName);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }

    #endregion

    public MqttClientDisconnectReason? MqttClientDisconnectReason { get; set; }
    public MqttClientPublishReasonCode MqttClientPublishReasonCode { get; set; }
    public string MqttClientStatusReasonString { get; set; } = null!;
}
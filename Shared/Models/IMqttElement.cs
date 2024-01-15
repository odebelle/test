using MQTTnet.Client;

namespace Shared.Models;

public interface IMqttElement
{
    MqttClientDisconnectReason? MqttClientDisconnectReason { get; set; }
    MqttClientPublishReasonCode MqttClientPublishReasonCode { get; set; }
    string MqttClientStatusReasonString { get; set; }
    
    string? LastErrorMessage { get; set; }
    DateTime? LastError { get; set; }
}
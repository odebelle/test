namespace Shared.Models;

public static class ProducerElementExtensions
{
    public static void UpdateMe(this IProducerElement me, IProducerElement element)
    {
        me.Information = element.Information;
        me.FirstRun = element.FirstRun;
        me.LastRun = element.LastRun;
        me.RunningStatus = element.RunningStatus;
        me.NextRun = element.NextRun;
        me.LastErrorMessage = element.LastErrorMessage;
        me.LastError = element.LastError;
        me.RunSuccessful = element.RunSuccessful;
        me.LastSuccessfulRun = element.LastSuccessfulRun;
        me.PublishStatus = element.PublishStatus;
        // me.IconCss = element.IconCss;
        me.ActionDisabled = element.ActionDisabled;
        me.Host = element.Host;
        me.MqttClientStatusReasonString = element.MqttClientStatusReasonString;
        me.MqttClientDisconnectReason = element.MqttClientDisconnectReason;
        me.MqttClientPublishReasonCode = element.MqttClientPublishReasonCode;
    }
}
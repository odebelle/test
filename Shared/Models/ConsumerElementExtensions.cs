namespace Shared.Models;

public static class ConsumerElementExtensions
{
    public static void UpdateMe(this IConsumerElement me, IConsumerElement element)
    {
        me.Information = element.Information;
        me.Description = element.Description;
        me.FirstConsumption = element.FirstConsumption;
        me.LastConsumption = element.LastConsumption;
        me.ConsumerStatus = element.ConsumerStatus;
        me.RunningStatus = element.RunningStatus;
        me.LastErrorMessage = element.LastErrorMessage;
        me.LastError = element.LastError;
        me.LastSuccessfulConsumption = element.LastSuccessfulConsumption;
        me.ActionDisabled = element.ActionDisabled;
        me.IconCss = me.ConsumerStatus switch
        {
            ConsumerStatus.None => "warning",
            ConsumerStatus.Starting => "warning",
            ConsumerStatus.Started => "warning",
            ConsumerStatus.Running => "pause",
            ConsumerStatus.Paused => "play_arrow",
            ConsumerStatus.Stopping => "warning",
            ConsumerStatus.Stopped => "play_arrow",
            _ => "e-icons e-warning"
        };
    }
}
namespace Shared.Models;

public class DaemonMessage
{
    public string? ServiceName { get; set; }
    public int Result { get; set; }
    public string? TopicResponse { get; set; }
    
    /// <summary>
    /// Apply Status. True for start service,otherwise stop service.
    /// </summary>
    public bool Start { get; set; }
}


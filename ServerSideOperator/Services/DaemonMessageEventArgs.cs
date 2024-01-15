using Shared.Models;

namespace ServerSideOperator.Services;

public class DaemonMessageEventArgs : EventArgs
{
    public DaemonMessage Message { get; set; } = null!;
}
namespace Shared.Enums;

[Serializable]
public enum RunningStatus
{
    OutOfControl,
    Running,
    Paused,
    Pending,
    OnHold,
    Unknown
}
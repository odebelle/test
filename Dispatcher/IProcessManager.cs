namespace Dispatcher;

public interface IProcessManager
{
    event EventHandler? Paused;
    event EventHandler? Resumed;
    void Pause();
    void Resume();
    CancellationTokenSource ListenTokenSource { get; set; }
    CancellationTokenSource DeafTokenSource { get; set; }
}
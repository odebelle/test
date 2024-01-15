namespace Dispatcher;

public class ConsumerProcessManager : IConsumerProcessManager
{
    public ConsumerProcessManager()
    {
    }

    public event EventHandler? Paused;
    public event EventHandler? Resumed;
    public CancellationTokenSource ListenTokenSource { get; set; } = new();
    public CancellationTokenSource DeafTokenSource { get; set; } = new();


    public void Pause()
    {
        Paused?.Invoke(this, EventArgs.Empty);
    }

    public void Resume()
    {
        Resumed?.Invoke(this, EventArgs.Empty);
    }
}
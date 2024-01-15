namespace Dispatcher;

public class ProducerProcessManager : IProducerProcessManager
{
    public event EventHandler? Paused;
    public event EventHandler? Resumed;
    public void Pause()
    {
        Paused?.Invoke(this, EventArgs.Empty);
    }

    public void Resume()
    {
        Resumed?.Invoke(this, EventArgs.Empty);
    }

    public CancellationTokenSource ListenTokenSource { get; set; }
    public CancellationTokenSource DeafTokenSource { get; set; }
}
using Shared.Enums;
using Shared.Models;

namespace Dispatcher;

public abstract class DeadLetterImplementation : IDeadLetterProducer
{
    protected abstract Task<TransitStatus> PublishMessageAsync(object messageHolder);

    protected DeadLetterImplementation()
    {
        Topic = DefaultExchangeName.DeadLetter;
    }

    protected string Topic { get; }

    public Task<TransitStatus> SendAsync(object body)
    {
        return PublishMessageAsync(body);
    }

    #region IDisposable

    private bool _disposed;

    public void Dispose()
    {
        // Dispose of unmanaged resources.
        Dispose(true);
        // Suppress finalization.
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
        {
            return;
        }

        if (disposing)
        {
            // TODO: dispose managed state (managed objects).
        }

        // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
        // TODO: set large fields to null.

        _disposed = true;
    }

    #endregion
}
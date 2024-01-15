using Microsoft.Extensions.Configuration;
using Shared.Enums;
using Shared.Models;

namespace Dispatcher;

public abstract class Producer<TSource> : IProducer<TSource> where TSource : class
{
    // ReSharper disable once UnusedAutoPropertyAccessor.Local
    private IConfiguration Configuration { get; }

    protected abstract Task<TransitStatus> PublishMessage(IMessageHolder<TSource> messageHolder);

    protected Producer(IConfiguration configuration)
    {
        Configuration = configuration;
        Topic = typeof(TSource).GetTopicName();
    }

    public Task<TransitStatus> SendAsync(IEnumerable<TSource> body)
    {
        var message = new MessageHolder<TSource>(body);
        return PublishMessage(message);
    }

    public Task<TransitStatus> StackMessage<TPayoff>(IMessageHolder<TSource, TPayoff> messageHolder)
    {
        throw new NotImplementedException();
    }

    public string Topic { get; }


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
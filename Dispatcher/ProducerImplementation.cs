using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Shared.Enums;
using Shared.Models;

namespace Dispatcher;

public abstract class ProducerImplementation<TSource> : IProducer<TSource> where TSource : class
{
    private readonly ILogger _logger;
    protected IConfiguration? Configuration { get; }

    protected abstract Task<TransitStatus> PublishMessage(IMessageHolder<TSource> messageHolder);
    public abstract Task<TransitStatus> StackMessage<TPayoff>(IMessageHolder<TSource, TPayoff> messageHolder);

    protected ProducerImplementation(IConfiguration configuration, ILogger logger)
    {
        _logger = logger;
        Configuration = configuration;
        Topic = typeof(TSource).GetTopicName();
    }

    public Task<TransitStatus> SendAsync(IEnumerable<TSource> body)
    {
        var message = new MessageHolder<TSource>(body);
        return PublishMessage(message);
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
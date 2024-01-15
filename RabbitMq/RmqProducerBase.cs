using System.Text.Json;
using Dispatcher;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Shared;
using Shared.Enums;
using Shared.Models;

namespace RabbitMq;

public abstract class RmqProducerBase<TSource> : ProducerImplementation<TSource> where TSource : class
{
    private IModel? _channel;
    private ConnectionFactory? _connectionFactory;
    private IConnection? _connection;
    protected BrokerConfiguration BrokerConfiguration { get; set; } = new();

    protected MessageResponse? ConfirmResult { get; set; }
    protected string? Exchange { get; private set; }

    public event EventHandler<BasicReturnEventArgs>? OnBasicReturn;

    protected RmqProducerBase(IConfiguration configuration, ILogger logger) : base(configuration, logger)
    {
        Configuration.Bind(nameof(BrokerOptions), BrokerConfiguration);
    }

    /// <summary>
    /// Channel
    /// </summary>
    protected IModel Channel
    {
        get
        {
            if (_channel != null)
                return _channel;

            _channel = Connection.CreateModel();
            _channel.ContinuationTimeout =
                BrokerConfiguration.ChannelContinuationTimeOut ?? new TimeSpan(0, 0, 60);

            _channel.ConfirmSelect();
            _channel.BasicAcks += ChannelBasicAcks;
            _channel.BasicNacks += ChannelBasicReject;
            _channel.BasicReturn += ChannelBasicReturn;

            return _channel;
        }
    }

    private void ChannelBasicReturn(object? sender, BasicReturnEventArgs e)
    {
        var body = JsonSerializer.Deserialize<MessageHolder<TSource>>(e.Body.ToArray());
        body!.ErrorUtc = DateTime.UtcNow;
        body.Topic = Exchange ?? typeof(TSource).Name;
        body.TransitStatus = TransitStatus.Unprocessed;

        e.Body = body.SerializeToUtf8Bytes();

        OnBasicReturn?.Invoke(sender, e);
    }

    protected abstract void ChannelBasicReject(object? sender, BasicNackEventArgs e);
    protected abstract void ChannelBasicAcks(object? sender, BasicAckEventArgs e);

    /// <summary>
    /// Allow the publisher to Reconnect his connection
    /// </summary>
    protected void Reconnect()
    {
        _connection = null;
        _channel = null;
    }

    /// <summary>
    /// RabbitMqConnector
    /// </summary>
    /// <param name="exchangeName"></param>
    protected void ConfigureGlobalExchange(string? exchangeName)
    {
        Exchange = exchangeName;
        BasicProperties = Channel.CreateBasicProperties();
    }

    /// <summary>
    /// BasicProperties
    /// </summary>
    protected IBasicProperties? BasicProperties { get; set; }

    /// <summary>
    /// Connection
    /// </summary>
    private IConnection Connection => _connection ??= ConnectionFactory.CreateConnection();

    /// <summary>
    /// ConnectionFactory
    /// </summary>
    private ConnectionFactory ConnectionFactory
    {
        get
        {
            _connectionFactory ??= new ConnectionFactory().SetBasicConnectionProperties(BrokerConfiguration);

            return _connectionFactory;
        }
    }


    #region IDisposable Support

    private void Close()
    {
        // todo: create close exception

        _channel?.Close();

        _channel = null;
        if (_connection is { IsOpen: true })
            _connection.Close();

        _connection = null;
        _connectionFactory = null;
    }

    private bool _disposedValue; // Pour détecter les appels redondants

    /// <summary>
    /// Dispose
    /// </summary>
    /// <param name="disposing"></param>
    protected override void Dispose(bool disposing)
    {
        if (_disposedValue)
            return;

        if (disposing)
        {
            // TODO: supprimer l'état managé (objets managés).
        }

        Close();

        _disposedValue = true;
    }

    #endregion
}
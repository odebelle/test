using Dispatcher;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Shared.Enums;
using Shared.Models;

namespace RabbitMq;

public sealed class RmqDeadLetterProducer : DeadLetterImplementation
{
    private bool _disposedValue; // Pour détecter les appels redondants
    private IModel? _channel;
    private IConnection? _connection;
    private ConnectionFactory? _connectionFactory;
    private MessageResponse? _confirmResult;
 
    private readonly BrokerConfiguration _brokerConfiguration;
    private readonly System.Collections.Concurrent.ConcurrentDictionary<ulong, object> _confirmations = new();

    public event EventHandler<BasicReturnEventArgs>? OnBasicReturn; 

    public RmqDeadLetterProducer(BrokerConfiguration options)
    {
        _brokerConfiguration = options;
        _basicProperties = Channel.CreateBasicProperties();
    }

    protected override Task<TransitStatus> PublishMessageAsync(object messageHolder)
    {
        try
        {
            _ = _confirmations.TryAdd(Channel.NextPublishSeqNo, messageHolder);
            _confirmResult = Publish(messageHolder);

            Channel.WaitForConfirms();

            return Task.FromResult(TransitStatus.Confirmed);
        }
        catch (Exception)
        {
            return Task.FromResult(TransitStatus.Unprocessed);
        }

    }

    private MessageResponse Publish(object model)
    {
        if (Channel.IsClosed)
            Reconnect();

        if (_brokerConfiguration.AllowDeclaration)
            Channel.ExchangeDeclare(Topic, ExchangeType.Topic, true, false, null);
        
        var body = model.ToJson().GetBytesUtf8();

        Channel.BasicPublish(
            exchange: Topic,
            routingKey: DefaultExchangeName.DeadLetter,
            mandatory: true,
            basicProperties: _basicProperties,
            body: body);
        
        return new MessageResponse
        {
            Message = "MESSAGE SENT",
            TransitStatus = TransitStatus.Sent,
            ValidationState = ValidationState.Valid
        };
    }

    /// <summary>
    /// Channel
    /// </summary>
    private IModel Channel
    {
        get
        {
            if (_channel != null)
                return _channel;

            _channel = Connection.CreateModel();
            _channel.ContinuationTimeout =
                _brokerConfiguration.ChannelContinuationTimeOut ?? new TimeSpan(0, 0, 60);

            _channel.ConfirmSelect();
            _channel.BasicAcks += ChannelBasicAcks;
            _channel.BasicNacks += ChannelBasicReject;
            _channel.BasicReturn += ChannelBasicReturn;

            return _channel;
        }
    }

    private void ChannelBasicReturn(object? sender, BasicReturnEventArgs e)
    {
        // There are several common types of publisher errors that are handled using different protocol features:
        
        // Publishing to a non-existent exchange results in a channel error, which closes the channel so that no further publishing (or any other operation) is allowed on it.
        
        // When a published message cannot be routed to any queue (e.g. because there are no bindings defined for the target exchange),
        // and the publisher set the mandatory message property to false (this is the default),
        // the message is discarded or republished to an alternate exchange, if any.
        
        // When a published message cannot be routed to any queue, and the publisher set the mandatory message property to true,
        // the message will be returned to it.
        // The publisher must have a returned message handler set up in order to handle the return
        // (e.g. by logging an error or retrying with a different exchange)
        
        
        // my reflexion is set mandatory to true and return the reply text and start event
        OnBasicReturn?.Invoke(this, e);
    }

    private void CleanConfirmations(ulong deliveryTag, bool multiple)
    {
        if (multiple)
        {
            var confirmed = _confirmations.Where(w => w.Key <= deliveryTag);
            foreach (var item in confirmed)
            {
                _confirmations.TryRemove(item.Key, out _);
            }
        }
        else
        {
            _confirmations.TryRemove(deliveryTag, out _);
        }
    }

    private void ChannelBasicReject(object? sender, BasicNackEventArgs e)
    {
        _ = _confirmations.TryGetValue(e.DeliveryTag, out _);
        _confirmResult = new MessageResponse
        {
            DeliveryTag = e.DeliveryTag,
            Message = $"Publish for {e.DeliveryTag} not confirmed",
            TransitStatus = TransitStatus.Redirect
        };
        CleanConfirmations(e.DeliveryTag, e.Multiple);
    }
    
    private void ChannelBasicAcks(object? sender, BasicAckEventArgs e)
    {
        _ = _confirmations.TryGetValue(e.DeliveryTag, out _);
        _confirmResult = new MessageResponse
        {
            DeliveryTag = e.DeliveryTag,
            Message = $"Publish for {e.DeliveryTag} confirmed.",
            TransitStatus = TransitStatus.Confirmed
        };
        CleanConfirmations(e.DeliveryTag, e.Multiple);
    }


    /// <summary>
    /// Allow the publisher to Reconnect his connection
    /// </summary>
    private void Reconnect()
    {
        _connection = null;
        _channel = null;
    }

    /// <summary>
    /// BasicProperties
    /// </summary>
    private IBasicProperties? _basicProperties;

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
            _connectionFactory ??= new ConnectionFactory().SetBasicConnectionProperties(_brokerConfiguration);

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
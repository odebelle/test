using Dispatcher;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Shared.Enums;
using Shared.Models;

namespace RabbitMq;

public sealed class RmqMarshalProducer<TSource> : RmqProducerBase<TSource> where TSource: class
{
    private readonly System.Collections.Concurrent.ConcurrentDictionary<ulong, object> _confirmations = new();
    private readonly string _exchangeName;
    private readonly string _marshalQueueName;

    public RmqMarshalProducer(IConfiguration configuration, string? exchangeName, ILogger logger) : base(configuration, logger)
    {
        _exchangeName = $"{ExchangePrefix.Marshal}.{exchangeName}";
        _marshalQueueName = $"{QueuePrefix.Marshal}.{exchangeName}";
        ConfigureGlobalExchange(_exchangeName);


        if (!BrokerConfiguration.AllowDeclaration) 
            return;
        
        var arguments = new Dictionary<string, object> { { "x-single-active-consumer", true } };
        
        Channel.BasicQos(0, 10, true);
        Channel.ExchangeDeclare(Exchange, ExchangeType.Fanout, true, false, null);
        Channel.QueueDeclare(_marshalQueueName, true, false, false, arguments);
        Channel.QueueBind(_marshalQueueName, Exchange, "", null);
    }

    protected override void ChannelBasicReject(object? sender, BasicNackEventArgs e)
    {
        _ = _confirmations.TryGetValue(e.DeliveryTag, out _);
        ConfirmResult = new MessageResponse
        {
            DeliveryTag = e.DeliveryTag,
            Message = $"Publish for {e.DeliveryTag} not confirmed",
            TransitStatus = TransitStatus.Redirect
        };
        CleanConfirmations(e.DeliveryTag, e.Multiple);
    }

    /// <summary>
    /// ChannelBasicAcks
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    protected override void ChannelBasicAcks(object? sender, BasicAckEventArgs e)
    {
        _ = _confirmations.TryGetValue(e.DeliveryTag, out _);
        ConfirmResult = new MessageResponse
        {
            DeliveryTag = e.DeliveryTag,
            Message = $"Publish for {e.DeliveryTag} confirmed.",
            TransitStatus = TransitStatus.Confirmed
        };
        CleanConfirmations(e.DeliveryTag, e.Multiple);
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

    protected override Task<TransitStatus> PublishMessage(IMessageHolder<TSource> messageHolder)
        => throw new NotImplementedException("Publish unexpected! Are you sure you don't want to marshal instead?");

    public override Task<TransitStatus> StackMessage<TPayoff>(IMessageHolder<TSource, TPayoff> messageHolder)
    {
        _ = _confirmations.TryAdd(Channel.NextPublishSeqNo, messageHolder);
        messageHolder.Topic = _exchangeName;
        messageHolder.Subject = _marshalQueueName;
        messageHolder.TransitStatus = TransitStatus.Redirect;
        messageHolder.PublishUtc = DateTime.UtcNow;

        ConfirmResult = PublishAndConfirm(messageHolder);

        if (!Channel.WaitForConfirms())
        {
            messageHolder.ErrorUtc = DateTime.UtcNow;
        }

        return Task.FromResult(ConfirmResult.TransitStatus);
    }

    private MessageResponse Publish(object model)
    {
        if (Channel.IsClosed)
            Reconnect();


        var body = model.ToJson().GetBytesUtf8();

        Channel.BasicPublish(
            exchange: Exchange,
            routingKey: "",
            mandatory: true,
            basicProperties: BasicProperties,
            body: body);
        return new MessageResponse
        {
            Message = "MESSAGE SENT",
            TransitStatus = TransitStatus.Sent,
            ValidationState = ValidationState.Valid
        };
    }

    private MessageResponse PublishAndConfirm(object model)
    {
        return Publish(model);
    }
}
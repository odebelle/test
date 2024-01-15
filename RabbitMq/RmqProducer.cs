using Dispatcher;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Shared.Enums;
using Shared.Models;

namespace RabbitMq;

public sealed class RmqProducer<TSource> : RmqProducerBase<TSource> where TSource : class
{
    private readonly ILogger _logger;
    private readonly System.Collections.Concurrent.ConcurrentDictionary<ulong, IMessageHolder<TSource>> _confirmations = new();

    public RmqProducer(IConfiguration serviceProvider, ILogger logger) : base(serviceProvider, logger)
    {
        _logger = logger;
        ConfigureGlobalExchange($"{ExchangePrefix.Default}.{Topic}");
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
    {
        if (messageHolder is { TransitStatus: TransitStatus.Unprocessed })
        {
            _ = _confirmations.TryAdd(Channel.NextPublishSeqNo, messageHolder);
            messageHolder.TransitStatus = TransitStatus.Sent;
            ConfirmResult = PublishAndConfirm(messageHolder);

            if (!Channel.WaitForConfirms())
            {
                messageHolder.ErrorUtc = DateTime.UtcNow;
            }

            return Task.FromResult(ConfirmResult.TransitStatus);
        }

        messageHolder.ErrorUtc = DateTime.UtcNow;

        return Task.FromResult(TransitStatus.Unprocessed);
    }

    public override Task<TransitStatus> StackMessage<TPayoff>(IMessageHolder<TSource, TPayoff> messageHolder)
    {
        return Task.FromException<TransitStatus>(new NotImplementedException("Marshal unexpected!"));
    }

    private MessageResponse Publish(IMessageHolder<TSource> model)
    {
        if (Channel.IsClosed)
            Reconnect();

        if (BrokerConfiguration.AllowDeclaration)
            Channel.ExchangeDeclare(Exchange, ExchangeType.Fanout, true, false, null);
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

    private MessageResponse PublishAndConfirm(IMessageHolder<TSource> model)
    {
        model.PublishUtc = DateTime.UtcNow;

        return Publish(model);
    }
}
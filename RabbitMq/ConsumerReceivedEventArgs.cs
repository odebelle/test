using Dispatcher;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace RabbitMq;

public class ConsumerReceivedEventArgs<TSource> where TSource : class
{
    /// <summary>
    /// Source
    /// </summary>
    public MessageHolder<TSource>? Source { get; set; }

    /// <summary>
    /// BasicDeliverEventArgs
    /// </summary>
    public BasicDeliverEventArgs? BasicDeliverEventArgs { get; set; }

    /// <summary>
    /// QueueName
    /// </summary>
    public string? QueueName { get; set; }

    /// <summary>
    /// RabbitMQ Model
    /// </summary>
    public IModel? Model { get; set; }

    /// <summary>
    /// Nack
    /// </summary>
    public void Nack()
    {
        Model?.BasicNack(BasicDeliverEventArgs?.DeliveryTag ?? 1, false, false);
    }

    /// <summary>
    /// Ack
    /// </summary>
    public void Ack()
    {
        Model?.BasicAck(BasicDeliverEventArgs?.DeliveryTag ?? 1, false);
    }
}
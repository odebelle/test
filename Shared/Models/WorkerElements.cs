using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Shared.Models;

public class WorkerElements : IWorkerElements
{ 
    public WorkerElements()
    {
    }

    private ConsumerElement? InitConsumerElement(dynamic dispatch)
    {
        ConsumerElement? element = null;
        var consumer = dispatch.Consumer;

        if (consumer is null)
            return element;

        element = new ConsumerElement
        {
            Information = consumer.Information,
            Group = dispatch.Subject ?? dispatch.Name,
            Route = dispatch.Name,
            TopicName = consumer.Topic,
            Marshal = consumer.Marshal,
            Description = consumer.Information
        };

        return element;
    }

    public DateTime Time { get; set; } = DateTime.Now;
    public string Route { get; set; } = null!;
    public IProducerElement? ProducerElement { get; set; }
    public ConsumerElement? ConsumerElement { get; set; }

    public string? Marshal { get; set; }

    public ConsumerElement? DeadLetterElement { get; set; }
}
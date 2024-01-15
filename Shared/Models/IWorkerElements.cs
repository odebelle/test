namespace Shared.Models;

public interface IWorkerElements
{
    DateTime Time { get; set; }
    string Route { get; set; }
    IProducerElement? ProducerElement { get; set; }
    ConsumerElement? ConsumerElement { get; set; }
    string? Marshal { get; set; }
    ConsumerElement? DeadLetterElement { get; set; }
}
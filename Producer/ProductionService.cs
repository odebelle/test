using Dispatcher;
using Producer.Cron;

namespace Producer;

internal static class Producer
{
    internal static IEnumerable<DummyClass> Execute(ILogger<BackgroundService>? logger, CancellationToken stoppingToken)
    {
        return Enumerable.Range(1, 2).Select(s => new DummyClass()
        {
            Name = $"{nameof(DummyClass)}_{s}",
            Number = s,
            TsDateTime = DateTime.Today.AddDays(s)
        });
    }
}
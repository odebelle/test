namespace ServerSideOperator.Services;

public class PingService(ILogger<PingService> logger, IMqttService mqttService, IConfiguration configuration)
    : BackgroundService
{
    private readonly int _millisecondsDelay = configuration.GetValue<int>(nameof(PingService));

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await mqttService.ConnectAsync(stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            var errorList = await mqttService.PingWorkersAsync();
            if (errorList.Count != 0)
            {
                logger.LogWarning("Some error was occured while trying to receive workers status.\n\t{@errorLIst}", errorList);
            }

            logger.LogInformation("Workers information's next pull refresh at: {@NextPullRefresh}",
                DateTime.Now.AddMilliseconds(_millisecondsDelay));

            await Task.Delay(_millisecondsDelay, stoppingToken);
        }
    }
}
using Dispatcher;
using RabbitMQ.Client.Exceptions;
using Shared.Enums;
using Shared.Models;

namespace Producer.Cron;

public class CronBackgroundService<TSource> : BackgroundService where TSource: class
{
    private readonly AutoResetEvent _autoEvent;
    private readonly CronExpression _cronExpression;
    private CancellationToken _stoppingToken = CancellationToken.None;
    private Timer? _timer;
    private bool _pending;
    private readonly bool _noWait = true;
    private readonly ILogger<BackgroundService> _logger;
    private readonly IWorkerElements _workerElement;
    private bool _isFirstRun;
    private readonly IDispatcherProducer _context;
    private readonly IMqttWorkerService _mqttWorkerService;
    private readonly string _producerName;
    private readonly IProducerElement _element;

    private IProducerProcessManager ProcessManager { get; }
    private DateTimeOffset FirstRun { get; set; }
    private DateTimeOffset NextRun { get; set; }

    public CronBackgroundService(ILogger<BackgroundService> logger, IProducerProcessManager processManager,
        IConfiguration configuration, IDispatcherProducer context, IMqttWorkerService mqttWorkerService, IProducerElement element)
    {
        element.EnsureIsCorrectlySet();
        _element = element;
        _context = context;
        _producerName = _element.TopicName;
        _mqttWorkerService = mqttWorkerService;
        
        
        _logger = logger;
        _workerElement = new WorkerElements();

        configuration.Bind(nameof(WorkerElements), _workerElement);

        var cronExpression = _workerElement.ProducerElement?.CronExpression;
        if (cronExpression is not null)
        {
            ProcessManager = new ProducerProcessManager();
            _autoEvent = new AutoResetEvent(false);
            _cronExpression = new CronExpression(cronExpression);
            ProcessManager.ListenTokenSource = new CancellationTokenSource();
            ProcessManager.Paused += OnProducerPaused;
            ProcessManager.Resumed += OnProducerResumed;
        }
        else
        {
            throw new ArgumentException($"{nameof(cronExpression)} is missing");
        }
    }

    private async Task SendChangedStatusAsync(RunningStatus? runningStatus = null, CancellationToken? stoppingToken = null)
    {
        await ChangeStatusAsync(runningStatus, stoppingToken);
        try
        {
            await _mqttWorkerService.InitializeAsync(stoppingToken);
            await _mqttWorkerService.SendProducerElementAsync(_element, stoppingToken ?? CancellationToken.None);
        }
        catch (Exception e)
        {
            _logger?.LogWarning(e, "Unable to initialize MQTT Broker: {@message}", e.Message);
        }
    }

    private Task ChangeStatusAsync(RunningStatus? runningStatus = null, CancellationToken? stoppingToken = null)
    {
        _element.RunningStatus = runningStatus ?? _element.RunningStatus;

        if (_element.RunningStatus == RunningStatus.Pending && (_element.RunSuccessful ?? false))
        {
            _element.LastSuccessfulRun = _element.NextRun;
        }

        if (_element.RunningStatus == RunningStatus.Pending)
        {
            _element.LastRun = _element.NextRun;
            _element.NextRun = NextRun.DateTime;
        }

        _element.Information = $"{_element.RunningStatus}";

        return Task.CompletedTask;
    }

    private Task ResumeStatus(CancellationToken? stoppingToken)
    {
        _element.NextRun = NextRun.DateTime;
        _element.RunningStatus = RunningStatus.Pending;

        return Task.CompletedTask;
    }

    private void OnProducerResumed(object? sender, EventArgs e)
    {
        ProcessManager.ListenTokenSource = new CancellationTokenSource();
        ResumeStatus(_stoppingToken);
    }

    private void OnProducerPaused(object? sender, EventArgs e)
    {
        ProcessManager.ListenTokenSource.Cancel();
        if (_pending)
            ChangeStatusAsync(RunningStatus.Paused, _stoppingToken);
    }

    protected sealed override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _stoppingToken = stoppingToken;
        FirstRun = DateTimeOffset.Now;
        NextRun = _cronExpression.GetNextValidTimeAfter(DateTimeOffset.Now) ?? DateTimeOffset.Now;

        while (!stoppingToken.IsCancellationRequested)
        {
            if (_noWait)
            {
                _timer = new Timer(Callback!, _autoEvent, 1, Timeout.Infinite);

                _autoEvent.WaitOne();
            }

            ConfigureTimer();

            try
            {
                await Task.Delay(-1, stoppingToken);
            }
            catch (TaskCanceledException)
            {
                Console.WriteLine("Task cancelled at {0:F}", DateTime.Now);
                _autoEvent.Set();
                _autoEvent.WaitOne(-1, _pending);
            }
        }
    }

    private async void Callback(object state)
    {
        _pending = false;
        if (_stoppingToken.IsCancellationRequested)
            return;

        if (!ProcessManager.ListenTokenSource.IsCancellationRequested)
        {
            await Process(_stoppingToken);
        }

        ConfigureTimer();

        _autoEvent.Set();

        if (ProcessManager.ListenTokenSource.IsCancellationRequested)
        {
            await SendChangedStatusAsync(RunningStatus.Paused, _stoppingToken);
            _logger.LogInformation("Pause requested for Producer: {@Name} ", _workerElement.ProducerElement?.Group);
        }
        else
        {
            await SendChangedStatusAsync(RunningStatus.Pending, _stoppingToken);
            _logger.LogInformation("Pending requested for Producer: {@Name} ", _workerElement.ProducerElement?.Group);
        }

        _pending = true;

        _autoEvent.WaitOne();
    }

    private void ConfigureTimer()
    {
        try
        {
            if (_stoppingToken.IsCancellationRequested && _timer != null)
            {
                _timer.Dispose();
                return;
            }

            var dueTime = GetDueTime();


            if (_timer == null)
            {
                _timer = new Timer(Callback!, _autoEvent, dueTime, Timeout.InfiniteTimeSpan);
            }
            else
                _timer.Change(dueTime, Timeout.InfiniteTimeSpan);
        }
        catch (Exception e)
        {
            // TODO : Catch this critical error
            Console.WriteLine(e.Message);
            _timer?.Change(5000, 60000);
        }
    }

    private TimeSpan GetDueTime()
    {
        var now = DateTime.Now;

        NextRun = _cronExpression.GetNextValidTimeAfter(now) ?? now.AddMinutes(5);

        TimeSpan dueTime = NextRun - now;
        return dueTime;
    }

    private async Task<bool> Process(CancellationToken stoppingToken)
    {
        try
        {
            if (_isFirstRun)
            {
                _logger?.LogInformation("First occurence for '{@_producerName}': {@FirstOccurence}", _producerName,
                    FirstRun.DateTime);

                _element.FirstRun = FirstRun.DateTime;
                _element.LastRun = FirstRun.DateTime;
                _element.NextRun = NextRun.DateTime;
                _element.PublishStatus = TransitStatus.Unprocessed;
            }

            _logger?.LogInformation("Producer '{@_producerName}' Running at: {@RunningAt}", _producerName,
                DateTime.Now);

            // TODO: check if running possible in case of multiple producer (load balancing implementation)
            _ = SendChangedStatusAsync(RunningStatus.Running, stoppingToken);

            var produced = Producer.Execute(_logger, stoppingToken);

            _element.PublishStatus = await _context.PublishAsync(produced);
            _logger?.LogInformation(
                "Status for message from '{@_producerName}': {@TransitStatus}\n\t{@Confirmed}",
                _producerName, _element.PublishStatus, produced);
        }
        catch (BrokerUnreachableException brokerUnreachableException)
        {
            _element.PublishStatus = TransitStatus.Unprocessed;
            _element.LastErrorMessage = $"RabbitMq : {brokerUnreachableException.Message}";
            _element.LastError = DateTime.UtcNow;

            // TODO: release check load balancing

            _logger?.LogError(brokerUnreachableException, "RabbitMq: {@Message}", brokerUnreachableException.Message);
        }
        catch (Exception ex)
        {
            _element.PublishStatus = TransitStatus.Unprocessed;
            _element.LastErrorMessage = ex.Message;
            _element.LastError = DateTime.UtcNow;

            // TODO: release check load balancing

            _logger?.LogError(ex, "{@Message}", ex.Message);
        }

        _element.RunSuccessful = _element.PublishStatus == TransitStatus.Confirmed;
        _isFirstRun = false;

        _ = SendChangedStatusAsync(RunningStatus.Pending, stoppingToken);

        _logger?.LogInformation("Next occurence for producer '{@_producerName}': {@NextOccurence}",
            _producerName, NextRun);

        return _element.RunSuccessful ?? false;
    }
}
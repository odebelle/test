using System.Collections.ObjectModel;
using Microsoft.AspNetCore.Components;
using Radzen;
using ServerSideOperator.Services;
using Shared.Enums;
using Shared.Models;

namespace ServerSideOperator.Components.Pages.Services;

public partial class Services
{
    [Inject] public IMqttService MqttService { get; set; } = null!;

    private readonly ObservableCollection<IProducerElement> _producers = new();
    private readonly ObservableCollection<IConsumerElement> _consumers = new();

    private readonly ConsumerComparer _consumerComparer = new();
    private readonly ProducerComparer _producerComparer = new();

    protected override async Task OnInitializedAsync()
    {
        UpdateConsumerCollection();
        UpdateProducerCollection();

        MqttService.OnProducerChange += async () => await InvokeAsync(UpdateProducerCollection);
        MqttService.OnConsumerChange += async () => await InvokeAsync(UpdateConsumerCollection);
        MqttService.OnDaemonChange += DaemonChanged;

        await base.OnInitializedAsync();
    }

    private void DaemonChanged(object? sender, DaemonMessageEventArgs e)
    {
        throw new NotImplementedException();
    }

    private void UpdateConsumerCollection()
    {
        MqttService.Consumers.Except(_consumers, _consumerComparer).ForEach(f => _consumers.Add(f));

        MqttService.Consumers.Intersect(_consumers, _consumerComparer).ForEach(f =>
        {
            var consumer = _consumers.Single(s => s.Equals(f));
            consumer.UpdateMe(f);
            MqttService.Consumers.Single(s => s.Equals(f)).UpdateMe(f);
        });
        InvokeAsync(StateHasChanged);
    }

    private void UpdateProducerCollection()
    {
        MqttService.Producers.Except(_producers, _producerComparer).ForEach(f => _producers.Add(f));

        MqttService.Producers.Intersect(_producers, _producerComparer).ForEach(f =>
        {
            var producer = _producers.Single(s => s.Equals(f));
            producer.UpdateMe(f);
            MqttService.Producers.Single(s => s.Equals(f)).UpdateMe(f);
        });
        InvokeAsync(StateHasChanged);
    }

    private async Task SendProducerStateAsync(IProducerElement element)
    {
        string action = string.Empty;
        bool allowChange = false;

        switch (element.RunningStatus)
        {
            case RunningStatus.Running:
                element.ActionDisabled = true;
                break;
            case RunningStatus.Paused:
                allowChange = true;
                action = WorkerAction.Resume;
                break;
            case RunningStatus.Pending:
                allowChange = true;
                action = WorkerAction.Pause;
                break;
            case RunningStatus.OnHold:
                break;
            case RunningStatus.Unknown:
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(element.RunningStatus));
        }

        if (allowChange)
        {
            element.RunningStatus = RunningStatus.OnHold;
            element.ActionDisabled = true;
            await MqttService.SendToTopic($"{element.Route}.{WorkerControllerPath.Producer}.{action}");
        }
        else element.ActionDisabled = false;
    }

    private async Task SendConsumerElement(IConsumerElement element)
    {
        var action = string.Empty;
        bool allowChange;

        element.ActionDisabled = true;

        switch (element.ConsumerStatus)
        {
            case ConsumerStatus.None:
                allowChange = false;
                break;
            case ConsumerStatus.Starting:
                allowChange = false;
                break;
            case ConsumerStatus.Started:
                allowChange = true;
                action = WorkerAction.Pause;
                break;
            case ConsumerStatus.Running:
                allowChange = true;
                action = WorkerAction.Pause;
                break;
            case ConsumerStatus.Paused:
                allowChange = true;
                action = WorkerAction.Resume;
                break;
            case ConsumerStatus.Stopping:
                allowChange = true;
                action = WorkerAction.Resume;
                break;
            case ConsumerStatus.Stopped:
                allowChange = true;
                action = WorkerAction.Resume;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(element.RunningStatus));
        }

        if (allowChange)
        {
            element.ConsumerStatus = ConsumerStatus.None;
            element.ActionDisabled = true;
            await MqttService.SendToTopic($"{element.Route}.{WorkerControllerPath.Consumer}.{action}");
            // await WorkerClient.ChangeConsumerStateAsync(element.Key.Route, action,
            //     element.Key);
        }
    }

    private void ConsumerCellRender(DataGridCellRenderEventArgs<IConsumerElement> obj)
    {
        if (obj.Data.Information == Constants.Unreachable)
        {
            obj.Attributes["class"] = "rz-background-color-danger";
        }
    }


    private void OnConsumerGridRender(DataGridRenderEventArgs<IConsumerElement> obj)
    {
        if (obj.FirstRender)
        {
            obj.Grid.Groups.Add(new GroupDescriptor() { Property = "Group", SortOrder = SortOrder.Ascending });
        }
    }


    private void OnProducerGridRender(DataGridRenderEventArgs<IProducerElement> obj)
    {
        if (obj.FirstRender)
        {
            obj.Grid.Groups.Add(new GroupDescriptor() { Property = "Group", SortOrder = SortOrder.Ascending });
        }
    }

    private void OnProducerCellRender(DataGridCellRenderEventArgs<IProducerElement> obj)
    {
        if (obj.Data.Information == Constants.Unreachable)
        {
            obj.Attributes["class"] = "rz-background-color-danger";
        }
    }
}
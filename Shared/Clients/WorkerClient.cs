using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using Shared.Models;

namespace Shared.Clients;

public class WorkerClient : IWorkerClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<WorkerClient> _logger;

    public WorkerClient(HttpClient httpClient, ILogger<WorkerClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<bool?> ChangeProducerStateAsync(string route, string action, string? jobKey = null)
    {
        var r = string.Join('/', route, WorkerControllerPath.Producer, action);
        var result = await _httpClient.PostAsync(r, new StringContent(string.Empty), CancellationToken.None);
        if (result.IsSuccessStatusCode)
        {
            _logger.LogInformation("State sent to {@route} with response code '{@Code}'", route, result.StatusCode);
        }
        else
        {
            _logger.LogWarning("State '{@action}' sent to {@route} is unsuccessful with reason phrase: {@ReasonPhrase}",
                action, route, result.ReasonPhrase);
        }

        return result.IsSuccessStatusCode;
    }

    public async Task<bool?> ChangeConsumerStateAsync(string route, string action, string? key = null)
    {
        var r = string.Join('/', route, WorkerControllerPath.Consumer, action);
        var result = await _httpClient.PostAsync(r, new StringContent(string.Empty), CancellationToken.None);
        if (result.IsSuccessStatusCode)
        {
            _logger.LogInformation("State '{@action}' sent to {@route} with response code '{@Code}'",
                action, result.RequestMessage!.RequestUri?.AbsoluteUri, result.StatusCode);
        }
        else
        {
            _logger.LogWarning("State '{@action}' sent to {@route} is unsuccessful with reason phrase: {@ReasonPhrase}",
                action, result.RequestMessage!.RequestUri?.AbsoluteUri, result.ReasonPhrase);
        }

        return result.IsSuccessStatusCode;
    }

    public async Task<WorkerElements?> GetWorkerElementsAsync(string route)
    {
        var result = await _httpClient.GetFromJsonAsync<WorkerElements?>(route);
        _logger.LogInformation("Message get from \n\t\t{@result}", result);

        return result;
    }

    public Task<ProducerElement[]?> GetProducerElementsAsync(string route)
    {
        throw new NotImplementedException();
    }

    public Task<HttpResponseMessage> TestAsync(string route)
    {
        return _httpClient.GetAsync(route);
    }
}
using System.Text.Json;
using Persistence;

namespace ServerSideOperator.Services;

public interface IWorkerPersistenceClient
{
    Task<Producer[]> GetProducers(int userId, CancellationToken? cancellationToken = null);
}

public sealed class WorkerPersistenceClient(HttpClient httpClient, ILogger<WorkerPersistenceClient> logger)
    : IDisposable, IWorkerPersistenceClient
{
    public async Task<Producer[]> GetProducers(int userId, CancellationToken? cancellationToken = null)
    {
        var ct = cancellationToken ??= CancellationToken.None;
        var uri = "producer";
        var u = new Uri(uri);
        try
        {
            var p = await httpClient.GetAsync(u, HttpCompletionOption.ResponseContentRead, ct);
            p.EnsureSuccessStatusCode();

            var result = await p.Content.ReadFromJsonAsync<Producer[]>();
            var result2 =
                await httpClient.GetFromJsonAsync<Producer[]>(uri,
                    new JsonSerializerOptions(JsonSerializerDefaults.Web)) ?? Array.Empty<Producer>();

            return result ?? result2;
        }
        catch (Exception ex)
        {
            logger.LogError("Error getting something fun to say: {Error}", ex);
        }

        return [];
    }

    public void Dispose() => httpClient?.Dispose();
}
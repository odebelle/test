using Elastic.Clients.Elasticsearch;
using Elastic.Transport;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Serilog.Sinks.Elasticsearch;
using Shared.Models;

namespace Shared.Clients;

public class DeadLetterClient : IDeadLetterClient
{
    private readonly ILogger<DeadLetterClient> _logger;

    private readonly ElasticsearchClient _storeClient;

    public event EventHandler? OnDeadMessageStore;

    public DeadLetterClient(ILogger<DeadLetterClient> logger, IOptions<ElasticsearchSinkOptions> elasticOptions, IConfiguration configuration)
    {
        
        var uris = configuration.GetValue<string>("Elastic:Uris").Split(',').Select(u =>
        {
            var b = new UriBuilder(new Uri(u));
            return b.Uri;
        });
        
        var usr = configuration.GetValue<string>("Elastic:usr");
        var pwd = configuration.GetValue<string>("Elastic:pwd");

        
        _logger = logger;
        var options = elasticOptions.Value;
        NodePool nodePool = new StickyNodePool(uris);
        
        var settings = new ElasticsearchClientSettings(nodePool)
            // .CertificateFingerprint("<FINGERPRINT>")
            .DefaultIndex($"sol-bro-dead-letter")
            .Authentication(new BasicAuthentication(usr, pwd));
        
        _storeClient = new ElasticsearchClient(settings);
    }

    public async Task<bool> StoreDeadLetterAsync(object? messageHolder)
    {
        try
        {
            if (messageHolder is null)
                throw new NullReferenceException("Unable to store dead letter. MessageHolder cannot be null.");

            var response = await _storeClient.IndexAsync(messageHolder);


            if (!response.IsValidResponse)
            {
                throw response.ApiCallDetails.OriginalException;
            }

            _logger.LogInformation("Dead letter stored on '{@Index}' with response: {@Result}", response.Index,
                response.Result);
        }
        catch (Exception exception)
        {
            _logger.LogCritical(exception, "{@Message}", exception.Message);
            return false;
        }

        return true;
    }

    public Task<IEnumerable<string>> GetAvailableTopicsAsync()
    {
        return Task.FromResult(Enumerable.Range(0, 25).Select(i => "not implemented"));
    }

    public IEnumerable<MessageHolder> GetDeadLetters()
    {
        try
        {
            var stored = _storeClient.Search<MessageHolder>();
            if (stored.IsValidResponse)
            {
                if (stored.Documents.Count > 0)
                {
                    _logger.LogInformation("{@MessageHolder}", stored.Documents.FirstOrDefault());
                    return stored.Documents.OrderByDescending(o=>o.ErrorUtc);
                }

                _logger.LogInformation("Sequence contains no elements");
            }
            else
                _logger.LogWarning(null, "Elastic query response is not valid.");
        }
        catch (Exception exception)
        {
            _logger.LogCritical(exception, "{@Message}", exception.Message);
        }

        return Enumerable.Empty<MessageHolder>();
    }

    public IEnumerable<MessageHolder> GetDeadLetters(int? take, int? skip)
    {
        try
        {
            var stored = _storeClient.Search<MessageHolder>(s => s
                .Size(take)
                .From(skip)
            );
            if (stored.IsValidResponse)
            {
                _logger.LogInformation("{@MessageHolder}", stored.Documents.FirstOrDefault());

                return stored.Documents.Count > 0 ? stored.Documents.OrderByDescending(o=>o.ErrorUtc) : Enumerable.Empty<MessageHolder>();
            }
        }
        catch (Exception exception)
        {
            _logger.LogCritical(exception, "{@Message}", exception.Message);
        }

        return Enumerable.Empty<MessageHolder>();
    }

    public async Task<IEnumerable<MessageHolder>?> GetDeadLetterAsync(int from, int size = 10,
        DateTime? fromDateTime = null)
    {
        try
        {
            var stored = await _storeClient.SearchAsync<MessageHolder>(s => s
                .From(from)
                .Size(size)
            );
            if (stored.IsValidResponse)
            {
                if (stored.Documents.Count > 0)
                    OnDeadMessageStore?.Invoke(this, EventArgs.Empty);
                return stored.Documents.OrderByDescending(o=>o.ErrorUtc);
            }
        }
        catch (Exception exception)
        {
            _logger.LogCritical(exception, "{@Message}", exception.Message);
        }

        return null;
    }
}
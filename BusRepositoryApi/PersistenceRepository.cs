using Microsoft.EntityFrameworkCore;
using Persistence;

namespace BusRepositoryApi;

public class PersistenceRepository(BusRemoteOperatorContext context, ILogger<PersistenceRepository> logger)
    : IPersistenceRepository
{
    private readonly IServiceCollection? _services;
    private readonly ILogger<PersistenceRepository>? _logger = logger;
    private BusRemoteOperatorContext? _context = context;

    public IEnumerable<Dispatch> GetDispatches()
    {
        var result = _context?.Dispatch
            .Include(i => i.Consumer)
            .Include(i => i.Producer);

        return result ?? Enumerable.Empty<Dispatch>();
    }

    public Task<IQueryable<Producer>> GetProducers(CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Gets defined consumers
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public async Task<IList<Consumer>> GetConsumersAsync(CancellationToken cancellationToken)
    {
        var result=  await _context?.Consumer.ToListAsync(cancellationToken)!;
        return result;
    }

    public Dispatch? GetDispatch(string dispatchName)
    {
        var result = _context?.Dispatch
            .Include(i => i.Consumer)
            .Include(i => i.Producer)
            .FirstOrDefault(f => f.Name == dispatchName);

        return result;
    }
}
using Persistence;

namespace BusRepositoryApi;

public interface IPersistenceRepository
{
    Dispatch? GetDispatch(string dispatchName);
    IEnumerable<Dispatch> GetDispatches();
    Task<IQueryable<Producer>> GetProducers(CancellationToken cancellationToken);
    Task<IList<Consumer>> GetConsumersAsync(CancellationToken cancellationToken);
}
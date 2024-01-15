using Shared.Models;

namespace Shared.Clients;

public interface IDeadLetterClient
{
    public event EventHandler? OnDeadMessageStore;
    
    Task<IEnumerable<MessageHolder>?> GetDeadLetterAsync(int from, int size = 10, DateTime? fromDateTime = null);
    Task<bool> StoreDeadLetterAsync(object? messageHolder);
    Task<IEnumerable<string>> GetAvailableTopicsAsync();
    IEnumerable<MessageHolder> GetDeadLetters();
    IEnumerable<MessageHolder> GetDeadLetters(int? take, int? skip);
}
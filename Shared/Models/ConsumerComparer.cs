namespace Shared.Models;

public class ConsumerComparer : IEqualityComparer<IConsumerElement?>
{
    public bool Equals(IConsumerElement? x, IConsumerElement? y)
    {
        if (x == null || y == null) return false;
        return x.Id.Equals(y.Id);
    }

    public int GetHashCode(IConsumerElement obj)
    {
        return HashCode.Combine(obj.Id);
    }
}
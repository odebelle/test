namespace Shared.Models;

public class ProducerComparer : IEqualityComparer<IProducerElement?>
{
    public bool Equals(IProducerElement? x, IProducerElement? y)
    {
        if (x == null || y == null) return false;
        return x.Id.Equals(y.Id);
    }

    public int GetHashCode(IProducerElement obj)
    {
        return HashCode.Combine(obj.Id);
    }
}
namespace Shared.Models;

public static class EnumerableExtensions
{
    public static void ForEach<T>(this IEnumerable<T> me, Action<T> func)
    {
        foreach (var item in me)
        {
            func(item);
        }
    }
}
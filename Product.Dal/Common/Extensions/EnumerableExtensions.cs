namespace Product.Dal.Common.Extensions;

public static class EnumerableExtensions
{
    public static IEnumerable<T> SelectManyRecursive<T>(this IEnumerable<T> source, Func<T, IEnumerable<T>> selector)
    {
        var result = source.Where(x => selector(x) != null).SelectMany(selector);
        if (!result.Any())
        {
            return result;
        }
        return result.Concat(result.SelectManyRecursive(selector));
    }
}

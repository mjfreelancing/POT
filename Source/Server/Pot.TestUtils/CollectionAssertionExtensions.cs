using Shouldly;

namespace Pot.TestUtils;

public static class CollectionAssertionExtensions
{
    public static void ShouldAllSatisfy<T>(this IEnumerable<T> source, Action<T> assertion)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(assertion);

        foreach (var item in source)
        {
            assertion(item);
        }
    }

    public static T ShouldContainSingle<T>(this IEnumerable<T> source, Func<T, bool> predicate, string? customMessage = null)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(predicate);

        var matchingItems = source.Where(predicate).ToList();

        matchingItems.Count.ShouldBe(1, customMessage);

        return matchingItems[0];
    }

    public static void ShouldHaveValues<TSource, TValue>(this IEnumerable<TSource> source, Func<TSource, TValue> selector,
        IEnumerable<TValue> expected, bool ignoreOrder = true, string? customMessage = null)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(selector);
        ArgumentNullException.ThrowIfNull(expected);

        source.Select(selector).ShouldBe(expected, ignoreOrder: ignoreOrder, customMessage: customMessage);
    }

    public static void ShouldAllMatch<T>(this IEnumerable<T> source, Func<T, bool> predicate, string? customMessage = null)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(predicate);

        source.All(predicate).ShouldBeTrue(customMessage);
    }
}
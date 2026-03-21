using AllOverIt.Assertion;
using Shouldly;

namespace Pot.TestUtils;

public static class CollectionAssertionExtensions
{
    public static void ShouldAllSatisfy<T>(this IEnumerable<T> source, Action<T> assertion)
    {
        _ = source.WhenNotNull();
        _ = assertion.WhenNotNull();

        foreach (var item in source)
        {
            assertion(item);
        }
    }

    public static T ShouldContainSingle<T>(this IEnumerable<T> source, Func<T, bool> predicate, string? customMessage = null)
    {
        _ = source.WhenNotNull();
        _ = predicate.WhenNotNull();

        var matchingItems = source.Where(predicate).ToList();

        matchingItems.Count.ShouldBe(1, customMessage);

        return matchingItems[0];
    }

    public static void ShouldHaveValues<TSource, TValue>(this IEnumerable<TSource> source, Func<TSource, TValue> selector,
        IEnumerable<TValue> expected, bool ignoreOrder = true, string? customMessage = null)
    {
        _ = source.WhenNotNull();
        _ = selector.WhenNotNull();
        _ = expected.WhenNotNull();

        source.Select(selector).ShouldBe(expected, ignoreOrder: ignoreOrder, customMessage: customMessage);
    }

    public static void ShouldAllMatch<T>(this IEnumerable<T> source, Func<T, bool> predicate, string? customMessage = null)
    {
        _ = source.WhenNotNull();
        _ = predicate.WhenNotNull();

        source.All(predicate).ShouldBeTrue(customMessage);
    }

    public static void ShouldContainValue(this IEnumerable<string> source, string expectedSubstring,
        StringComparison comparison = StringComparison.Ordinal, string? customMessage = null)
    {
        _ = source.WhenNotNull();
        ArgumentException.ThrowIfNullOrWhiteSpace(expectedSubstring);

        source.Any(value => value.Contains(expectedSubstring, comparison)).ShouldBeTrue(customMessage);
    }
}
using AllOverIt.Assertion;

namespace Pot.AspNetCore.Integration.Tests.Pipeline.Support;

/// <summary>
/// Decides whether every documented operation has had its coverage decided: exercised by the matrix, or left alone
/// with a reason.
/// </summary>
/// <remarks>
/// The checks are pure, so the rules can be shown to work on data the application does not currently contain — a
/// documented operation nothing covers, or an ignore that states no reason — rather than only on the contract's
/// current contents.
/// </remarks>
public static class InventoryCoverage
{
    /// <summary>
    /// Finds every way the coverage of the documented operations is undecided or inconsistent.
    /// </summary>
    /// <param name="documented">The operations the contract documents.</param>
    /// <param name="exercised">The operations the matrix generates at least one case for.</param>
    /// <param name="ignored">The operations left alone, each with its reason.</param>
    /// <returns>One problem per operation, ordered so a run reports them the same way every time.</returns>
    public static IReadOnlyList<string> FindProblems(IEnumerable<EndpointOperation> documented,
        IEnumerable<EndpointOperation> exercised, IEnumerable<IgnoredOperation> ignored)
    {
        _ = documented.WhenNotNull();
        _ = exercised.WhenNotNull();
        _ = ignored.WhenNotNull();

        var documentedSet = documented.ToHashSet();
        var exercisedSet = exercised.ToHashSet();
        var ignoredList = ignored.ToList();
        var ignoredSet = ignoredList.Select(ignoredOperation => ignoredOperation.Operation).ToHashSet();

        var problems = new List<string>();

        foreach (var operation in documentedSet)
        {
            if (!exercisedSet.Contains(operation) && !ignoredSet.Contains(operation))
            {
                problems.Add(
                    $"{operation} is documented but neither exercised nor ignored - decide its coverage, by generating a case or by stating why it needs none");
            }
        }

        foreach (var operation in exercisedSet.Where(operation => !documentedSet.Contains(operation)))
        {
            problems.Add($"{operation} is exercised but is not a documented operation - the case is stale and must be removed");
        }

        foreach (var ignoredOperation in ignoredList.Where(ignoredOperation => string.IsNullOrWhiteSpace(ignoredOperation.Reason)))
        {
            problems.Add($"{ignoredOperation.Operation} is ignored without a stated reason - a reader cannot tell whether it still holds");
        }

        foreach (var operation in ignoredSet.Where(operation => !documentedSet.Contains(operation)))
        {
            problems.Add($"{operation} is ignored but is not a documented operation - the entry is stale and must be removed");
        }

        foreach (var operation in ignoredList
            .GroupBy(ignoredOperation => ignoredOperation.Operation)
            .Where(group => group.Count() > 1)
            .Select(group => group.Key))
        {
            problems.Add($"{operation} is ignored more than once - keep one entry so its reason is unambiguous");
        }

        return [.. problems.OrderBy(problem => problem, StringComparer.Ordinal)];
    }
}

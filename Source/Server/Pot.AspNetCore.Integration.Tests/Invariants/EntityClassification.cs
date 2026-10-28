using AllOverIt.Assertion;

namespace Pot.AspNetCore.Integration.Tests.Invariants;

/// <summary>
/// Decides whether every entity the data model declares has had its tenant scope decided: reachable from a site, or
/// declared shared with a reason.
/// </summary>
/// <remarks>
/// The checks are pure, so the failure modes are shown on data the model does not currently contain — a table that
/// reaches no site and nobody declared, or a shared declaration that a foreign key has since contradicted.
/// </remarks>
public static class EntityClassification
{
    /// <summary>
    /// Finds every way the classification of the declared entities is undecided or inconsistent.
    /// </summary>
    /// <param name="scopes">What the model declares.</param>
    /// <param name="globals">The entities declared shared, each with its reason.</param>
    /// <returns>One problem per entity, ordered so a run reports them the same way every time.</returns>
    public static IReadOnlyList<string> FindProblems(TenantScopes scopes, IEnumerable<GlobalEntity> globals)
    {
        _ = scopes.WhenNotNull();
        _ = globals.WhenNotNull();

        var entities = scopes.Entities.ToHashSet();
        var tenantScoped = scopes.TenantScopedEntities.ToHashSet();
        var declared = globals.ToList();
        var declaredEntities = declared.Select(global => global.Entity).ToHashSet();

        var problems = new List<string>();

        foreach (var entity in entities.Where(entity => !tenantScoped.Contains(entity) && !declaredEntities.Contains(entity)))
        {
            problems.Add(
                $"{entity.Name} is declared by the model but neither reaches a site nor is declared shared - classify it by giving it a site, or by stating why it belongs to none");
        }

        foreach (var global in declared.Where(global => string.IsNullOrWhiteSpace(global.Reason)))
        {
            problems.Add($"{global.Entity.Name} is declared shared without a stated reason - a reader cannot tell whether it still holds");
        }

        foreach (var global in declared.Where(global => !entities.Contains(global.Entity)))
        {
            problems.Add($"{global.Entity.Name} is declared shared but the model no longer declares it - the entry is stale and must be removed");
        }

        foreach (var global in declared.Where(global => tenantScoped.Contains(global.Entity)))
        {
            problems.Add(
                $"{global.Entity.Name} is declared shared but now reaches a site - it is tenant-scoped, so the entry must be removed");
        }

        foreach (var entity in declared
            .GroupBy(global => global.Entity)
            .Where(group => group.Count() > 1)
            .Select(group => group.Key))
        {
            problems.Add($"{entity.Name} is declared shared more than once - keep one entry so its reason is unambiguous");
        }

        return [.. problems.OrderBy(problem => problem, StringComparer.Ordinal)];
    }
}

namespace Pot.AspNetCore.Integration.Tests.Invariants.Support;

/// <summary>
/// What the data model declares: every entity, the ones that belong to a tenant, and the query that reads each
/// tenant-scoped table for one site.
/// </summary>
/// <param name="Entities">Every entity the model declares.</param>
/// <param name="TenantScopedEntities">The entities that can be reached from a site.</param>
/// <param name="Tables">The tenant-scoped tables, each with its single-tenant query.</param>
public sealed record TenantScopes(IReadOnlyList<Type> Entities, IReadOnlyList<Type> TenantScopedEntities,
    IReadOnlyList<TenantScopedTable> Tables);

using AllOverIt.Assertion;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Pot.Data.Entities;
using System.Text;

namespace Pot.AspNetCore.Integration.Tests.Invariants.Support;

/// <summary>
/// Works out how each declared entity belongs to a site, so a fingerprint reads every tenant-scoped table without a
/// hand-written list of tables.
/// </summary>
/// <remarks>
/// Belonging is decided by reachability: an entity that holds, or reaches through its foreign keys, the site's key is
/// tenant-scoped. An entity that reaches no site is reported as unreachable, and the classification is what turns that
/// into a failure until someone says why the table is shared.
/// </remarks>
public static class TenantScopeResolver
{
    private const int MaximumHops = 3;
    private const string SiteParameter = "@siteId";

    /// <summary>
    /// Resolves the tenant scope of every entity the model declares.
    /// </summary>
    /// <param name="model">The model to resolve.</param>
    /// <returns>Every entity, the tenant-scoped ones, and the query that reads each tenant-scoped table.</returns>
    public static TenantScopes Resolve(IModel model)
    {
        _ = model.WhenNotNull();

        var entityTypes = model.GetEntityTypes()
            .Where(entityType => entityType.GetTableName() is not null)
            .Where(entityType => !entityType.IsOwned())
            .ToList();

        var tables = new List<TenantScopedTable>();
        var tenantScopedEntities = new List<Type>();

        foreach (var entityType in entityTypes)
        {
            var table = BuildTable(entityType);

            if (table is null)
            {
                continue;
            }

            tables.Add(table);
            tenantScopedEntities.Add(entityType.ClrType);
        }

        var entities = entityTypes.Select(entityType => entityType.ClrType).ToList();

        return new TenantScopes(entities, tenantScopedEntities, tables);
    }

    private static TenantScopedTable? BuildTable(IEntityType entityType)
    {
        var tableName = entityType.GetTableName()!;

        if (entityType.ClrType == typeof(SiteEntity))
        {
            // The tenant's own row is scoped by the key the other tables point at.
            var siteKeyColumns = KeyColumnNames(entityType);
            var siteKeyMatch = string.Join(" AND ", siteKeyColumns.Select(column => $"c.{Quote(column)} = {SiteParameter}"));

            return new TenantScopedTable(tableName, $"SELECT c.* FROM {Quote(tableName)} c WHERE {siteKeyMatch}", siteKeyColumns);
        }

        var path = FindPathToSite(entityType);

        return path is null ? null : BuildJoinQuery(entityType, path);
    }

    private static IReadOnlyList<IForeignKey>? FindPathToSite(IEntityType startingEntity)
    {
        var pending = new Queue<(IEntityType Entity, List<IForeignKey> Path)>();
        pending.Enqueue((startingEntity, []));

        while (pending.Count > 0)
        {
            var (entity, path) = pending.Dequeue();

            if (path.Count >= MaximumHops)
            {
                continue;
            }

            foreach (var foreignKey in entity.GetForeignKeys())
            {
                var extended = new List<IForeignKey>(path) { foreignKey };

                if (foreignKey.PrincipalEntityType.ClrType == typeof(SiteEntity))
                {
                    return extended;
                }

                pending.Enqueue((foreignKey.PrincipalEntityType, extended));
            }
        }

        return null;
    }

    private static TenantScopedTable BuildJoinQuery(IEntityType entityType, IReadOnlyList<IForeignKey> path)
    {
        var tableName = entityType.GetTableName()!;
        var query = new StringBuilder($"SELECT c.* FROM {Quote(tableName)} c");

        var previousAlias = "c";
        var previousEntity = entityType;

        for (var index = 0; index < path.Count; index++)
        {
            var foreignKey = path[index];
            var principalEntity = foreignKey.PrincipalEntityType;
            var alias = $"j{index}";

            var matches = foreignKey.Properties
                .Select((property, position) =>
                    $"{previousAlias}.{Quote(ColumnName(property, previousEntity))} = {alias}.{Quote(ColumnName(foreignKey.PrincipalKey.Properties[position], principalEntity))}");

            query.Append($" INNER JOIN {Quote(principalEntity.GetTableName()!)} {alias} ON {string.Join(" AND ", matches)}");

            previousAlias = alias;
            previousEntity = principalEntity;
        }

        var siteKeyColumns = KeyColumnNames(previousEntity);
        var siteKeyMatch = string.Join(" AND ", siteKeyColumns.Select(column => $"{previousAlias}.{Quote(column)} = {SiteParameter}"));

        query.Append($" WHERE {siteKeyMatch}");

        return new TenantScopedTable(tableName, query.ToString(), KeyColumnNames(entityType));
    }

    private static IReadOnlyList<string> KeyColumnNames(IEntityType entityType)
    {
        var primaryKey = entityType.FindPrimaryKey();

        return primaryKey is null
            ? []
            : [.. primaryKey.Properties.Select(property => ColumnName(property, entityType))];
    }

    private static string ColumnName(IProperty property, IEntityType declaringEntity)
    {
        var storeObject = StoreObjectIdentifier.Table(declaringEntity.GetTableName()!, declaringEntity.GetSchema());

        return property.GetColumnName(storeObject) ?? property.Name;
    }

    private static string Quote(string identifier)
    {
        return $"\"{identifier}\"";
    }
}

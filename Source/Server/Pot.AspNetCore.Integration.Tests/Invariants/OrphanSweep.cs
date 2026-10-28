using AllOverIt.Assertion;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Pot.Data;
using System.Data.Common;
using System.Text;

namespace Pot.AspNetCore.Integration.Tests.Invariants;

/// <summary>
/// Looks for rows that still point at a row that no longer exists, across every foreign key the model declares.
/// </summary>
/// <remarks>
/// The sweep is built from the model's foreign keys, so a table added later is covered without anyone listing it. A
/// reference that survives its principal is the signature of a deletion that removed a record but not everything that
/// depended on it, whether or not the deletion reported success.
/// </remarks>
public static class OrphanSweep
{
    /// <summary>
    /// Finds every dangling reference in the database.
    /// </summary>
    /// <param name="dbContext">The context whose model decides the foreign keys and whose connection reads them.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>One description per foreign key that has a dangling reference, empty when the store is consistent.</returns>
    public static async Task<IReadOnlyList<string>> FindAsync(PotDbContext dbContext, CancellationToken cancellationToken)
    {
        _ = dbContext.WhenNotNull();

        var problems = new List<string>();

        foreach (var foreignKey in dbContext.Model.GetEntityTypes().SelectMany(entityType => entityType.GetForeignKeys()))
        {
            var orphan = await FindFirstOrphanAsync(dbContext, foreignKey, cancellationToken);

            if (orphan is not null)
            {
                problems.Add(orphan);
            }
        }

        return problems;
    }

    private static async Task<string?> FindFirstOrphanAsync(PotDbContext dbContext, IForeignKey foreignKey,
        CancellationToken cancellationToken)
    {
        var dependent = foreignKey.DeclaringEntityType;
        var principal = foreignKey.PrincipalEntityType;

        if (dependent.GetTableName() is null || principal.GetTableName() is null)
        {
            return null;
        }

        var matches = foreignKey.Properties.Select((property, position) =>
            $"d.{Quote(ColumnName(property, dependent))} = p.{Quote(ColumnName(foreignKey.PrincipalKey.Properties[position], principal))}");

        var notNull = foreignKey.Properties.Select(property => $"d.{Quote(ColumnName(property, dependent))} IS NOT NULL");

        var query = new StringBuilder()
            .Append($"SELECT d.* FROM {Quote(dependent.GetTableName()!)} d")
            .Append($" LEFT JOIN {Quote(principal.GetTableName()!)} p ON {string.Join(" AND ", matches)}")
            .Append($" WHERE {string.Join(" AND ", notNull)} AND p.{Quote(ColumnName(foreignKey.PrincipalKey.Properties[0], principal))} IS NULL")
            .ToString();

        var row = await ReadFirstRowAsync(dbContext, query, cancellationToken);

        if (row is null)
        {
            return null;
        }

        var keyColumns = dependent.FindPrimaryKey()?.Properties.Select(property => ColumnName(property, dependent)).ToList();

        var rowKey = keyColumns is null
            ? "?"
            : string.Join("|", keyColumns.Select(column => row.TryGetValue(column, out var value) ? value : "?"));

        return $"table '{dependent.GetTableName()}' row {rowKey} references a {principal.GetTableName()} that no longer exists";
    }

    private static async Task<Dictionary<string, string>?> ReadFirstRowAsync(PotDbContext dbContext, string query,
        CancellationToken cancellationToken)
    {
        var connection = dbContext.Database.GetDbConnection();

        if (connection.State != System.Data.ConnectionState.Open)
        {
            await connection.OpenAsync(cancellationToken);
        }

        await using var command = connection.CreateCommand();
        command.CommandText = query;

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        if (!await reader.ReadAsync(cancellationToken))
        {
            return null;
        }

        return ReadRow(reader);
    }

    private static Dictionary<string, string> ReadRow(DbDataReader reader)
    {
        var row = new Dictionary<string, string>(StringComparer.Ordinal);

        for (var ordinal = 0; ordinal < reader.FieldCount; ordinal++)
        {
            row[reader.GetName(ordinal)] = reader.IsDBNull(ordinal)
                ? "(null)"
                : Convert.ToString(reader.GetValue(ordinal), System.Globalization.CultureInfo.InvariantCulture) ?? "(null)";
        }

        return row;
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

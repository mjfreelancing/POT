using AllOverIt.Assertion;
using Microsoft.EntityFrameworkCore;
using Pot.Data;
using Pot.Data.Entities;
using System.Data.Common;
using System.Globalization;

namespace Pot.AspNetCore.Integration.Tests.Invariants.Support;

/// <summary>
/// Reads a tenant's rows out of every tenant-scoped table, so an invariant compares what an action actually touched
/// rather than only what a route reports.
/// </summary>
/// <remarks>
/// The queries come from the model rather than from a list of tables, so a table added later is read without anyone
/// remembering it. Values are read as text so the comparison is exact and a difference can be shown as it was stored.
/// </remarks>
public static class TenantSnapshotter
{
    /// <summary>
    /// Captures one tenant's rows.
    /// </summary>
    /// <param name="dbContext">The context whose model decides the tables and whose connection reads them.</param>
    /// <param name="siteRowId">The tenant to capture.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The tenant's rows, table by table and row by row.</returns>
    public static async Task<TenantFingerprint> CaptureAsync(PotDbContext dbContext, Guid siteRowId,
        CancellationToken cancellationToken)
    {
        _ = dbContext.WhenNotNull();

        var scopes = TenantScopeResolver.Resolve(dbContext.Model);
        var siteId = await FindSiteIdAsync(dbContext, siteRowId, cancellationToken);

        var tables = new Dictionary<string, IReadOnlyDictionary<string, IReadOnlyDictionary<string, string>>>(StringComparer.Ordinal);

        foreach (var table in scopes.Tables)
        {
            // A tenant that no longer exists is read as holding nothing, so its removal shows up as every row going.
            tables[table.Table] = siteId is null
                ? new Dictionary<string, IReadOnlyDictionary<string, string>>(StringComparer.Ordinal)
                : await ReadTableAsync(dbContext, table, siteId.Value, cancellationToken);
        }

        return new TenantFingerprint(siteRowId, tables);
    }

    private static async Task<int?> FindSiteIdAsync(PotDbContext dbContext, Guid siteRowId, CancellationToken cancellationToken)
    {
        var site = await dbContext.Set<SiteEntity>()
            .AsNoTracking()
            .SingleOrDefaultAsync(entity => entity.RowId == siteRowId, cancellationToken);

        return site?.Id;
    }

    private static async Task<IReadOnlyDictionary<string, IReadOnlyDictionary<string, string>>> ReadTableAsync(
        PotDbContext dbContext, TenantScopedTable table, int siteId, CancellationToken cancellationToken)
    {
        var connection = dbContext.Database.GetDbConnection();

        if (connection.State != System.Data.ConnectionState.Open)
        {
            await connection.OpenAsync(cancellationToken);
        }

        await using var command = connection.CreateCommand();
        command.CommandText = table.Query;

        var parameter = command.CreateParameter();
        parameter.ParameterName = "@siteId";
        parameter.Value = siteId;
        command.Parameters.Add(parameter);

        var rows = new Dictionary<string, IReadOnlyDictionary<string, string>>(StringComparer.Ordinal);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            var columns = new Dictionary<string, string>(StringComparer.Ordinal);

            for (var ordinal = 0; ordinal < reader.FieldCount; ordinal++)
            {
                columns[reader.GetName(ordinal)] = ReadValue(reader, ordinal);
            }

            var rowKey = string.Join("|", table.KeyColumns.Select(column => columns.TryGetValue(column, out var value) ? value : "?"));
            rows[rowKey] = columns;
        }

        return rows;
    }

    private static string ReadValue(DbDataReader reader, int ordinal)
    {
        if (reader.IsDBNull(ordinal))
        {
            return "(null)";
        }

        var value = reader.GetValue(ordinal);

        return value switch
        {
            DateTime dateTime => dateTime.ToString("O", CultureInfo.InvariantCulture),
            DateOnly dateOnly => dateOnly.ToString("O", CultureInfo.InvariantCulture),
            IFormattable formattable => formattable.ToString(null, CultureInfo.InvariantCulture),
            _ => value.ToString() ?? "(unknown)"
        };
    }
}

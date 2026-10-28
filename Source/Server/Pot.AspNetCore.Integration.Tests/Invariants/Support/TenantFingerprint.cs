using AllOverIt.Assertion;

namespace Pot.AspNetCore.Integration.Tests.Invariants.Support;

/// <summary>
/// What one tenant's rows held at a moment in time, table by table and row by row, so a difference can name the row
/// and the column that moved.
/// </summary>
/// <param name="SiteRowId">The tenant the snapshot was taken for.</param>
/// <param name="Tables">The rows of each tenant-scoped table, keyed by table and then by row.</param>
public sealed record TenantFingerprint(Guid SiteRowId,
    IReadOnlyDictionary<string, IReadOnlyDictionary<string, IReadOnlyDictionary<string, string>>> Tables)
{
    /// <summary>
    /// One way a snapshot differs from another.
    /// </summary>
    /// <param name="Table">The table the difference was found in.</param>
    /// <param name="RowKey">The row that changed, or an empty string when the whole table did.</param>
    /// <param name="WasRemoved">Whether the row was present earlier and absent later.</param>
    /// <param name="Description">The difference as a reviewer should read it.</param>
    public sealed record RowChange(string Table, string RowKey, bool WasRemoved, string Description)
    {
        /// <summary>
        /// Identifies the row so a test can decide whether its disappearance was expected.
        /// </summary>
        /// <returns>The table and the row key, joined so a set can hold it.</returns>
        public string Identify()
        {
            return $"{Table}|{RowKey}";
        }
    }

    /// <summary>
    /// Finds the first way this snapshot differs from another, describing it in terms of the tenant, the table, the
    /// row and the value that moved.
    /// </summary>
    /// <param name="other">The snapshot taken later.</param>
    /// <returns>The first difference, or <see langword="null" /> when the tenant is unchanged.</returns>
    public string? FindFirstDifference(TenantFingerprint other)
    {
        var changes = FindChanges(other);

        return changes.Count == 0 ? null : changes[0].Description;
    }

    /// <summary>
    /// Finds every way this snapshot differs from another.
    /// </summary>
    /// <param name="other">The snapshot taken later.</param>
    /// <returns>The differences, ordered by table and row so a run reports them the same way every time.</returns>
    public IReadOnlyList<RowChange> FindChanges(TenantFingerprint other)
    {
        _ = other.WhenNotNull();

        var changes = new List<RowChange>();

        foreach (var table in Tables.OrderBy(table => table.Key, StringComparer.Ordinal))
        {
            if (!other.Tables.TryGetValue(table.Key, out var laterRows))
            {
                changes.Add(new RowChange(table.Key, string.Empty, false,
                    $"tenant {SiteRowId} table '{table.Key}': the table is not in the later snapshot"));

                continue;
            }

            foreach (var row in table.Value.OrderBy(row => row.Key, StringComparer.Ordinal))
            {
                if (!laterRows.TryGetValue(row.Key, out var laterColumns))
                {
                    changes.Add(new RowChange(table.Key, row.Key, true,
                        $"tenant {SiteRowId} table '{table.Key}': row {row.Key} is gone"));

                    continue;
                }

                var column = FindFirstColumnDifference(row.Value, laterColumns);

                if (column is not null)
                {
                    changes.Add(new RowChange(table.Key, row.Key, false,
                        $"tenant {SiteRowId} table '{table.Key}': row {row.Key}, column '{column.Value.Column}' was '{column.Value.Earlier}' but is now '{column.Value.Later}'"));
                }
            }

            foreach (var rowKey in laterRows.Keys
                .Where(rowKey => !table.Value.ContainsKey(rowKey))
                .OrderBy(rowKey => rowKey, StringComparer.Ordinal))
            {
                changes.Add(new RowChange(table.Key, rowKey, false,
                    $"tenant {SiteRowId} table '{table.Key}': row {rowKey} appeared"));
            }
        }

        foreach (var table in other.Tables.Keys
            .Where(table => !Tables.ContainsKey(table))
            .OrderBy(table => table, StringComparer.Ordinal))
        {
            changes.Add(new RowChange(table, string.Empty, false,
                $"tenant {SiteRowId} table '{table}': the table appeared in the later snapshot"));
        }

        return changes;
    }

    /// <summary>
    /// Describes the snapshot's contents, so a difference can be judged against what was expected.
    /// </summary>
    /// <returns>A stable description of every row.</returns>
    public string Write()
    {
        var lines = Tables
            .OrderBy(table => table.Key, StringComparer.Ordinal)
            .SelectMany(table => table.Value
                .OrderBy(row => row.Key, StringComparer.Ordinal)
                .Select(row => $"{table.Key}|{row.Key}|{WriteColumns(row.Value)}"));

        return string.Join(Environment.NewLine, lines);
    }

    private static string WriteColumns(IReadOnlyDictionary<string, string> columns)
    {
        return string.Join(",",
            columns.OrderBy(column => column.Key, StringComparer.Ordinal).Select(column => $"{column.Key}={column.Value}"));
    }

    private static (string Column, string Earlier, string Later)? FindFirstColumnDifference(
        IReadOnlyDictionary<string, string> earlier, IReadOnlyDictionary<string, string> later)
    {
        foreach (var column in earlier.OrderBy(column => column.Key, StringComparer.Ordinal))
        {
            if (!later.TryGetValue(column.Key, out var laterValue))
            {
                return (column.Key, column.Value, "(column absent)");
            }

            if (!string.Equals(column.Value, laterValue, StringComparison.Ordinal))
            {
                return (column.Key, column.Value, laterValue);
            }
        }

        foreach (var column in later.Keys.Where(column => !earlier.ContainsKey(column)).OrderBy(column => column, StringComparer.Ordinal))
        {
            return (column, "(column absent)", later[column]);
        }

        return null;
    }
}

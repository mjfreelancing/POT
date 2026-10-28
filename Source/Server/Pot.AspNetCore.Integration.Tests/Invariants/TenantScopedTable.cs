namespace Pot.AspNetCore.Integration.Tests.Invariants;

/// <summary>
/// One table the fingerprint reads, with the query that restricts it to a single tenant.
/// </summary>
/// <param name="Table">The table name, so a difference names the table a reviewer would look in.</param>
/// <param name="Query">The query, whose only parameter is the site's internal identifier.</param>
/// <param name="KeyColumns">
/// The columns that identify a row within the table, so a difference can name one row rather than a count.
/// </param>
public sealed record TenantScopedTable(string Table, string Query, IReadOnlyList<string> KeyColumns);

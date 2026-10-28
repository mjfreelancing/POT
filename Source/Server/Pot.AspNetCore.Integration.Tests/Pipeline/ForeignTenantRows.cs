using AllOverIt.Assertion;

namespace Pot.AspNetCore.Integration.Tests.Pipeline;

/// <summary>
/// The rows another tenant owns, so a case can name one and prove the caller is answered as though it did not exist.
/// </summary>
/// <param name="SiteRowId">The other tenant's site.</param>
/// <param name="UserRowId">A user in the other tenant.</param>
/// <param name="AccountRowId">An account in the other tenant.</param>
/// <param name="ExpenseRowId">An expense in the other tenant.</param>
/// <param name="IncomeRowId">An income in the other tenant.</param>
public sealed record ForeignTenantRows(Guid SiteRowId, Guid UserRowId, Guid AccountRowId, Guid ExpenseRowId, Guid IncomeRowId)
{
    private const string RowParameter = "{id}";

    /// <summary>
    /// Resolves the row an operation names from the resource its path addresses.
    /// </summary>
    /// <param name="operation">The operation to resolve a row for.</param>
    /// <returns>The other tenant's row, or <see langword="null" /> when the resource is not classified.</returns>
    public Guid? FindRowId(EndpointOperation operation)
    {
        _ = operation.WhenNotNull();

        return ResourceBeforeRowParameter(operation.Path)?.ToLowerInvariant() switch
        {
            "accounts" => AccountRowId,
            "expenses" => ExpenseRowId,
            "incomes" => IncomeRowId,
            "users" => UserRowId,
            "sites" => SiteRowId,

            // An approval names the user awaiting the decision, so a foreign approval is a foreign user.
            "approvals" => UserRowId,
            _ => null
        };
    }

    /// <summary>
    /// Finds operations that name a row whose resource has no row declared, so a new resource fails rather than
    /// quietly losing its case.
    /// </summary>
    /// <param name="documented">The operations the contract documents.</param>
    /// <returns>One problem per unclassified operation.</returns>
    public IReadOnlyList<string> FindUnclassifiedOperations(IEnumerable<EndpointOperation> documented)
    {
        _ = documented.WhenNotNull();

        return
        [
            .. documented
                .Where(operation => operation.Path.Contains(RowParameter, StringComparison.Ordinal))
                .Where(operation => FindRowId(operation) is null)
                .Select(operation =>
                    $"{operation} names a row but no row of that resource is declared for another tenant - classify the resource so the case can run")
                .OrderBy(problem => problem, StringComparer.Ordinal)
        ];
    }

    private static string? ResourceBeforeRowParameter(string path)
    {
        var segments = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
        var rowIndex = Array.FindIndex(segments, segment => segment.Equals(RowParameter, StringComparison.Ordinal));

        return rowIndex > 0 ? segments[rowIndex - 1] : null;
    }
}

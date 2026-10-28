using AllOverIt.Assertion;

namespace Pot.AspNetCore.Integration.Tests.Pipeline;

/// <summary>
/// Generates the authorization cases for the documented operations, so an operation added later is exercised without
/// anyone listing it.
/// </summary>
/// <remarks>
/// Every operation is attempted anonymously and as each role. An operation whose path names an <c>id</c> is also
/// attempted with an identifier that belongs to another tenant, because that is the case a membership check would
/// get wrong.
/// </remarks>
public static class AuthorizationMatrix
{
    private const string RowParameter = "{id}";

    /// <summary>
    /// Generates a case per caller for every documented operation that is not ignored.
    /// </summary>
    /// <param name="documented">The operations the contract documents.</param>
    /// <param name="ignored">The operations left alone.</param>
    /// <returns>The cases, ordered so a run reports them the same way every time.</returns>
    public static IReadOnlyList<AuthorizationCase> Generate(IEnumerable<DocumentedOperation> documented,
        IEnumerable<IgnoredOperation> ignored)
    {
        _ = documented.WhenNotNull();
        _ = ignored.WhenNotNull();

        var ignoredSet = ignored.Select(ignoredOperation => ignoredOperation.Operation).ToHashSet();

        return
        [
            .. documented
                .Where(documentedOperation => !ignoredSet.Contains(documentedOperation.Operation))
                .SelectMany(GenerateFor)
                .OrderBy(authorizationCase => authorizationCase.Operation.Path, StringComparer.Ordinal)
                .ThenBy(authorizationCase => authorizationCase.Operation.Method, StringComparer.Ordinal)
                .ThenBy(authorizationCase => authorizationCase.Caller)
                .ThenBy(authorizationCase => authorizationCase.RowIsForeign)
        ];
    }

    /// <summary>
    /// The operations the matrix exercises.
    /// </summary>
    /// <param name="cases">The generated cases.</param>
    /// <returns>Distinct operations, so coverage can be compared with the inventory.</returns>
    public static IReadOnlyList<EndpointOperation> CoveredOperations(IEnumerable<AuthorizationCase> cases)
    {
        _ = cases.WhenNotNull();

        return [.. cases.Select(authorizationCase => authorizationCase.Operation).Distinct()];
    }

    private static IEnumerable<AuthorizationCase> GenerateFor(DocumentedOperation documentedOperation)
    {
        var operation = documentedOperation.Operation;
        var carriesRequestBody = documentedOperation.CarriesRequestBody;

        yield return new AuthorizationCase(operation, AuthorizationCaller.Anonymous, CarriesRequestBody: carriesRequestBody);
        yield return new AuthorizationCase(operation, AuthorizationCaller.Administrator, CarriesRequestBody: carriesRequestBody);
        yield return new AuthorizationCase(operation, AuthorizationCaller.Viewer, CarriesRequestBody: carriesRequestBody);

        if (operation.Path.Contains(RowParameter, StringComparison.Ordinal))
        {
            yield return new AuthorizationCase(operation, AuthorizationCaller.Administrator, RowIsForeign: true,
                carriesRequestBody);
        }
    }
}

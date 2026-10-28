namespace Pot.AspNetCore.Integration.Tests.Pipeline.Support;

/// <summary>
/// One authorization case: an operation, the caller that attempts it and whether the row it names belongs to
/// another tenant.
/// </summary>
/// <param name="Operation">The documented operation the case exercises.</param>
/// <param name="Caller">Who attempts the operation.</param>
/// <param name="RowIsForeign">
/// Whether a row the path names is owned by a different tenant. Ignored for a path that names no row.
/// </param>
/// <param name="CarriesRequestBody">Whether the operation documents a request body.</param>
public sealed record AuthorizationCase(EndpointOperation Operation, AuthorizationCaller Caller, bool RowIsForeign = false,
    bool CarriesRequestBody = false)
{
    /// <summary>
    /// The case as a failure message should name it.
    /// </summary>
    /// <returns>The operation, the caller, and whether the row belonged to another tenant.</returns>
    public override string ToString()
    {
        var caller = Caller.ToString();

        return RowIsForeign
            ? $"{Operation} as {caller} naming another tenant's row"
            : $"{Operation} as {caller}";
    }
}

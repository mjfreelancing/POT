namespace Pot.AspNetCore.Integration.Tests.Pipeline;

/// <summary>
/// The operations the authorization matrix does not exercise, each with the reason it is left alone.
/// </summary>
/// <remarks>
/// Every entry is a decision that has to survive review, so the list holds only operations a case cannot exercise
/// safely or meaningfully: one that ends the session, the tenant or the identity every other case in the run
/// depends on, or one whose answer is decided by something other than the caller's authorization. Everything else is
/// exercised, because the matrix is generated from the contract rather than written by hand.
/// </remarks>
public static class IgnoredOperations
{
    private static readonly IgnoredOperation[] Operations =
    [
        new(
            EndpointOperation.Create("POST", "/api/auth/logout"),
            "Ends the session every later case in the run signs in with, so this case would refuse all of them."),
        new(
            EndpointOperation.Create("POST", "/api/auth/refresh"),
            "Answered from the refresh-token cookie rather than the caller's authorization, so the endpoint's own declaration says nothing about what a bearer token should receive."),
        new(
            EndpointOperation.Create("DELETE", "/api/me"),
            "Removes the caller, and the matrix holds one caller per role for the whole run."),
        new(
            EndpointOperation.Create("DELETE", "/api/sites/current"),
            "Removes the tenant every case in the run acts in, and with it the rows the other cases read."),
        new(
            EndpointOperation.Create("POST", "/api/maintenance/import"),
            "Binds a multipart form, and a request ASP.NET cannot bind is answered 415 before the caller's authorization is tested - so the only body that reaches the decision imports data.")
    ];

    /// <summary>
    /// The operations left alone, each with its reason.
    /// </summary>
    public static IReadOnlyList<IgnoredOperation> All
    {
        get
        {
            return Operations;
        }
    }
}

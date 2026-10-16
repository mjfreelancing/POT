using AllOverIt.Assertion;
using Microsoft.AspNetCore.Http.HttpResults;
using Pot.App.Features.Accruals.Status.Models;

namespace Pot.AspNetCore.Features.Accruals.Status;

/// <summary>
/// Represents the accrual status returned by the API.
/// </summary>
internal sealed class Response
{
    /// <summary>
    /// The expense renewals required for the requested accounts.
    /// </summary>
    public required Guid[] ExpenseRenewalsRequired { get; init; }

    /// <summary>
    /// The income renewals required for the requested accounts.
    /// </summary>
    public required Guid[] IncomeRenewalsRequired { get; init; }

    /// <summary>
    /// Creates an HTTP 200 response from the accrual status output.
    /// </summary>
    /// <param name="output">The accrual status output.</param>
    /// <returns>The successful API response.</returns>
    public static Ok<Response> Ok(Output output)
    {
        _ = output.WhenNotNull();

        var response = new Response
        {
            ExpenseRenewalsRequired = output.ExpenseRenewalsRequired,
            IncomeRenewalsRequired = output.IncomeRenewalsRequired
        };

        return TypedResults.Ok(response);
    }
}

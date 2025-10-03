using AllOverIt.Assertion;
using Microsoft.AspNetCore.Http.HttpResults;
using Pot.App.Features.Projections.Models;

namespace Pot.AspNetCore.Features.Projections.Get;

internal sealed class Response
{
    public List<AccountDailyFinancialProjection> Accounts { get; init; }
    public List<DateProjection> Global { get; init; }

    public static Ok<Response> Ok(Output projections)
    {
        return TypedResults.Ok(new Response(projections));
    }

    private Response(Output projections)
    {
        _ = projections.WhenNotNull();

        Accounts = projections.Accounts;
        Global = projections.Global;
    }
}

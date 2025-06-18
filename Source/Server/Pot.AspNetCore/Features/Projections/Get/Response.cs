using AllOverIt.Assertion;
using Microsoft.AspNetCore.Http.HttpResults;
using Pot.App.Features.Projections.Models;
using Pot.AspNetCore.Models;

namespace Pot.AspNetCore.Features.Projections.Get;

internal sealed class Response : ResponseBase
{
    public List<AccountDailyBalanceAvailable> Accounts { get; init; }
    public List<DateBalanceAvailable> Global { get; init; }

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

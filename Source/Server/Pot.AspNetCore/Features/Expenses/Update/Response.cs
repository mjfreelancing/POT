using Microsoft.AspNetCore.Http.HttpResults;
using Pot.App.Features.Expenses.Update.Models;
using Pot.AspNetCore.Models;
using System.ComponentModel;

namespace Pot.AspNetCore.Features.Expenses.Update;

internal sealed class Response : ResponseBase
{
    // Included because server-side business rules may canonicalize this value from the submitted input.
    // Not actually used by the client application, but it is appropriate to return since it can be different to the original input.
    [Description("When automatic allocations will begin accruing for this expense")]
    public DateOnly? AccrualStart { get; init; }

    public static Ok<Response> Ok(Output income)
    {
        var response = new Response(income);

        return TypedResults.Ok(response);
    }

    private Response(Output income)
    {
        RowId = income.RowId;
        Etag = income.Etag;
        AccrualStart = income.AccrualStart;
    }
}

using Microsoft.AspNetCore.Http.HttpResults;
using Pot.App.Features.Expenses.Update.Models;
using Pot.AspNetCore.Models;

namespace Pot.AspNetCore.Features.Expenses.Update;

internal sealed class Response : ResponseBase
{
    // Included because server-side business rules may canonicalize this value from the submitted input.
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

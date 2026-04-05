using Microsoft.AspNetCore.Http.HttpResults;
using Pot.App.Features.Expenses.Create.Models;
using Pot.AspNetCore.Models;

namespace Pot.AspNetCore.Features.Expenses.Create;

public sealed class Response : ResponseBase
{
    // Included because server-side business rules may canonicalize this value from the submitted input.
    public DateOnly? AccrualStart { get; init; }

    public static CreatedAtRoute<Response> Created(Output expense)
    {
        var response = new Response(expense);

        return TypedResults.CreatedAtRoute(
            response,
            nameof(Extensions.RouteGroupBuilderExtensions.GetExpense),
            new { Id = response.RowId });
    }

    private Response(Output expense)
    {
        RowId = expense.RowId;
        Etag = expense.Etag;
        AccrualStart = expense.AccrualStart;
    }
}

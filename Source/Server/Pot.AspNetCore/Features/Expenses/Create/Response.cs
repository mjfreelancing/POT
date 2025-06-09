using Microsoft.AspNetCore.Http.HttpResults;
using Pot.App.Features.Expenses.Create.Models;
using Pot.AspNetCore.Models;

namespace Pot.AspNetCore.Features.Expenses.Create;

public sealed class Response : ResponseBase
{
    public static CreatedAtRoute<Response> Created(Output Expense)
    {
        var response = new Response(Expense);

        return TypedResults.CreatedAtRoute(
            response,
            nameof(Extensions.RouteGroupBuilderExtensions.GetExpense),
            new { Id = response.RowId });
    }

    private Response(Output Expense)
    {
        RowId = Expense.RowId;
        Etag = Expense.Etag;
    }
}

using Microsoft.AspNetCore.Http.HttpResults;
using Pot.App.Features.Incomes.Create.Models;
using Pot.AspNetCore.Models;

namespace Pot.AspNetCore.Features.Incomes.Create;

public sealed class Response : ResponseBase
{
    public static CreatedAtRoute<Response> Created(Output income)
    {
        var response = new Response(income);

        return TypedResults.CreatedAtRoute(
            response,
            nameof(Extensions.RouteGroupBuilderExtensions.GetIncome),
            new { Id = response.RowId });
    }

    private Response(Output income)
    {
        RowId = income.RowId;
        Etag = income.Etag;
    }
}

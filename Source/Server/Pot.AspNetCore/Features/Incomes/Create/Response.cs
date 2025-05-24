using Microsoft.AspNetCore.Http.HttpResults;
using Pot.AspNetCore.Models;
using Pot.Data.Entities;

namespace Pot.AspNetCore.Features.Incomes.Create;

public sealed class Response : ResponseBase
{
    public static CreatedAtRoute<Response> Created(IncomeEntity income)
    {
        var response = new Response(income);

        return TypedResults.CreatedAtRoute(
            response,
            nameof(Extensions.RouteGroupBuilderExtensions.GetIncome),
            new { Id = response.RowId });
    }

    private Response(IncomeEntity income)
    {
        RowId = income.RowId;
        ETag = income.Etag;
    }
}

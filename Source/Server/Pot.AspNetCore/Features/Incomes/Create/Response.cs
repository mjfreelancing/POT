using Microsoft.AspNetCore.Http.HttpResults;
using Pot.App.Features.Incomes.Create.Models;
using Pot.AspNetCore.Models;

namespace Pot.AspNetCore.Features.Incomes.Create;

/// <summary>Describes the result of creating an income.</summary>
public sealed class Response : ResponseBase
{
    /// <summary>Creates a resource response with a route to the newly created resource.</summary>
    /// <param name="income">The created income data used to construct the response.</param>
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

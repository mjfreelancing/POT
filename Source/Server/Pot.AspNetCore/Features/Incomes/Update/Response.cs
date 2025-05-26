using Microsoft.AspNetCore.Http.HttpResults;
using Pot.AspNetCore.Models;
using Pot.Data.Entities;

namespace Pot.AspNetCore.Features.Incomes.Update;

internal sealed class Response : ResponseBase
{
    public static Ok<Response> Ok(IncomeEntity income)
    {
        var response = new Response(income);

        return TypedResults.Ok(response);
    }

    private Response(IncomeEntity income)
    {
        RowId = income.RowId;
        ETag = income.Etag;
    }
}

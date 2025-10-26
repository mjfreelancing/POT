using Microsoft.AspNetCore.Http.HttpResults;
using Pot.App.Features.Sites.Update.Models;
using Pot.AspNetCore.Models;

namespace Pot.AspNetCore.Features.Sites.Update;

internal sealed class Response : ResponseBase
{
    public static Ok<Response> Ok(Output income)
    {
        var response = new Response(income);

        return TypedResults.Ok(response);
    }

    private Response(Output income)
    {
        RowId = income.RowId;
        Etag = income.Etag;
    }
}

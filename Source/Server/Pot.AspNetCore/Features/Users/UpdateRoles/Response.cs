using Microsoft.AspNetCore.Http.HttpResults;
using Pot.App.Features.Users.UpdateRoles.Models;
using Pot.AspNetCore.Models;

namespace Pot.AspNetCore.Features.Users.UpdateRoles;

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

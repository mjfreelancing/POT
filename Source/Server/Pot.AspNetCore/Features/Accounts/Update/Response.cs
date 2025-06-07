using Microsoft.AspNetCore.Http.HttpResults;
using Pot.App.Features.Accounts.Update.Models;
using Pot.AspNetCore.Models;

namespace Pot.AspNetCore.Features.Accounts.Update;

internal sealed class Response : ResponseBase
{
    public static Ok<Response> Ok(Output account)
    {
        var response = new Response(account);

        return TypedResults.Ok(response);
    }

    private Response(Output account)
    {
        RowId = account.RowId;
        Etag = account.Etag;
    }
}

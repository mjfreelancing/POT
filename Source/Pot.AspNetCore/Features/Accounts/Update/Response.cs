using Microsoft.AspNetCore.Http.HttpResults;
using Pot.AspNetCore.Models;
using Pot.Data.Entities;

namespace Pot.AspNetCore.Features.Accounts.Update;

internal sealed class Response : ResponseBase
{
    public static Ok<Response> Ok(AccountEntity account)
    {
        var response = new Response(account);

        return TypedResults.Ok(response);
    }

    private Response(AccountEntity account)
    {
        RowId = account.RowId;
        ETag = account.Etag;
    }
}

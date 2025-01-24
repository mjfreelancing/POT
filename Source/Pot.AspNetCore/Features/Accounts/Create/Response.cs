using Microsoft.AspNetCore.Http.HttpResults;
using Pot.AspNetCore.Models;
using Pot.Data.Entities;

namespace Pot.AspNetCore.Features.Accounts.Create;

public sealed class Response : ResponseBase
{
    public static CreatedAtRoute<Response> Created(AccountEntity account)
    {
        var response = new Response(account);

        return TypedResults.CreatedAtRoute(
            response,
            nameof(Extensions.RouteGroupBuilderExtensions.GetAccount),
            new { Id = response.RowId });
    }

    private Response(AccountEntity account)
    {
        RowId = account.RowId;
        ETag = account.Etag;
    }
}

using AllOverIt.Assertion;
using Microsoft.AspNetCore.Http.HttpResults;
using Pot.App.Features.Accounts.Create.Models;
using Pot.AspNetCore.Models;

namespace Pot.AspNetCore.Features.Accounts.Create;

public sealed class Response : ResponseBase
{
    public static CreatedAtRoute<Response> Created(Output account)
    {
        var response = new Response(account);

        return TypedResults.CreatedAtRoute(
            response,
            nameof(Extensions.RouteGroupBuilderExtensions.GetAccount),
            new { Id = response.RowId });
    }

    private Response(Output account)
    {
        _ = account.WhenNotNull();

        RowId = account.RowId;
        Etag = account.Etag;
    }
}

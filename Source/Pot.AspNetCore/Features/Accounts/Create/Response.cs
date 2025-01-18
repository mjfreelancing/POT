using Microsoft.AspNetCore.Http.HttpResults;
using Pot.Data.Entities;

namespace Pot.AspNetCore.Features.Accounts.Create;

internal sealed class Response
{
    public int Id { get; init; }
    public long ETag { get; init; }

    public static CreatedAtRoute<Response> Created(AccountEntity account)
    {
        var response = new Response(account);

        return TypedResults.CreatedAtRoute(
            response,
            nameof(Extensions.RouteGroupBuilderExtensions.GetAccount),
            new { response.Id });
    }

    private Response(AccountEntity account)
    {
        Id = account.Id;
        ETag = account.Etag;
    }
}

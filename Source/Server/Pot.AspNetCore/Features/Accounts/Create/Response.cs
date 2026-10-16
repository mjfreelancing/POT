using AllOverIt.Assertion;
using Microsoft.AspNetCore.Http.HttpResults;
using Pot.App.Features.Accounts.Create.Models;
using Pot.AspNetCore.Models;

namespace Pot.AspNetCore.Features.Accounts.Create;

/// <summary>Describes the result of creating an account.</summary>
public sealed class Response : ResponseBase
{
    /// <summary>Creates a resource response with a route to the newly created resource.</summary>
    /// <param name="account">The created account data used to construct the response.</param>
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

using AllOverIt.Assertion;
using AllOverIt.Extensions;
using Microsoft.AspNetCore.Http.HttpResults;
using Pot.App.Features.Approvals.Pending.Models;
using Pot.AspNetCore.Models;
using System.ComponentModel;

namespace Pot.AspNetCore.Features.Approvals.Pending;

internal sealed class Response : ResponseBase
{
    [Description("The user's username")]
    public string Username { get; init; }

    [Description("The user's email")]
    public string Email { get; init; }

    public static Ok<Response[]> Ok(List<Output> users)
    {
        var responses = users.SelectToArray(user => new Response(user));

        return TypedResults.Ok(responses);
    }

    private Response(Output user)
    {
        _ = user.WhenNotNull();

        RowId = user.RowId;
        Etag = user.Etag;
        Username = user.Username;
        Email = user.Email;
    }
}

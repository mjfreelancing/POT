using AllOverIt.Assertion;
using AllOverIt.Extensions;
using Microsoft.AspNetCore.Http.HttpResults;
using Pot.App.Features.Users.GetAll.Models;
using Pot.AspNetCore.Models;
using System.ComponentModel;

namespace Pot.AspNetCore.Features.Users.GetAll;

internal sealed class Response : ResponseBase
{
    [Description("The user's username")]
    public string Username { get; init; }

    [Description("The user's display name")]
    public string DisplayName { get; init; }

    [Description("The user's email")]
    public string Email { get; init; }

    [Description("The user's status")]
    public string Status { get; init; }

    [Description("The user's assigned roles")]
    public string[] Roles { get; init; }

    [Description("The user's last logged in timestamp (UTC)")]
    public DateTime? LastLoggedInUtc { get; init; }

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
        DisplayName = user.DisplayName;
        Email = user.Email;
        Status = user.Status;
        Roles = user.Roles;
        LastLoggedInUtc = user.LastLoggedInUtc;
    }
}

using Microsoft.AspNetCore.Http.HttpResults;
using Pot.App.Features.Auth.Me.Models;
using Pot.AspNetCore.Models;
using System.ComponentModel;

namespace Pot.AspNetCore.Features.Me.Get;

internal sealed class Response : ResponseBase
{
    [Description("The user's username")]
    public string Username { get; init; }

    [Description("The user's display name")]
    public string DisplayName { get; init; }

    [Description("The user's email")]
    public string Email { get; init; }

    [Description("The user's permissions")]
    public string[] Permissions { get; init; }

    public static Ok<Response> Ok(Output user, IEnumerable<string> permissions)
    {
        return TypedResults.Ok(new Response(user, permissions));
    }

    private Response(Output user, IEnumerable<string> permissions)
    {
        RowId = user.RowId;
        Etag = user.Etag;
        Username = user.Username;
        DisplayName = user.DisplayName;
        Email = user.Email;
        Permissions = permissions.ToArray();
    }
}
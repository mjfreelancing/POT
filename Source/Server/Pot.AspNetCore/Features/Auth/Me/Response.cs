using Microsoft.AspNetCore.Http.HttpResults;
using System.ComponentModel;

namespace Pot.AspNetCore.Features.Auth.Me;

internal sealed class Response
{
    [Description("The user's username.")]
    public string Username { get; init; }

    [Description("The user's permissions.")]
    public string[] Permissions { get; init; }

    public static Ok<Response> Ok(string username, IEnumerable<string> permissions)
    {
        return TypedResults.Ok(new Response(username, permissions));
    }

    private Response(string username, IEnumerable<string> permissions)
    {
        Username = username;
        Permissions = permissions.ToArray();
    }
}
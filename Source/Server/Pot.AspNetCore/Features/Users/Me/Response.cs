using Microsoft.AspNetCore.Http.HttpResults;
using System.ComponentModel;

namespace Pot.AspNetCore.Features.Users.Me;

internal sealed class Response
{
    [Description("The user's username")]
    public string Username { get; init; }

    [Description("The user's display name")]
    public string DisplayName { get; init; }

    [Description("The user's email")]
    public string Email { get; init; }

    [Description("The user's permissions")]
    public string[] Permissions { get; init; }

    public static Ok<Response> Ok(string username, string displayName, string email, IEnumerable<string> permissions)
    {
        return TypedResults.Ok(new Response(username, displayName, email, permissions));
    }

    private Response(string username, string displayName, string email, IEnumerable<string> permissions)
    {
        Username = username;
        DisplayName = displayName;
        Email = email;
        Permissions = permissions.ToArray();
    }
}
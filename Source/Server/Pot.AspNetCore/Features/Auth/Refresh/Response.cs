using Microsoft.AspNetCore.Http.HttpResults;

namespace Pot.AspNetCore.Features.Auth.Refresh;

/// <summary>Describes the result of refreshing a user's authentication tokens.</summary>
public sealed class Response
{
    /// <summary>Gets the refreshed access token.</summary>
    public string AccessToken { get; init; }

    /// <summary>Creates a response containing the refreshed access token.</summary>
    /// <param name="accessToken">The access token issued by the token refresh operation.</param>
    public static Ok<Response> Ok(string accessToken)
    {
        return TypedResults.Ok(new Response(accessToken));
    }

    private Response(string accessToken)
    {
        AccessToken = accessToken;
    }
}

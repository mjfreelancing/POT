using Microsoft.AspNetCore.Http.HttpResults;

namespace Pot.AspNetCore.Features.Auth.Refresh;

public sealed class Response
{
    public string AccessToken { get; init; }

    public static Ok<Response> Ok(string accessToken)
    {
        return TypedResults.Ok(new Response(accessToken));
    }

    private Response(string accessToken)
    {
        AccessToken = accessToken;
    }
}

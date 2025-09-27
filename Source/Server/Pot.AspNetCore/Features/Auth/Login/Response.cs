using AllOverIt.Assertion;
using Microsoft.AspNetCore.Http.HttpResults;
using Pot.AspNetCore.Concerns.Auth.Models;

namespace Pot.AspNetCore.Features.Auth.Login;

public sealed class Response
{
    public string AccessToken { get; init; }
    public string RefreshToken { get; init; }

    public static Ok<Response> Ok(AuthTokens authTokens)
    {
        return TypedResults.Ok(new Response(authTokens));
    }

    private Response(AuthTokens authTokens)
    {
        _ = authTokens.WhenNotNull();

        AccessToken = authTokens.AccessToken;
        RefreshToken = authTokens.RefreshToken;
    }
}

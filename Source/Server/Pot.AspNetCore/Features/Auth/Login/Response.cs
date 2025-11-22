using AllOverIt.Patterns.Enumeration;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Pot.AspNetCore.Features.Auth.Login;

public sealed class Response
{
    private sealed class LoginStatus : EnrichedEnum<LoginStatus>
    {
        public const string Success = nameof(Success);
        public const string Approval = nameof(Approval);
    }

    public required string Status { get; init; }
    public string? AccessToken { get; init; }
    public string? Message { get; init; }

    public static Ok<Response> Success(string accessToken)
    {
        return TypedResults.Ok(new Response
        {
            Status = LoginStatus.Success,
            AccessToken = accessToken
        });
    }

    public static Ok<Response> Approval(string message)
    {
        return TypedResults.Ok(new Response
        {
            Status = LoginStatus.Approval,
            Message = message
        });
    }
}

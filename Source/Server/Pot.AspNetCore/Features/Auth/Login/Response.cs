using AllOverIt.Patterns.Enumeration;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Pot.AspNetCore.Features.Auth.Login;

/// <summary>Describes the result of a user sign-in attempt.</summary>
public sealed class Response
{
    private sealed class LoginStatus : EnrichedEnum<LoginStatus>
    {
        public const string Success = nameof(Success);
        public const string Approval = nameof(Approval);
    }

    /// <summary>Gets the sign-in outcome.</summary>
    public required string Status { get; init; }
    /// <summary>Gets the access token issued after successful sign-in.</summary>
    public string? AccessToken { get; init; }
    /// <summary>Gets the message explaining why additional approval is required.</summary>
    public string? Message { get; init; }

    /// <summary>Creates a successful sign-in response.</summary>
    /// <param name="accessToken">The access token issued to the user.</param>
    public static Ok<Response> Success(string accessToken)
    {
        return TypedResults.Ok(new Response
        {
            Status = LoginStatus.Success,
            AccessToken = accessToken
        });
    }

    /// <summary>Creates a response indicating that additional approval is required.</summary>
    /// <param name="message">The message explaining the required approval.</param>
    public static Ok<Response> Approval(string message)
    {
        return TypedResults.Ok(new Response
        {
            Status = LoginStatus.Approval,
            Message = message
        });
    }
}

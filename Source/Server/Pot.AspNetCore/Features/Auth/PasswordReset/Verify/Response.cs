using Microsoft.AspNetCore.Http.HttpResults;
using Pot.App.Features.Auth.PasswordReset.Verify.Models;

namespace Pot.AspNetCore.Features.Auth.PasswordReset.Verify;

/// <summary>Describes the result of verifying a password-reset request.</summary>
public sealed class Response
{
    /// <summary>Gets the outcome of the password-reset verification.</summary>
    public string Status { get; init; }
    /// <summary>Gets an explanation of the verification outcome, when provided.</summary>
    public string? Message { get; init; }
    /// <summary>Gets the number of minutes to wait before retrying, when applicable.</summary>
    public int? RetryMinutes { get; init; }

    /// <summary>Creates a response from the password-reset verification result.</summary>
    /// <param name="output">The result returned by the password-reset verification operation.</param>
    public static Ok<Response> Ok(Output output)
    {
        return TypedResults.Ok(new Response(output));
    }

    private Response(Output output)
    {
        Status = output.Status.Name;
        Message = output.Message;
        RetryMinutes = output.RetryMinutes;
    }
}

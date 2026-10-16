using Microsoft.AspNetCore.Http.HttpResults;
using Pot.App.Features.Auth.Signup.Complete.Models;

namespace Pot.AspNetCore.Features.Auth.Signup.Complete;

/// <summary>Describes the result of completing user registration.</summary>
public sealed class Response
{
    /// <summary>Gets the outcome of the registration attempt.</summary>
    public string Status { get; init; }
    /// <summary>Gets an explanation of the registration outcome, when provided.</summary>
    public string? Message { get; init; }
    /// <summary>Gets the number of minutes to wait before retrying, when applicable.</summary>
    public int? RetryMinutes { get; init; }

    /// <summary>Creates a response from the registration-completion result.</summary>
    /// <param name="output">The result returned by the registration operation.</param>
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

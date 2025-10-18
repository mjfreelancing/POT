using Microsoft.AspNetCore.Http.HttpResults;
using Pot.App.Features.Auth.PasswordReset.Verify.Models;

namespace Pot.AspNetCore.Features.Auth.PasswordReset.Verify;

public sealed class Response
{
    public string Status { get; init; }
    public string? Message { get; init; }
    public int? RetryMinutes { get; init; }

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

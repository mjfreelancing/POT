using AllOverIt.Assertion;
using Microsoft.AspNetCore.Http.HttpResults;
using Pot.App.Features.Auth.Signup.Request.Models;

namespace Pot.AspNetCore.Features.Auth.Signup.Send;

public sealed class Response
{
    public string Status { get; init; }
    public string? Message { get; init; }
    public string? ReferenceCode { get; init; }

    public static Ok<Response> Ok(Output output)
    {
        return TypedResults.Ok(new Response(output));
    }

    private Response(Output output)
    {
        _ = output.WhenNotNull();

        Status = output.Status.Name;
        Message = output.Message;
        ReferenceCode = output.ReferenceCode;
    }
}

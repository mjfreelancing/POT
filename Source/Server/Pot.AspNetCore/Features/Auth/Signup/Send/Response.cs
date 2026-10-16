using AllOverIt.Assertion;
using Microsoft.AspNetCore.Http.HttpResults;
using Pot.App.Features.Auth.Signup.Request.Models;

namespace Pot.AspNetCore.Features.Auth.Signup.Send;

/// <summary>Describes the result of starting a user registration request.</summary>
public sealed class Response
{
    /// <summary>Gets the outcome of the registration request.</summary>
    public string Status { get; init; }
    /// <summary>Gets an explanation of the registration outcome, when provided.</summary>
    public string? Message { get; init; }
    /// <summary>Gets the reference code used to complete registration.</summary>
    public string? ReferenceCode { get; init; }

    /// <summary>Creates a response from the registration-start result.</summary>
    /// <param name="output">The result returned by the registration operation.</param>
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

using AllOverIt.Assertion;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Pot.AspNetCore.Features.Auth.PasswordReset.Send;

/// <summary>Describes the result of sending a password-reset message.</summary>
public sealed class Response
{
    /// <summary>Gets the reference code used to complete the password-reset process.</summary>
    public string ReferenceCode { get; init; }

    /// <summary>Creates a response containing the password-reset reference code.</summary>
    /// <param name="referenceCode">The reference code issued for the password-reset request.</param>
    public static Ok<Response> Ok(string referenceCode)
    {
        return TypedResults.Ok(new Response(referenceCode));
    }

    private Response(string referenceCode)
    {
        _ = referenceCode.WhenNotNull();

        ReferenceCode = referenceCode;
    }
}

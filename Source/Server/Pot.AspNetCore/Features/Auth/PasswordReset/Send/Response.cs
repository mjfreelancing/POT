using AllOverIt.Assertion;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Pot.AspNetCore.Features.Auth.PasswordReset.Send;

public sealed class Response
{
    public string ReferenceCode { get; init; }

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

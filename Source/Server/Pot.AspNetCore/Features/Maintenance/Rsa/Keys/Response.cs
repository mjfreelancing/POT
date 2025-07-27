using Microsoft.AspNetCore.Http.HttpResults;
using System.ComponentModel;

namespace Pot.AspNetCore.Features.Maintenance.Rsa.Keys;

internal sealed class Response
{
    [Description("The public key.")]
    public required string PublicKey { get; init; }

    [Description("The private key.")]
    public required string PrivateKey { get; init; }

    public static Ok<Response> Ok(string publicKey, string privateKey)
    {
        var response = new Response
        {
            PublicKey = publicKey,
            PrivateKey = privateKey
        };

        return TypedResults.Ok(response);
    }
}

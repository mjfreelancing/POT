using AllOverIt.Cryptography.Extensions;
using AllOverIt.Cryptography.RSA;
using AllOverIt.Logging.Extensions;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Pot.AspNetCore.Features.Maintenance.Rsa.Keys;

internal sealed class Handler
{
    public static Task<Ok<Response>> Invoke(ILogger<Handler> logger, IConfiguration config, CancellationToken _)
    {
        logger.LogCall(null);

        var rsaKeyPair = new RsaKeyPair();

        var publicKey = rsaKeyPair.GetPublicKeyAsBase64();
        var privateKey = rsaKeyPair.GetPrivateKeyAsBase64();

        var response = Response.Ok(publicKey, privateKey);

        return Task.FromResult(response);
    }
}

using AllOverIt.Extensions;
using AllOverIt.Logging.Extensions;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Pot.AspNetCore.Concerns.Auth;
using Pot.AspNetCore.Concerns.Validation;
using Pot.AspNetCore.Extensions;
using System.ComponentModel;

namespace Pot.AspNetCore.Features.Auth.Refresh;

internal sealed class Handler
{
    private const string AuthPrefix = "Bearer ";

    public static async Task<Results<Ok<Response>, ProblemHttpResult>> Invoke(Request request,

        [FromHeader(Name = "Authorization")]
        [Description("The access token to be refreshed")]
        string? accessToken,

        IAuthService authService, IProblemDetailsInspector problemDetailsInspector,
        ILogger<Handler> logger, CancellationToken cancellationToken)
    {
        logger.LogCall(null);

        if (accessToken.IsNullOrEmpty())
        {
            return AuthUtils.CreateAuthErrorResult();
        }

        var problemDetails = problemDetailsInspector.Validate(request);

        if (problemDetails.IsProblem())
        {
            logger.LogErrors(problemDetails);

            return TypedResults.Problem(problemDetails);
        }

        if (accessToken.StartsWith(AuthPrefix, StringComparison.OrdinalIgnoreCase))
        {
            accessToken = accessToken[AuthPrefix.Length..];
        }

        var authTokens = await authService.RefreshAsync(accessToken, request.RefreshToken, cancellationToken);

        return authTokens.IsSuccess
            ? Response.Ok(authTokens.Value!)
            : TypedResults.Problem(authTokens.Error!.ToProblemDetails());
    }
}


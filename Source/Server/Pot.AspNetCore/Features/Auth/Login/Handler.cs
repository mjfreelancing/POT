using AllOverIt.Logging.Extensions;
using Microsoft.AspNetCore.Http.HttpResults;
using Pot.AspNetCore.Concerns.Auth;
using Pot.AspNetCore.Concerns.Validation;
using Pot.AspNetCore.Extensions;

namespace Pot.AspNetCore.Features.Auth.Login;

internal sealed class Handler
{
    public static async Task<Results<Ok<Response>, ProblemHttpResult>> Invoke(Request request,
        IAuthService authService, IProblemDetailsInspector problemDetailsInspector,
        ILogger<Handler> logger, CancellationToken cancellationToken)
    {
        logger.LogCall(null, new { request.Username });

        var problemDetails = problemDetailsInspector.Validate(request);

        if (problemDetails.IsProblem())
        {
            logger.LogErrors(problemDetails);

            return TypedResults.Problem(problemDetails);
        }

        var authTokens = await authService.LoginAsync(request.Username, request.Password, cancellationToken);

        return authTokens.IsSuccess
            ? Response.Ok(authTokens.Value!)
            : TypedResults.Problem(authTokens.Error!.ToProblemDetails());
    }
}

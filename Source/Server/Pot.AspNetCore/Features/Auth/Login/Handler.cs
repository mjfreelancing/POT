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

        var authResult = await authService.LoginAsync(request.Username.Trim(), request.Password, cancellationToken);

        if (!authResult.IsSuccess)
        {
            // Authentication failed - bad credentials
            return TypedResults.Problem(authResult.Error!.ToProblemDetails());
        }

        if (authResult.Value is null)
        {
            // Successful credential validation but account pending approval
            return Response.Approval("Your account is pending approval. You'll receive an email when your account is activated.");
        }

        // Successful login with tokens
        return Response.Success(authResult.Value);
    }
}

using AllOverIt.Logging.Extensions;
using Microsoft.AspNetCore.Http.HttpResults;
using Pot.AspNetCore.Concerns.Auth;
using Pot.AspNetCore.Concerns.Validation;
using Pot.AspNetCore.Extensions;
using Pot.AspNetCore.Features.Auth.Services;

namespace Pot.AspNetCore.Features.Auth.ChangePassword;

internal sealed class Handler
{
    public static async Task<Results<Ok, ProblemHttpResult>> Invoke(HttpContext httpContext, Request request,
        IHttpUserService userService, IAuthService authService, IProblemDetailsInspector problemDetailsInspector,
        ILogger<Handler> logger, CancellationToken cancellationToken)
    {
        logger.LogCall(null);

        var problemDetails = problemDetailsInspector.Validate(request);

        if (problemDetails.IsProblem())
        {
            logger.LogErrors(problemDetails);

            return TypedResults.Problem(problemDetails);
        }

        var userInfo = await userService.GetUserInfoAsync(httpContext, cancellationToken);

        if (userInfo is null)
        {
            return AuthUtils.CreateAuthErrorResult();
        }

        var passwordChanged = await authService.ChangePasswordAsync(userInfo.UserId, request.CurrentPassword, request.NewPassword, cancellationToken);

        return passwordChanged.IsSuccess
            ? TypedResults.Ok()
            : AuthUtils.CreateAuthErrorResult();
    }
}

using AllOverIt.Logging.Extensions;
using Microsoft.AspNetCore.Http.HttpResults;
using Pot.App.Errors;
using Pot.AspNetCore.Concerns.Auth;
using Pot.AspNetCore.Concerns.Validation;
using Pot.AspNetCore.Extensions;
using Pot.AspNetCore.Features.Me.Services;

namespace Pot.AspNetCore.Features.Me.ChangePassword;

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

        var userInfo = await userService.GetMeInfoAsync(httpContext, cancellationToken);

        if (userInfo is null)
        {
            return CreateInvalidUserOrPasswordError();
        }

        var passwordChanged = await authService.ChangePasswordAsync(userInfo.RowId, request.CurrentPassword, request.NewPassword, cancellationToken);

        return passwordChanged.IsSuccess
            ? TypedResults.Ok()
            : CreateInvalidUserOrPasswordError();
    }

    private static ProblemHttpResult CreateInvalidUserOrPasswordError()
    {
        var problemDetailsError = ApiDetailErrorFactory.CreateUnprocessableEntityError("Invalid user or password");
        return TypedResults.Problem(problemDetailsError.ToProblemDetails());
    }
}

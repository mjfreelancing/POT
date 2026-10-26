using AllOverIt.Logging.Extensions;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Pot.App.Features.Sites.Delete;
using Pot.AspNetCore.Concerns.Auth.Configuration;
using Pot.AspNetCore.Concerns.Validation;
using Pot.AspNetCore.Extensions;
using Pot.AspNetCore.Features.Auth;

namespace Pot.AspNetCore.Features.Sites.Delete;

internal sealed class Handler
{
    public static async Task<Results<Ok, ProblemHttpResult>> Invoke(HttpContext httpContext, [FromBody] Request request,
        IDeleteSiteService siteService, IProblemDetailsInspector problemDetailsInspector,
        PlatformAdminOptions platformAdminOptions, AuthenticationOptions authOptions, ILogger<Handler> logger,
        CancellationToken cancellationToken)
    {
        logger.LogCall(null);

        var problemDetails = problemDetailsInspector.Validate(request);

        if (problemDetails.IsProblem())
        {
            return TypedResults.Problem(problemDetails);
        }

        var input = request.MapToInput(platformAdminOptions.GetUserRowIds());

        var result = await siteService.DeleteSiteAsync(input, cancellationToken);

        if (!result.IsSuccess)
        {
            return TypedResults.Problem(result.Error!.ToProblemDetails());
        }

        // The caller's session rows are gone with the site, so the cookie is cleared to stop the client
        // presenting a refresh token for a session that no longer exists.
        RefreshTokenHelper.ClearCookie(httpContext, authOptions);

        return TypedResults.Ok();
    }
}

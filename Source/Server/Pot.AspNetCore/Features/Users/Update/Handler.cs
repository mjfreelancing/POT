using AllOverIt.Logging.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Pot.App.Features.Users.Update;
using Pot.AspNetCore.Concerns.Validation;
using Pot.AspNetCore.Extensions;
using System.ComponentModel;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Pot.AspNetCore.Features.Users.Update;

internal sealed class Handler
{
    private const string ManageUsersPolicy = "user:manage";

    public static async Task<Results<Ok<Response>, NotFound, ProblemHttpResult>> Invoke([Description("The user Id")] Guid id,
        Request request, IUpdateUserService userService, IProblemDetailsInspector problemDetailsInspector,
        IAuthorizationService authorizationService, ClaimsPrincipal user, ILogger<Handler> logger,
        CancellationToken cancellationToken)
    {
        logger.LogCall(null);

        // A caller may edit their own details; editing anyone else needs user:manage. Anything the caller
        // cannot act on is refused as not-found, so the response cannot be used to probe for other users.
        if (!IsSelf(user, id) && !await IsAuthorizedToManageUsersAsync(authorizationService, user))
        {
            logger.LogWarning("Refused an update of user {RequestedRowId}: the caller may only edit their own details", id);

            return TypedResults.NotFound();
        }

        var problemDetails = problemDetailsInspector.Validate(request);

        if (problemDetails.IsProblem())
        {
            logger.LogErrors(problemDetails);

            return TypedResults.Problem(problemDetails);
        }

        var userInput = request.MapToInput(id);

        var result = await userService.UpdateUserAsync(userInput, cancellationToken);

        return result.IsSuccess
            ? Response.Ok(result.Value!)
            : TypedResults.Problem(result.Error!.ToProblemDetails());
    }

    private static bool IsSelf(ClaimsPrincipal user, Guid id)
    {
        var callerId = user.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

        return Guid.TryParse(callerId, out var callerRowId) && callerRowId == id;
    }

    private static async Task<bool> IsAuthorizedToManageUsersAsync(IAuthorizationService authorizationService, ClaimsPrincipal user)
    {
        var authorization = await authorizationService.AuthorizeAsync(user, ManageUsersPolicy);

        return authorization.Succeeded;
    }
}

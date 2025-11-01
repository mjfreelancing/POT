using AllOverIt.Logging.Extensions;
using Microsoft.AspNetCore.Http.HttpResults;
using Pot.App.Features.Users.Invite;
using Pot.AspNetCore.Concerns.Validation;
using Pot.AspNetCore.Extensions;
using Pot.AspNetCore.Features.Users.Invite.Mappings;

namespace Pot.AspNetCore.Features.Users.Invite;

internal sealed class Handler
{
    public static async Task<Results<Ok, ProblemHttpResult>> Invoke(Request request, IInviteUserService inviteUserService,
        IProblemDetailsInspector problemDetailsInspector, ILogger<Handler> logger, CancellationToken cancellationToken)
    {
        logger.LogCall(null, new { request.Username, request.RoleIds });

        var problemDetails = problemDetailsInspector.Validate(request);

        if (problemDetails.IsProblem())
        {
            logger.LogErrors(problemDetails);

            return TypedResults.Problem(problemDetails);
        }

        var input = request.MapToInput();

        var output = await inviteUserService.InviteUserAsync(input, cancellationToken);

        return output.IsSuccess
            ? TypedResults.Ok()
            : TypedResults.Problem(output.Error!.ToProblemDetails());
    }
}

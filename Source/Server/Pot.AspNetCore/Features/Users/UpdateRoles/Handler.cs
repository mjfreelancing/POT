using AllOverIt.Logging.Extensions;
using Microsoft.AspNetCore.Http.HttpResults;
using Pot.App.Features.Users.UpdateRoles;
using Pot.AspNetCore.Extensions;
using Pot.AspNetCore.Features.Users.UpdateRoles.Mappings;
using System.ComponentModel;

namespace Pot.AspNetCore.Features.Users.UpdateRoles;

internal sealed class Handler
{
    public static async Task<Results<Ok<Response>, NotFound, ProblemHttpResult>> Invoke([Description("The user Id")] Guid id,
        Request request, IUpdateUserRolesService userRolesService, ILogger<Handler> logger, CancellationToken cancellationToken)
    {
        logger.LogCall(null);

        var userInput = request.MapToInput(id);

        var result = await userRolesService.UpdateUserRolesAsync(userInput, cancellationToken);

        return result.IsSuccess
            ? Response.Ok(result.Value!)
            : TypedResults.Problem(result.Error!.ToProblemDetails());
    }
}

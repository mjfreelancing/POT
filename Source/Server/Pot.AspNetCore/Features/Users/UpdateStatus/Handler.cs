using AllOverIt.Logging.Extensions;
using Microsoft.AspNetCore.Http.HttpResults;
using Pot.App.Features.Users.UpdateStatus;
using Pot.AspNetCore.Concerns.Auth.Configuration;
using Pot.AspNetCore.Extensions;
using System.ComponentModel;

namespace Pot.AspNetCore.Features.Users.UpdateStatus;

internal sealed class Handler
{
    public static async Task<Results<Ok<Response>, NotFound, ProblemHttpResult>> Invoke([Description("The user Id")] Guid id,
        Request request, IUpdateUserStatusService userService, PlatformAdminOptions platformAdminOptions,
        ILogger<Handler> logger, CancellationToken cancellationToken)
    {
        logger.LogCall(null);

        var userInput = request.MapToInput(id, platformAdminOptions.GetUserRowIds());

        var result = await userService.UpdateUserStatusAsync(userInput, cancellationToken);

        return result.IsSuccess
            ? Response.Ok(result.Value!)
            : TypedResults.Problem(result.Error!.ToProblemDetails());
    }
}

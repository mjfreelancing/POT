using AllOverIt.Logging.Extensions;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Pot.App.Features.Users.Delete;
using Pot.AspNetCore.Concerns.Auth.Configuration;
using Pot.AspNetCore.Extensions;
using System.ComponentModel;

namespace Pot.AspNetCore.Features.Users.Delete;

internal sealed class Handler
{
    public static async Task<Results<Ok, ProblemHttpResult>> Invoke([Description("The user Id")] Guid id,
        [FromBody] Request request, IDeleteUserService userService, PlatformAdminOptions platformAdminOptions,
        ILogger<Handler> logger, CancellationToken cancellationToken)
    {
        logger.LogCall(null);

        var userInput = request.MapToInput(id, platformAdminOptions.GetUserRowIds());

        var result = await userService.DeleteUserAsync(userInput, cancellationToken);

        return result.IsSuccess
            ? TypedResults.Ok()
            : TypedResults.Problem(result.Error!.ToProblemDetails());
    }
}

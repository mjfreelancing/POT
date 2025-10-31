using AllOverIt.Logging.Extensions;
using Microsoft.AspNetCore.Http.HttpResults;
using Pot.App.Features.Users.UpdateStatus;
using Pot.AspNetCore.Extensions;
using Pot.AspNetCore.Features.Users.UpdateStatus.Mappings;
using System.ComponentModel;

namespace Pot.AspNetCore.Features.Users.UpdateStatus;

internal sealed class Handler
{
    public static async Task<Results<Ok<Response>, NotFound, ProblemHttpResult>> Invoke([Description("The user Id")] Guid id,
        Request request, IUpdateUserStatusService userService, ILogger<Handler> logger, CancellationToken cancellationToken)
    {
        logger.LogCall(null);

        var userInput = request.MapToInput(id);

        var result = await userService.UpdateUserStatusAsync(userInput, cancellationToken);

        return result.IsSuccess
            ? Response.Ok(result.Value!)
            : TypedResults.Problem(result.Error!.ToProblemDetails());
    }
}

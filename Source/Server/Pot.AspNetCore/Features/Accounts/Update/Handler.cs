using AllOverIt.Logging.Extensions;
using Microsoft.AspNetCore.Http.HttpResults;
using Pot.App.Features.Accounts.Update;
using Pot.AspNetCore.Concerns.Validation;
using Pot.AspNetCore.Extensions;
using Pot.AspNetCore.Features.Accounts.Update.Mappings;
using System.ComponentModel;

namespace Pot.AspNetCore.Features.Accounts.Update;

internal sealed class Handler
{
    public static async Task<Results<Ok<Response>, ProblemHttpResult>> Invoke([Description("The account Id")] Guid id,
        Request request, IUpdateAccountService accountService, IProblemDetailsInspector problemDetailsInspector,
        ILogger<Handler> logger, CancellationToken cancellationToken)
    {
        logger.LogCall(null);

        var problemDetails = problemDetailsInspector.Validate(request);

        if (problemDetails.IsProblem())
        {
            logger.LogErrors(problemDetails);

            return TypedResults.Problem(problemDetails);
        }

        var accountInput = request.MapToInput(id);

        var result = await accountService.UpdateAccountAsync(accountInput, cancellationToken);

        return result.IsSuccess
            ? Response.Ok(result.Value!)
            : TypedResults.Problem(result.Error!.ToProblemDetails());
    }
}

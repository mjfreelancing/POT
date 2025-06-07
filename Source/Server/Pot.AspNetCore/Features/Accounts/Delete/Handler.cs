using AllOverIt.Logging.Extensions;
using Microsoft.AspNetCore.Http.HttpResults;
using Pot.App.Features.Accounts.Delete;
using Pot.AspNetCore.Extensions;
using System.ComponentModel;

namespace Pot.AspNetCore.Features.Accounts.Delete;

internal sealed class Handler
{
    public static async Task<Results<Ok, NotFound, ProblemHttpResult>> Invoke([Description("The account Id.")] Guid id,
        IDeleteAccountService accountService, ILogger<Handler> logger, CancellationToken cancellationToken)
    {
        logger.LogCall(null);

        var deletedResult = await accountService.DeleteAccountAsync(id, cancellationToken);

        if (deletedResult.IsSuccess)
        {
            return deletedResult.Value
                ? TypedResults.Ok()
                : TypedResults.NotFound();
        }

        return TypedResults.Problem(deletedResult.Error!.GetProblemDetails());
    }
}

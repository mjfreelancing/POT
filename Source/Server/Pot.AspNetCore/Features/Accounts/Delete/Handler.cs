using AllOverIt.Logging.Extensions;
using Microsoft.AspNetCore.Http.HttpResults;
using Pot.App.Features.Accounts.Delete;
using System.ComponentModel;

namespace Pot.AspNetCore.Features.Accounts.Delete;

internal sealed class Handler
{
    // A 422 may be returned if the account cannot be deleted due to being associated with one or more expenses or incomes.
    public static async Task<Results<Ok, NotFound, ProblemHttpResult>> Invoke([Description("The account Id.")] Guid id,
        IDeleteAccountService accountService, ILogger<Handler> logger, CancellationToken cancellationToken)
    {
        logger.LogCall(null);

        var deleted = await accountService.DeleteAccountAsync(id, cancellationToken);

        return deleted
            ? TypedResults.Ok()
            : TypedResults.NotFound();
    }
}

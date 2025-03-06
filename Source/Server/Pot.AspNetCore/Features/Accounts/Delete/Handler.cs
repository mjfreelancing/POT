using AllOverIt.Logging.Extensions;
using Microsoft.AspNetCore.Http.HttpResults;
using Pot.Data.Repositories.Accounts;
using System.ComponentModel;

namespace Pot.AspNetCore.Features.Accounts.Delete;

internal sealed class Handler
{
    // A 422 may be returned if the account cannot be deleted due to being associated with one or more expenses.
    public static async Task<Results<Ok, NotFound, ProblemHttpResult>> Invoke([Description("The account Id.")] Guid id,
        IPersistableAccountRepository accountRepository, ILogger<Handler> logger, CancellationToken cancellationToken)
    {
        logger.LogCall(null);

        var account = await accountRepository.GetAccountOrDefaultAsync(id, cancellationToken);

        if (account is null)
        {
            return TypedResults.NotFound();
        }

        // TODO: Don't allow deletion of accounts with associated expenses.
        //       Consider soft deletes. ? Reporting requirements.
        //       Decide when the need arises.

        accountRepository.Delete(account);

        await accountRepository.SaveAsync(cancellationToken);

        return TypedResults.Ok();
    }
}

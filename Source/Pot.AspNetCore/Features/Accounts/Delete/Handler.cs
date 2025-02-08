using AllOverIt.Logging.Extensions;
using Microsoft.AspNetCore.Http.HttpResults;
using Pot.Data.Repositories.Accounts;
using System.ComponentModel;

namespace Pot.AspNetCore.Features.Accounts.Delete;

internal sealed class Handler
{
    public static async Task<Results<Ok, NotFound, ProblemHttpResult>> Invoke([Description("The account Id.")] Guid id,
        IPersistableAccountRepository accountRepository, ILogger<Handler> logger, CancellationToken cancellationToken)
    {
        logger.LogCall(null);

        var account = await accountRepository.GetAccountOrDefaultAsync(id, cancellationToken);

        if (account is null)
        {
            return TypedResults.NotFound();
        }

        // TODO: Don't allow deletion of accounts with associated expenses
        // TODO: Consider soft deletes. ? Reporting requirements.

        accountRepository.Delete(account);
        await accountRepository.SaveAsync(cancellationToken);

        return TypedResults.Ok();
    }
}

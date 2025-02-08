using AllOverIt.Logging.Extensions;
using Microsoft.AspNetCore.Http.HttpResults;
using Pot.Data.Repositories.Accounts;
using System.ComponentModel;

namespace Pot.AspNetCore.Features.Accounts.Get;

internal sealed class Handler
{
    public static async Task<Results<Ok<Response>, NotFound, ProblemHttpResult>> Invoke([Description("The account Id.")] Guid id,
        IAccountRepository accountRepository, ILogger<Handler> logger, CancellationToken cancellationToken)
    {
        logger.LogCall(null);

        var account = await accountRepository.GetAccountOrDefaultAsync(id, cancellationToken);

        return account is null
            ? TypedResults.NotFound()
            : Response.Ok(account);
    }
}

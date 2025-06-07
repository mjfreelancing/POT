using AllOverIt.Logging.Extensions;
using Microsoft.AspNetCore.Http.HttpResults;
using Pot.App.Features.Accounts.Get;
using System.ComponentModel;

namespace Pot.AspNetCore.Features.Accounts.Get;

using OkGetResult = Ok<Response>;

internal sealed class Handler
{
    public static async Task<Results<OkGetResult, NotFound, ProblemHttpResult>> Invoke([Description("The account Id.")] Guid id,
        IGetAccountService accountService, ILogger<Handler> logger, CancellationToken cancellationToken)
    {
        logger.LogCall(null);

        var account = await accountService.GetAccountAsync(id, cancellationToken);

        return account is null
            ? TypedResults.NotFound()
            : Response.Ok(account);
    }
}

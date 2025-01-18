using AllOverIt.Logging.Extensions;
using Microsoft.AspNetCore.Http.HttpResults;
using Pot.Data.Repositories.Accounts;
using System.ComponentModel;

namespace Pot.AspNetCore.Features.Accounts.Get;

internal sealed class Handler
{
    public static async Task<Results<Ok<Response>, NotFound>> Invoke([Description("The account Id to get.")] int id,
        IAccountRepository accountRepository, ILogger<Handler> logger, CancellationToken cancellationToken)
    {
        logger.LogCall(null);

        var account = await accountRepository.GetByIdOrDefaultAsync(id, cancellationToken);

        if (account is null)
        {
            return TypedResults.NotFound();
        }

        return Response.Ok(account);
    }
}

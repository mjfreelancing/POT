using AllOverIt.Logging.Extensions;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Pot.Data.Repositories.Accounts;

namespace Pot.AspNetCore.Features.Accounts.GetAll;

internal sealed class Handler
{
    public static async Task<Ok<Response[]>> Invoke(IAccountRepository accountRepository, ILogger<Handler> logger,
        CancellationToken cancellationToken)
    {
        logger.LogCall(null);

        var accounts = await accountRepository.GetAll().ToListAsync(cancellationToken);

        return Response.Ok(accounts);
    }
}

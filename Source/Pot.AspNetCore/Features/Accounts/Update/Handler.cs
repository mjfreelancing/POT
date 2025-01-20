using AllOverIt.Logging.Extensions;
using Microsoft.AspNetCore.Http.HttpResults;
using Pot.Data.Extensions;
using Pot.Data.Repositories.Accounts;
using System.Net;

namespace Pot.AspNetCore.Features.Accounts.Update;

internal sealed class Handler
{
    public static async Task<Results<Ok<Response>, NotFound, StatusCodeHttpResult, ProblemHttpResult>> Invoke(Request request,
        IAccountRepository accountRepository, ILogger<Handler> logger, CancellationToken cancellationToken)
    {
        logger.LogCall(null);

        accountRepository.DbContext.WithTracking(true);

        var account = await accountRepository.FindAccountOrDefaultAsync(request.Bsb, request.Number, cancellationToken);

        if (account is null)
        {
            return TypedResults.NotFound();
        }

        account.Bsb = request.Bsb;
        account.Number = request.Number;
        account.Description = request.Description;
        account.Balance = request.Balance;
        account.Reserved = request.Reserved;
        account.Allocated = request.Allocated;
        account.DailyAccrual = request.DailyAccrual;

        // Don't call accountRepository.Update(account) as this will mark the
        // entity as modified even if nothing was changed.

        var update = await accountRepository.SaveAsync(cancellationToken);

        if (update == 0)
        {
            return TypedResults.StatusCode((int)HttpStatusCode.NotModified);
        }

        return Response.Ok(account);
    }
}

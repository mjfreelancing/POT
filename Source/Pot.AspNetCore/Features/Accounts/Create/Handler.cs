using AllOverIt.Logging.Extensions;
using Microsoft.AspNetCore.Http.HttpResults;
using Pot.Data.Entities;
using Pot.Data.Repositories.Accounts;

namespace Pot.AspNetCore.Features.Accounts.Create;

internal sealed class Handler
{
    public static async Task<Results<CreatedAtRoute<Response>, ProblemHttpResult>> Invoke(Request request,
        IAccountRepository accountRepository, ILogger<Handler> logger, CancellationToken cancellationToken)
    {
        logger.LogCall(null);

        var account = new AccountEntity
        {
            Bsb = request.Bsb,
            Number = request.Number,
            Description = request.Description,
            Balance = request.Balance,
            Reserved = request.Reserved,
            Allocated = request.Allocated,
            DailyAccrual = request.DailyAccrual
        };

        await accountRepository.AddAndSaveAsync(account, cancellationToken);

        return Response.Created(account);
    }
}

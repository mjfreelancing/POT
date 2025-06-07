using AllOverIt.Logging.Extensions;
using Microsoft.AspNetCore.Http.HttpResults;
using Pot.App.Features.Accounts.GetAll;

namespace Pot.AspNetCore.Features.Accounts.GetAll;

internal sealed class Handler
{
    public static async Task<Ok<Response[]>> Invoke(IGetAllAccountsService accountService, ILogger<Handler> logger,
        CancellationToken cancellationToken)
    {
        logger.LogCall(null);

        var accounts = await accountService.GetAllAccountsAsync(cancellationToken);

        return Response.Ok(accounts);
    }
}

using AllOverIt.Logging.Extensions;
using Microsoft.AspNetCore.Http.HttpResults;
using Pot.App.Features.Accounts.Delete;
using Pot.AspNetCore.Extensions;
using System.ComponentModel;

namespace Pot.AspNetCore.Features.Accounts.Delete;

internal sealed class Handler
{
    public static async Task<Results<Ok, ProblemHttpResult>> Invoke([Description("The account Id")] Guid id,
        IDeleteAccountService accountService, ILogger<Handler> logger, CancellationToken cancellationToken)
    {
        logger.LogCall(null);

        var result = await accountService.DeleteAccountAsync(id, cancellationToken);

        return result.IsSuccess
            ? TypedResults.Ok()
            : TypedResults.Problem(result.Error!.ToProblemDetails());
    }
}

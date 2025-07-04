using AllOverIt.Logging.Extensions;
using Microsoft.AspNetCore.Http.HttpResults;
using Pot.App.Features.Accounts.Get;
using Pot.AspNetCore.Extensions;
using System.ComponentModel;

namespace Pot.AspNetCore.Features.Accounts.Get;

using OkGetResult = Ok<Response>;

internal sealed class Handler
{
    public static async Task<Results<OkGetResult, ProblemHttpResult>> Invoke([Description("The account Id.")] Guid id,
        IGetAccountService accountService, ILogger<Handler> logger, CancellationToken cancellationToken)
    {
        logger.LogCall(null);

        var result = await accountService.GetAccountWithLinkedCountsAsync(id, cancellationToken);

        return result.IsSuccess
            ? Response.Ok(result.Value!)
            : TypedResults.Problem(result.Error!.GetProblemDetails());
    }
}

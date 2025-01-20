using AllOverIt.Logging.Extensions;
using Microsoft.AspNetCore.Http.HttpResults;
using Pot.AspNetCore.ProblemDetails;
using Pot.AspNetCore.ProblemDetails.Extensions;
using Pot.Data.Repositories.Accounts;
using System.ComponentModel;

namespace Pot.AspNetCore.Features.Accounts.Get;

internal sealed class Handler
{
    public static async Task<Results<Ok<Response>, NotFound, ProblemHttpResult>> Invoke([Description("The account Id.")] string id,
        IAccountRepository accountRepository, ILogger<Handler> logger, CancellationToken cancellationToken)
    {
        logger.LogCall(null);

        if (!Guid.TryParse(id, out var accountId))
        {
            var problemDetails = ProblemDetailsFactory.CreateUnprocessableEntity(nameof(id), id, "The value is not a valid identifier");

            logger.LogErrors(problemDetails);

            return TypedResults.Problem(problemDetails);
        }

        var account = await accountRepository.GetByRowIdOrDefaultAsync(accountId, cancellationToken);

        if (account is null)
        {
            return TypedResults.NotFound();
        }

        return Response.Ok(account);
    }
}

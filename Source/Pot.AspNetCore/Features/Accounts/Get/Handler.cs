using AllOverIt.Logging.Extensions;
using Microsoft.AspNetCore.Http.HttpResults;
using Pot.AspNetCore.Concerns.ProblemDetails.Extensions;
using Pot.AspNetCore.Concerns.Validation;
using Pot.Data.Repositories.Accounts;
using System.ComponentModel;

namespace Pot.AspNetCore.Features.Accounts.Get;

internal sealed class Handler
{
    // We could use Guid for the Id but this raises a BadHttpRequestException without the ability to easily customise the error response.
    // Prefer to return 422 consistently over a mixture of 400 and 422.
    public static async Task<Results<Ok<Response>, NotFound, ProblemHttpResult>> Invoke([Description("The account Id.")] string id,
        IAccountRepository accountRepository, ILogger<Handler> logger, CancellationToken cancellationToken)
    {
        logger.LogCall(null);

        var problemDetails = ProblemDetailsInspector.ValidateAsRowId(id, "Account", out var accountId);

        if (problemDetails.IsProblem())
        {
            logger.LogErrors(problemDetails);

            return TypedResults.Problem(problemDetails);
        }

        var account = await accountRepository.GetAccountOrDefaultAsync(accountId, cancellationToken);

        return account is null
            ? TypedResults.NotFound()
            : Response.Ok(account);
    }
}

using AllOverIt.Logging.Extensions;
using Microsoft.AspNetCore.Http.HttpResults;
using Pot.AspNetCore.ProblemDetails;
using Pot.AspNetCore.ProblemDetails.Extensions;
using Pot.Data.Repositories.Accounts;
using System.ComponentModel;
using System.Net;

namespace Pot.AspNetCore.Features.Accounts.Get;

internal sealed class Handler
{
    public static async Task<Results<Ok<Response>, ProblemHttpResult, NotFound>> Invoke([Description("The account Id.")] string id,
        IAccountRepository accountRepository, ILogger<Handler> logger, CancellationToken cancellationToken)
    {
        logger.LogCall(null);

        if (!Guid.TryParse(id, out var accountId))
        {
            var errorDetails = new ProblemDetailsError
            {
                PropertyName = nameof(id),
                // ErrorCode = ,
                AttemptedValue = id,
                ErrorMessage = "The value is not a valid identifier"
            };

            var problemDetails = new Microsoft.AspNetCore.Mvc.ProblemDetails
            {
                Detail = "One or more validation errors occurred.",
                Status = (int)HttpStatusCode.UnprocessableEntity,
                Extensions = new Dictionary<string, object?>
                {
                    { "errors", errorDetails }
                }
            };

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

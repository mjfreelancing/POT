using AllOverIt.Logging.Extensions;
using Microsoft.AspNetCore.Http.HttpResults;
using Pot.AspNetCore.Concerns.ProblemDetails.Extensions;
using Pot.AspNetCore.Concerns.Validation;
using Pot.AspNetCore.Extensions;
using Pot.AspNetCore.Features.Accounts.Create.Services;

namespace Pot.AspNetCore.Features.Accounts.Create;

internal sealed class Handler
{
    public static async Task<Results<CreatedAtRoute<Response>, ProblemHttpResult>> Invoke(Request request,
        IProblemDetailsInspector problemDetailsInspector, ICreateAccountService createAccountService,
        ILogger<Handler> logger, CancellationToken cancellationToken)
    {
        logger.LogCall(null);

        var problemDetails = problemDetailsInspector.Validate(request);

        if (problemDetails.IsProblem())
        {
            logger.LogErrors(problemDetails);

            return TypedResults.Problem(problemDetails);
        }

        var result = await createAccountService.CreateAccountAsync(request, cancellationToken);

        return result.IsSuccess
            ? Response.Created(result.Value!)
            : TypedResults.Problem(result.Error!.GetProblemDetails());
    }
}

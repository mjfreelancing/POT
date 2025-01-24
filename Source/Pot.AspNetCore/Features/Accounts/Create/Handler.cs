using AllOverIt.Logging.Extensions;
using Microsoft.AspNetCore.Http.HttpResults;
using Pot.AspNetCore.Errors;
using Pot.AspNetCore.Features.Accounts.Create.Services;
using Pot.AspNetCore.ProblemDetails.Extensions;
using Pot.AspNetCore.Validation;
using Pot.AspNetCore.Validation.Extensions;

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

        if (result.IsSuccess)
        {
            return Response.Created(result.Value!);
        }

        var error = result.Error as ServiceError;

        return TypedResults.Problem(error!.ProblemDetails);
    }
}

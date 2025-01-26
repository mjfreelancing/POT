﻿using AllOverIt.Logging.Extensions;
using Microsoft.AspNetCore.Http.HttpResults;
using Pot.AspNetCore.Concerns.ProblemDetails.Extensions;
using Pot.AspNetCore.Concerns.Validation;
using Pot.AspNetCore.Concerns.Validation.Extensions;
using Pot.AspNetCore.Extensions;
using Pot.AspNetCore.Features.Accounts.Update.Services;
using Pot.Data.Repositories.Accounts;

namespace Pot.AspNetCore.Features.Accounts.Update;

internal sealed class Handler
{
    public static async Task<Results<Ok<Response>, NotFound, ProblemHttpResult>> Invoke(Request request,
        IProblemDetailsInspector problemDetailsInspector, IUpdateAccountService updateAccountService,
        IAccountRepository accountRepository, ILogger<Handler> logger, CancellationToken cancellationToken)
    {
        logger.LogCall(null);

        var problemDetails = problemDetailsInspector.Validate(request);

        if (problemDetails.IsProblem())
        {
            logger.LogErrors(problemDetails);

            return TypedResults.Problem(problemDetails);
        }

        var result = await updateAccountService.UpdateAccountAsync(request, cancellationToken);

        return result.IsSuccess
         ? Response.Ok(result.Value!)
         : TypedResults.Problem(result.Error!.GetProblemDetails());
    }
}

using AllOverIt.Logging.Extensions;
using Microsoft.AspNetCore.Http.HttpResults;
using Pot.App.Features.Incomes.Create;
using Pot.AspNetCore.Concerns.Validation;
using Pot.AspNetCore.Extensions;
using Pot.AspNetCore.Features.Incomes.Create.Mappings;

namespace Pot.AspNetCore.Features.Incomes.Create;

internal sealed class Handler
{
    public static async Task<Results<CreatedAtRoute<Response>, ProblemHttpResult>> Invoke(Request request,
        ICreateIncomeService incomeService, IProblemDetailsInspector problemDetailsInspector,
        ILogger<Handler> logger, CancellationToken cancellationToken)
    {
        logger.LogCall(null);

        var problemDetails = problemDetailsInspector.Validate(request);

        if (problemDetails.IsProblem())
        {
            logger.LogErrors(problemDetails);

            return TypedResults.Problem(problemDetails);
        }

        var incomeInput = request.MapToInput();

        var incomeOutput = await incomeService.CreateIncomeAsync(incomeInput, cancellationToken);

        return incomeOutput.IsSuccess
            ? Response.Created(incomeOutput.Value!)
            : TypedResults.Problem(incomeOutput.Error!.GetProblemDetails());
    }
}

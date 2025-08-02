using AllOverIt.Logging.Extensions;
using Microsoft.AspNetCore.Http.HttpResults;
using Pot.App.Features.Incomes.Update;
using Pot.AspNetCore.Concerns.Validation;
using Pot.AspNetCore.Extensions;
using Pot.AspNetCore.Features.Incomes.Update.Mappings;

namespace Pot.AspNetCore.Features.Incomes.Update;

internal sealed class Handler
{
    public static async Task<Results<Ok<Response>, NotFound, ProblemHttpResult>> Invoke(Request request,
        IUpdateIncomeService incomeService, IProblemDetailsInspector problemDetailsInspector,
        ILogger<Handler> logger, CancellationToken cancellationToken)
    {
        logger.LogCall(null);

        var validationContext = new RequestValidationContext
        {
            NextDue = request.NextDue,
            Frequency = request.Frequency
        };

        var problemDetails = problemDetailsInspector.Validate(request, validationContext);

        if (problemDetails.IsProblem())
        {
            logger.LogErrors(problemDetails);

            return TypedResults.Problem(problemDetails);
        }

        var incomeInput = request.MapToInput();

        var result = await incomeService.UpdateIncomeAsync(incomeInput, cancellationToken);

        return result.IsSuccess
            ? Response.Ok(result.Value!)
            : TypedResults.Problem(result.Error!.GetProblemDetails());
    }
}

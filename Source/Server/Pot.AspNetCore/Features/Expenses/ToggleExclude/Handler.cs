using AllOverIt.Logging.Extensions;
using Microsoft.AspNetCore.Http.HttpResults;
using Pot.App.Features.Expenses.Exclude;
using Pot.AspNetCore.Concerns.Validation;
using Pot.AspNetCore.Extensions;
using Pot.AspNetCore.Features.Expenses.ToggleExclude.Mappings;

namespace Pot.AspNetCore.Features.Expenses.ToggleExclude;

internal sealed class Handler
{
    public static async Task<Results<Ok, ProblemHttpResult>> Invoke(Request request, IExcludeExpensesService expenseService,
        IProblemDetailsInspector problemDetailsInspector, ILogger<Handler> logger, CancellationToken cancellationToken)
    {
        logger.LogCall(null);

        var problemDetails = problemDetailsInspector.Validate(request);

        if (problemDetails.IsProblem())
        {
            logger.LogErrors(problemDetails);

            return TypedResults.Problem(problemDetails);
        }

        var input = request.MapToInput();

        var excludeResult = await expenseService.ToggleExclusionAsync(input, cancellationToken);

        return excludeResult.IsSuccess
            ? TypedResults.Ok()
            : TypedResults.Problem(excludeResult.Error!.GetProblemDetails());
    }
}
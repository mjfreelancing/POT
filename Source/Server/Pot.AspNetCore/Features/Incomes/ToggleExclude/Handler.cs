using AllOverIt.Logging.Extensions;
using Microsoft.AspNetCore.Http.HttpResults;
using Pot.App.Features.Incomes.Exclude;
using Pot.AspNetCore.Concerns.Validation;
using Pot.AspNetCore.Extensions;
using Pot.AspNetCore.Features.Incomes.ToggleExclude.Mappings;

namespace Pot.AspNetCore.Features.Incomes.ToggleExclude;

internal sealed class Handler
{
    public static async Task<Results<Ok, ProblemHttpResult>> Invoke(Request request, IExcludeIncomesService expenseService,
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

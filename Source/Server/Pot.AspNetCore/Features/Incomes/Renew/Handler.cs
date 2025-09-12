using AllOverIt.Logging.Extensions;
using Microsoft.AspNetCore.Http.HttpResults;
using Pot.App.Features.Incomes.Renew;
using Pot.AspNetCore.Concerns.Validation;
using Pot.AspNetCore.Extensions;
using Pot.AspNetCore.Features.Incomes.Renew.Mappings;

namespace Pot.AspNetCore.Features.Incomes.Renew;

internal sealed class Handler
{
    public static async Task<Results<Ok, ProblemHttpResult>> Invoke(Request request, IRenewIncomesService incomesService,
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

        var renewResult = await incomesService.RenewAsync(input, cancellationToken);

        return renewResult.IsSuccess
            ? TypedResults.Ok()
            : TypedResults.Problem(renewResult.Error!.ToProblemDetails());
    }
}

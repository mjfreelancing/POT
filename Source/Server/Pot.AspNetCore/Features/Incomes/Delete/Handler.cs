using AllOverIt.Logging.Extensions;
using Microsoft.AspNetCore.Http.HttpResults;
using Pot.App.Features.Incomes.Delete;
using Pot.AspNetCore.Extensions;
using System.ComponentModel;

namespace Pot.AspNetCore.Features.Incomes.Delete;

internal sealed class Handler
{
    public static async Task<Results<Ok, NotFound, ProblemHttpResult>> Invoke([Description("The income Id.")] Guid id,
        IDeleteIncomeService incomeService, ILogger<Handler> logger, CancellationToken cancellationToken)
    {
        logger.LogCall(null);

        var deletedResult = await incomeService.DeleteIncomeAsync(id, cancellationToken);

        if (deletedResult.IsSuccess)
        {
            return deletedResult.Value
                ? TypedResults.Ok()
                : TypedResults.NotFound();
        }

        return TypedResults.Problem(deletedResult.Error!.ToProblemDetails());
    }
}

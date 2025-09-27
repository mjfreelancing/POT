using AllOverIt.Logging.Extensions;
using Microsoft.AspNetCore.Http.HttpResults;
using Pot.App.Features.Incomes.Get;
using System.ComponentModel;

namespace Pot.AspNetCore.Features.Incomes.Get;

internal sealed class Handler
{
    public static async Task<Results<Ok<Response>, NotFound, ProblemHttpResult>> Invoke([Description("The income Id")] Guid id,
        IGetIncomeService incomeService, ILogger<Handler> logger, CancellationToken cancellationToken)
    {
        logger.LogCall(null);

        var income = await incomeService.GetIncomeAsync(id, cancellationToken);

        return income is null
            ? TypedResults.NotFound()
            : Response.Ok(income);
    }
}

using AllOverIt.Logging.Extensions;
using Microsoft.AspNetCore.Http.HttpResults;
using Pot.Data.Repositories.Incomes;
using System.ComponentModel;

namespace Pot.AspNetCore.Features.Incomes.Get;

using OkGetResult = Ok<Response>;

internal sealed class Handler
{
    public static async Task<Results<OkGetResult, NotFound, ProblemHttpResult>> Invoke(
        [Description("The income Id.")] Guid id,
        IIncomeRepository incomeRepository,
        ILogger<Handler> logger,
        CancellationToken cancellationToken)
    {
        logger.LogCall(null);

        var income = await incomeRepository.GetIncomeOrDefaultAsync(id, cancellationToken);

        return income is null
            ? TypedResults.NotFound()
            : Response.Ok(income);
    }
}

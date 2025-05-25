using AllOverIt.Logging.Extensions;
using Microsoft.AspNetCore.Http.HttpResults;
using Pot.Data.Repositories.Incomes;
using System.ComponentModel;

namespace Pot.AspNetCore.Features.Incomes.Delete;

internal sealed class Handler
{
    public static async Task<Results<Ok, NotFound, ProblemHttpResult>> Invoke([Description("The income Id.")] Guid id,
        IPersistableIncomeRepository incomeRepository, ILogger<Handler> logger, CancellationToken cancellationToken)
    {
        logger.LogCall(null);

        var income = await incomeRepository.GetIncomeOrDefaultAsync(id, cancellationToken);

        if (income is null)
        {
            return TypedResults.NotFound();
        }

        incomeRepository.Delete(income);

        await incomeRepository.SaveAsync(cancellationToken);

        return TypedResults.Ok();
    }
}

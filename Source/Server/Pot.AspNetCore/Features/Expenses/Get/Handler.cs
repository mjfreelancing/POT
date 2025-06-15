using AllOverIt.Logging.Extensions;
using Microsoft.AspNetCore.Http.HttpResults;
using Pot.App.Features.Expenses.Get;
using System.ComponentModel;

namespace Pot.AspNetCore.Features.Expenses.Get;

using OkGetResult = Ok<Response>;

internal sealed class Handler
{
    public static async Task<Results<OkGetResult, NotFound, ProblemHttpResult>> Invoke([Description("The Expense Id.")] Guid id,
        IGetExpenseService expenseService, ILogger<Handler> logger, CancellationToken cancellationToken)
    {
        logger.LogCall(null);

        var expense = await expenseService.GetExpenseAsync(id, cancellationToken);

        return expense is null
            ? TypedResults.NotFound()
            : Response.Ok(expense);
    }
}

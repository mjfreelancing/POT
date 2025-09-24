using AllOverIt.Logging.Extensions;
using Microsoft.AspNetCore.Http.HttpResults;
using Pot.App.Features.Expenses.Delete;
using Pot.AspNetCore.Extensions;
using System.ComponentModel;

namespace Pot.AspNetCore.Features.Expenses.Delete;

internal sealed class Handler
{
    public static async Task<Results<Ok, ProblemHttpResult>> Invoke([Description("The Expense Id")] Guid id,
        IDeleteExpenseService expenseService, ILogger<Handler> logger, CancellationToken cancellationToken)
    {
        logger.LogCall(null);

        var result = await expenseService.DeleteExpenseAsync(id, cancellationToken);

        return result.IsSuccess
            ? TypedResults.Ok()
            : TypedResults.Problem(result.Error!.ToProblemDetails());
    }
}

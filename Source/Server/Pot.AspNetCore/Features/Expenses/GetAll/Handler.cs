using AllOverIt.Logging.Extensions;
using Microsoft.AspNetCore.Http.HttpResults;
using Pot.App.Features.Expenses.GetAll;

namespace Pot.AspNetCore.Features.Expenses.GetAll;

internal sealed class Handler
{
    public static async Task<Ok<Response[]>> Invoke(IGetExpensesService expenseService, ILogger<Handler> logger,
        CancellationToken cancellationToken)
    {
        logger.LogCall(null);

        var expenses = await expenseService.GetAllExpensesAsync(cancellationToken);

        return Response.Ok(expenses);
    }
}

using AllOverIt.Logging.Extensions;
using Pot.Data.Repositories.Expenses;

namespace Pot.AspNetCore.Features.Expenses.GetAll;

internal sealed class Handler
{
    public static async Task<IResult> Invoke(IExpenseRepository expenseRepository, ILogger<Handler> logger,
        CancellationToken cancellationToken)
    {
        logger.LogCall(null);

        var expenses = await expenseRepository.GetAllAsync(cancellationToken);

        return Response.Ok(expenses);
    }
}

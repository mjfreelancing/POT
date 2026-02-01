using AllOverIt.Logging.Extensions;
using Microsoft.AspNetCore.Http.HttpResults;
using Pot.App.Features.Expenses.GetAll;
using Pot.AspNetCore.Models;

namespace Pot.AspNetCore.Features.Expenses.GetAll;

using OkGetAllResult = Ok<PagedResponse<Response>>;

internal sealed class Handler
{
    public static async Task<Results<OkGetAllResult, ProblemHttpResult>> Invoke(Request request,
        IGetExpensesService expenseService, ILogger<Handler> logger, CancellationToken cancellationToken)
    {
        logger.LogCall(null);

        var incomes = await expenseService.GetAllExpensesAsync(request.Paging, cancellationToken);

        return Response.Ok(incomes);
    }
}

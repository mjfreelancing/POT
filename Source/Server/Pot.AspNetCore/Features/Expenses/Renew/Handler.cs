using AllOverIt.Logging.Extensions;
using Microsoft.AspNetCore.Http.HttpResults;
using Pot.App.Features.Expenses.Renew;
using Pot.AspNetCore.Extensions;
using Pot.AspNetCore.Features.Expenses.Renew.Mappings;

namespace Pot.AspNetCore.Features.Expenses.Renew;

internal sealed class Handler
{
    public static async Task<Results<Ok, ProblemHttpResult>> Invoke(Request request,
        IRenewExpensesService expenseService, ILogger<Handler> logger, CancellationToken cancellationToken)
    {
        logger.LogCall(null);

        var input = request.MapToInput();

        var renewResult = await expenseService.RenewAsync(input, cancellationToken);

        return renewResult.IsSuccess
            ? TypedResults.Ok()
            : TypedResults.Problem(renewResult.Error!.GetProblemDetails());
    }
}

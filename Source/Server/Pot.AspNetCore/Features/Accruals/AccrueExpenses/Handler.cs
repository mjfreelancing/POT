using AllOverIt.Logging.Extensions;
using Microsoft.AspNetCore.Http.HttpResults;
using Pot.App.Features.Accounts.AccrueExpenses;
using Pot.AspNetCore.Extensions;
using Pot.AspNetCore.Features.Accruals.AccrueExpenses.Mappings;

namespace Pot.AspNetCore.Features.Accruals.AccrueExpenses;

internal sealed class Handler
{
    public static async Task<Results<Ok, ProblemHttpResult>> Invoke(Request request,
        IAccrueExpensesService accrueExpensesService, ILogger<Handler> logger, CancellationToken cancellationToken)
    {
        logger.LogCall(null);

        var input = request.MapToInput();

        var accrueResult = await accrueExpensesService.AccrueAsync(input, cancellationToken);

        return accrueResult.IsSuccess
            ? TypedResults.Ok()
            : TypedResults.Problem(accrueResult.Error!.ToProblemDetails());
    }
}

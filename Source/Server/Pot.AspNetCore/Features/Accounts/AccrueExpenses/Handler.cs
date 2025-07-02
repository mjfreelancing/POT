using AllOverIt.Logging.Extensions;
using Microsoft.AspNetCore.Http.HttpResults;
using Pot.App.Features.Accounts.AccrueExpenses;
using Pot.AspNetCore.Extensions;
using Pot.AspNetCore.Features.Accounts.AccrueExpenses.Mappings;

namespace Pot.AspNetCore.Features.Accounts.AccrueExpenses;

internal sealed class Handler
{
    // TODO: Review all endpoints. In this case, NotFound is returned as a ProblemDetail which is different to the other endpoints, such as GET.
    public static async Task<Results<Ok, /*NotFound,*/ ProblemHttpResult>> Invoke(Request request,
        IAccrueExpensesService accrueExpensesService, ILogger<Handler> logger, CancellationToken cancellationToken)
    {
        logger.LogCall(null);

        var input = request.MapToInput();

        var accrueResult = await accrueExpensesService.AccrueAsync(input, cancellationToken);

        return accrueResult.IsSuccess
            ? TypedResults.Ok()
            : TypedResults.Problem(accrueResult.Error!.GetProblemDetails());
    }
}

using AllOverIt.Logging.Extensions;
using Microsoft.AspNetCore.Http.HttpResults;
using Pot.App.Features.Incomes.Renew;
using Pot.AspNetCore.Extensions;
using Pot.AspNetCore.Features.Incomes.Renew.Mappings;

namespace Pot.AspNetCore.Features.Incomes.Renew;

internal sealed class Handler
{
    public static async Task<Results<Ok, ProblemHttpResult>> Invoke(Request request,
        IRenewIncomesService incomesService, ILogger<Handler> logger, CancellationToken cancellationToken)
    {
        logger.LogCall(null);

        var input = request.MapToInput();

        var renewResult = await incomesService.RenewAsync(input, cancellationToken);

        return renewResult.IsSuccess
            ? TypedResults.Ok()
            : TypedResults.Problem(renewResult.Error!.GetProblemDetails());
    }
}

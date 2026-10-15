using AllOverIt.Logging.Extensions;
using Microsoft.AspNetCore.Http.HttpResults;
using Pot.App.Features.Incomes.GetAll;

namespace Pot.AspNetCore.Features.Incomes.GetAll;

internal sealed class Handler
{
    public static async Task<Ok<Response[]>> Invoke(IGetAllIncomesService incomeService, ILogger<Handler> logger,
        CancellationToken cancellationToken)
    {
        logger.LogCall(null);

        var incomes = await incomeService.GetAllIncomesAsync(cancellationToken);

        return Response.Ok(incomes);
    }
}
